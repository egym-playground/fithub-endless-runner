import cv2
import time
from bodytracking import global_vars
timeSincePostStatistics = 0

def render_results(ti ,results):
    global timeSincePostStatistics
    tf = time.time()

    if global_vars.DEBUG:
        if time.time() - timeSincePostStatistics >= 1:
            print("Theoretical Maximum FPS: %f" % (1 / (tf - ti)))
            timeSincePostStatistics = time.time()

        # Draw keypoints on image
        try:
            annotated_frame = results[0].plot()
            cv2.imshow('Body Tracking', annotated_frame)
            cv2.waitKey(1)
        except Exception as e:
            print(f"Cannot display window (headless environment): {e}")
            global_vars.DEBUG = False  # Disable further display attempts
