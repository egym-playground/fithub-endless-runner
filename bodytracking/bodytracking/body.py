import numpy as np
import pyorbbecsdk as ob
from bodytracking.orbbec_utils.utils import frame_to_bgr_image
from ultralytics import YOLO
import threading
import time
import global_vars
import struct
import cv2
from bodytracking.rendering import render_results
from websocket_server import WebSocketServer

from bodytracking.bodytracking_evaluation import evaluate_directions

class CaptureThread(threading.Thread):
    pipeline = None
    ret = None
    frame = None
    isRunning = False
    counter = 0
    timer = 0.0

    def run(self):
        ctx = ob.Context()
        devices = ctx.query_devices()
        if len(devices) == 0:
            raise RuntimeError("No Orbbec camera found")
        # dev = devices.get[0]
        self.pipeline = ob.Pipeline()
        config = ob.Config()
        profile_list = self.pipeline.get_stream_profile_list(ob.OBSensorType.COLOR_SENSOR)

        color_profile = profile_list.get_default_video_stream_profile()

        config.enable_stream(color_profile)
        self.pipeline.start(config)

        time.sleep(1)
        print("Opened Orbbec Capture @ %s fps" % str(global_vars.FPS))

        while not global_vars.KILL_THREADS:
            frameset = self.pipeline.wait_for_frames(100)
            if frameset is None:
                continue

            color_frame = frameset.get_color_frame()
            if color_frame is not None:
                self.frame = frame_to_bgr_image(color_frame)
                self.ret = True
                self.isRunning = True
            else:
                self.ret = False

            if global_vars.DEBUG:
                self.counter += 1
                if time.time() - self.timer >= 3:
                    # print("Capture FPS: ", self.counter / (time.time() - self.timer))
                    self.counter = 0
                    self.timer = time.time()

        self.pipeline.stop()


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

        while not global_vars.KILL_THREADS and capture.pipeline is not None:
            ti = time.time()

            ret = capture.ret
            image = capture.frame

            if image is None:
                 continue

            results = model(image, verbose=False, device=device)

            # TODO evaluate direction
            directions = evaluate_directions(results ,image.shape)

            render_results(ti, results, directions)

            if self.pipe is not None:
                self.data = ""

            relevant_directions = [direction_name for direction_name, valid in directions.items() if valid]
            self.websocket_server.notify_new_frame(relevant_directions)

        if global_vars.DEBUG:
            cv2.destroyAllWindows()
