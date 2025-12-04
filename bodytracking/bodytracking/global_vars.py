# Internally used, don't mind this.
KILL_THREADS = False

# Toggle this in order to view how your WebCam is being interpreted (reduces performance).
DEBUG = False

# To switch cameras. Sometimes takes a while.
WEBCAM_INDEX = 0

# Settings do not universally apply, not all WebCams support all frame rates and resolutions
USE_CUSTOM_CAM_SETTINGS = False
FPS = 60
WIDTH = 1080
HEIGHT = 1920

# [0, 2] Higher numbers are more precise, but also cost more performance. Good environment conditions = 1, otherwise 2.
MODEL_COMPLEXITY = 1

USE_GPU = True

PERSON_MINIMUM_THRESHOLD = 0.8

def detect_camera():
    """Automatically detect which camera is available (Orbbec or Kinect)."""
    # Try Orbbec first
    try:
        import pyorbbecsdk as ob
        ctx = ob.Context()
        devices = ctx.query_devices()
        if len(devices) > 0:
            return "orbbec"
    except (ImportError, RuntimeError):
        pass

    return "kinect"
camera_name = detect_camera()
print("Detected camera: %s" % camera_name)
if camera_name == "orbbec":
    USE_ORBBEC = True
else:
    USE_ORBBEC = False

