from bodytracking.camera.base_camera import Camera
from bodytracking import global_vars
if not global_vars.USE_ORBBEC:
    import pykinect_azure as pykinect
class KinectCamera(Camera):
    def __init__(self):
        # Initialize Kinect camera
        pykinect.initialize_libraries(module_k4a_path="/opt/egym/fithub-app/usr/lib/libk4a.so.1.4", module_k4abt_path="/opt/egym/fithub-app/usr/lib/libk4abt.so.1.1 ")

        device_config = pykinect.default_configuration
        device_config.color_resolution = pykinect.K4A_COLOR_RESOLUTION_1080P
        device_config.depth_mode = pykinect.K4A_DEPTH_MODE_OFF
        self._kinect = pykinect.start_device(config=device_config)
        global_vars.WIDTH = self.width()
        global_vars.HEIGHT = self.height()

    def get_frame(self):
        # Return a frame from the Kinect camera
        capture = self._kinect.update()
        ret, color_image = capture.get_color_image()
        if not ret:
            return None
        return color_image

    def height(self):
        return 1080

    def width(self):
        return 1920

    def stop(self):
        pass