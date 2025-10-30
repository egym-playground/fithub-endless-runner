import cv2
import time
from bodytracking import global_vars
timeSincePostStatistics = 0

def render_results(ti ,results, directions):
    global timeSincePostStatistics
    tf = time.time()

    if global_vars.DEBUG:
        if time.time() - timeSincePostStatistics >= 1:
            timeSincePostStatistics = time.time()

        # Draw keypoints on image
        try:
            annotated_frame = results[0].plot()
            # add title for any key that is true in directions
            for direction, detected in directions.items():
                if not detected:
                    continue
                cv2.putText(annotated_frame, f"Movement: {direction}", (10, 30 + 30 * list(directions.keys()).index(direction)),
                                cv2.FONT_HERSHEY_SIMPLEX, 1, (0, 0, 255), 2)
            cv2.imshow('Body Tracking', annotated_frame)
            cv2.waitKey(1)
        except Exception as e:
            print(f"Cannot display window (headless environment): {e}")
            global_vars.DEBUG = False  # Disable further display attempts
