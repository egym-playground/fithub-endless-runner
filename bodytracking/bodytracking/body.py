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
                    print("Capture FPS: ", self.counter / (time.time() - self.timer))
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
        model = YOLO("yolov8n-pose.pt")  # Use 'yolov8s-pose.pt' or 'yolov8m-pose.pt' for better accuracy

        # Enable GPU if available
        device = "cuda:0" if global_vars.USE_GPU else "cpu"
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

            # YOLOv8 inference
            # data = np.asanyarray(frame.get_data())
            # image = data.reshape((global_vars.HEIGHT, global_vars.WIDTH, 3))
            if image is None:
                continue
            results = model(image, verbose=False, device=device)

            # Rendering results
            render_results(ti, results)

            # TODO evaluate direction

            self.websocket_server.notify_new_frame("jump")

            if self.pipe is None and time.time() - self.timeSinceCheckedConnection >= 1:
                try:
                    self.pipe = open(r"\\.\pipe\UnityMediaPipeBody", "r+b", 0)
                except FileNotFoundError:
                    print("Waiting for Unity project to run...")
                    self.pipe = None
                self.timeSinceCheckedConnection = time.time()

            if self.pipe is not None:
                self.data = ""

                # Extract keypoints from YOLOv8 results
                if len(results[0].keypoints) > 0:
                    keypoints = results[0].keypoints[0]  # Get first person

                    if keypoints.xy is not None:
                        xy = keypoints.xy.cpu().numpy()  # 2D coordinates (x, y)
                        conf = keypoints.conf.cpu().numpy()  # Confidence scores

                        # YOLOv8 doesn't provide world coordinates directly
                        # Send 2D normalized coordinates
                        for i in range(len(xy)):
                            if conf[i] > 0.5:  # Confidence threshold
                                x_norm = (xy[i][0] / image.shape[1]) - 0.5
                                y_norm = (xy[i][1] / image.shape[0]) - 0.5
                                z_norm = 0  # No depth from single camera

                                self.data += "ANCHORED|{}|{}|{}|{}\n".format(i, x_norm, y_norm, z_norm)

                s = self.data.encode("utf-8")
                try:
                    self.pipe.write(struct.pack("I", len(s)) + s)
                    self.pipe.seek(0)
                except Exception:
                    print("Failed to write to pipe. Is the unity project open?")
                    self.pipe = None

        if self.pipe:
            self.pipe.close()
        if global_vars.DEBUG:
            cv2.destroyAllWindows()
