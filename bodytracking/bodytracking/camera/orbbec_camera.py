import pyorbbecsdk as ob
import time
from bodytracking import global_vars
from bodytracking.orbbec_utils.utils import frame_to_bgr_image
from bodytracking.camera.base_camera import Camera


class OrbbecCamera(Camera):
    counter = 0
    timer = 0.0
    pipeline = None


    def __init__(self):
        ctx = ob.Context()
        devices = ctx.query_devices()
        if len(devices) == 0:
            raise RuntimeError("No Orbbec camera found")

        self.pipeline = ob.Pipeline()
        config = ob.Config()
        profile_list = self.pipeline.get_stream_profile_list(ob.OBSensorType.COLOR_SENSOR)

        color_profile = profile_list.get_default_video_stream_profile()

        config.enable_stream(color_profile)
        self.pipeline.start(config)
        time.sleep(1)
        print("Opened Orbbec Capture @ %s fps" % str(global_vars.FPS))


    def get_frame(self):


        frameset = self.pipeline.wait_for_frames(100)
        if frameset is None:
            self.get_frame()

        if global_vars.DEBUG:
            self.counter += 1
            if time.time() - self.timer >= 3:
                # print("Capture FPS: ", self.counter / (time.time() - self.timer))
                self.counter = 0
                self.timer = time.time()
        color_frame = frameset.get_color_frame()
        if color_frame is not None:
            return  frame_to_bgr_image(color_frame)
        else:
            return None



    def stop(self):
        self.pipeline.stop()

    def width(self):
        return 1080

    def height(self):
        return 1920