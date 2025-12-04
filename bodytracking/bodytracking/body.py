from ultralytics import YOLO
import threading
import time
import global_vars
import cv2

from bodytracking.camera.orbbec_camera import OrbbecCamera
from bodytracking.camera.kinect_camera import KinectCamera
from bodytracking.rendering import render_results
from websocket_server import WebSocketServer

from bodytracking.bodytracking_evaluation import evaluate_directions
class CaptureThread(threading.Thread):
    ret = None
    isRunning = False
    frame = None
    camera = None

    def running(self):
        return self.isRunning

    def run(self):

        # dev = devices.get[0]
        if global_vars.USE_ORBBEC:
            self.camera = OrbbecCamera()
        else:
            self.camera = KinectCamera()

        time.sleep(1)
        print("Opened Capture @ %s fps" % str(global_vars.FPS))

        while not global_vars.KILL_THREADS:
            color_frame = self.camera.get_frame()
            if color_frame is not None:
                self.frame = color_frame
                self.ret = True
                self.isRunning = True
            else:
                self.ret = False
        self.camera.stop()


class BodyThread(threading.Thread):
    data = ""
    pipe = None
    timeSinceCheckedConnection = 0
    websocket_server = None

    # YOLO-Pose uses 17 keypoints (COCO format) vs MediaPipe's 33
    # Mapping YOLO keypoints to match Unity expectations
    YOLO_KEYPOINT_NAMES = [
        "nose",
        "left_eye",
        "right_eye",
        "left_ear",
        "right_ear",
        "left_shoulder",
        "right_shoulder",
        "left_elbow",
        "right_elbow",
        "left_wrist",
        "right_wrist",
        "left_hip",
        "right_hip",
        "left_knee",
        "right_knee",
        "left_ankle",
        "right_ankle",
    ]

    def run(self):
        # Load YOLOv8-pose model (it will download on first run)
        model = YOLO('yolov8n-pose.pt')  # Use 'yolov8s-pose.pt' or 'yolov8m-pose.pt' for better accuracy

        # Enable GPU if available
        device = 'cuda:0' if global_vars.USE_GPU else 'cpu'
        model.to(device)

        # Start WebSocket server
        self.websocket_server = WebSocketServer()
        self.websocket_server.start()

        capture = CaptureThread()
        capture.start()

        while not global_vars.KILL_THREADS and capture.isRunning == False:
            print("Waiting for camera and capture thread.")
            time.sleep(0.5)
        print("Beginning capture")

        while not global_vars.KILL_THREADS and capture.isRunning is not None:
            ti = time.time()

            ret = capture.ret
            image = capture.frame

            if image is None:
                 continue

            results = model(image, verbose=False, device=device)

            # TODO evaluate direction
            directions = evaluate_directions(results)

            render_results(ti, results, directions)

            if self.pipe is not None:
                self.data = ""

            relevant_directions = [direction_name for direction_name, valid in directions.items() if valid]
            self.websocket_server.notify_new_frame(relevant_directions)

        if global_vars.DEBUG:
            cv2.destroyAllWindows()
