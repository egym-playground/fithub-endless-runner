import numpy as np
from bodytracking import global_vars
from state_tracker import StateTracker
from pose_state import PoseState
from height_measure import HeightMeasure

DEFAULT_JUMP_VELOCITY = 15
DEFAULT_HORIZONTAL_THRESHOLD = 200
DEFAULT_SQUAT_THRESHOLD = 300
DEFAULT_START_THRESHOLD = 200  # Minimum distance wrists must be above shoulders

class DirectionEvaluator:
    def __init__(self):
        # Store previous positions for movement detection

        # Thresholds (normalized coordinates, range approximately -0.5 to 0.5)
        self.JUMP_COOLDOWN_FRAMES = 10  # Frames to wait before detecting another jump
        self.MIN_CONFIDENCE = global_vars.PERSON_MINIMUM_THRESHOLD  # Minimum detection confidence
        self.START_RATIO = 0.2  # Start gesture threshold ratio
        self.SQUAT_DEPTH_RATIO = 0.25  # Squat depth should be 25% of person height
        self.HORIZONTAL_RATIO = 1.25

        # Smoothing window
        self.position_history = []
        self.history_size = 3
        self.previous_direction = None
        self.previous_avg_hip = None

        # Jump detection state
        self.jump_cooldown = 0
        self.baseline_hip_y = None  # Store resting hip height
        self.baseline_frames = []
        self.baseline_window = 30  # Frames to establish baseline

        self.state_tracker = StateTracker()
        self.keypoints = None  # type: PoseState
        self.height_measure = HeightMeasure()

    @staticmethod
    def get_dynamic_jump_velocity():
        """Get jump threshold based on person's height"""
        return DEFAULT_JUMP_VELOCITY  # Fallback default

    def get_dynamic_squat_threshold(self):
        """Get squat threshold based on person's height"""
        if self.height_measure.get_height() > 300:
            return self.height_measure.get_height() * self.SQUAT_DEPTH_RATIO
        return DEFAULT_SQUAT_THRESHOLD

    def get_dynamic_start_threshold(self):
        """Get start gesture threshold based on person's height"""
        if self.height_measure.get_height() > 300:
            return self.height_measure.get_height() * self.START_RATIO  # 20% of height
        return DEFAULT_START_THRESHOLD

    def get_dynamic_horizontal(self):
        if self.height_measure.get_width() > 10:
            return self.height_measure.get_width() * self.HORIZONTAL_RATIO  # 20% of height
        return DEFAULT_HORIZONTAL_THRESHOLD

    def _get_largest_person_with_confidence(self, results):
        """
        Find the largest person detection with confidence >= 80%.

        Returns:
            int or None: Index of the largest valid person, or None if no valid person found
        """
        if len(results[0].boxes) == 0:
            return None

        boxes = results[0].boxes
        valid_indices = []

        for idx in range(len(boxes)):
            # Check confidence (box confidence for person detection)
            if boxes.conf[idx].cpu().numpy() >= self.MIN_CONFIDENCE:
                valid_indices.append(idx)

        if not valid_indices:
            return None

        # Find the largest bounding box among valid detections
        largest_idx = None
        largest_area = 0

        for idx in valid_indices:
            box = boxes.xyxy[idx].cpu().numpy()
            area = (box[2] - box[0]) * (box[3] - box[1])

            if area > largest_area:
                largest_area = area
                largest_idx = idx

        return largest_idx

    def _detect_horizontal_movement(self, avg_hip):
        """Detect left/right movement based on hip position change."""

        horizontal_delta = global_vars.WIDTH / 2 - avg_hip[0]

        moved_right = horizontal_delta > self.get_dynamic_horizontal()
        moved_left = horizontal_delta < -self.get_dynamic_horizontal()

        return moved_left, moved_right

    def _detect_start(self):
        """Detect start gesture when both wrists are above shoulders by threshold."""
        if self.keypoints.left_wrist is None or self.keypoints.right_wrist is None or self.keypoints.left_shoulder is None or self.keypoints.right_shoulder is None:
            return False

        # Check if both wrists are above their respective shoulders by at least threshold (Y increases downward)
        left_distance = self.keypoints.left_shoulder[1] - self.keypoints.left_wrist[1]
        right_distance = self.keypoints.right_shoulder[1] - self.keypoints.right_wrist[1]

        left_above = left_distance > self.get_dynamic_start_threshold()
        right_above = right_distance > self.get_dynamic_jump_velocity()

        return left_above and right_above

    def _check_feet_elevation(self, left_ankle, right_ankle):
        """
        Check if feet are elevated (moving upward or off ground).

        Returns:
            bool: True if at least one foot shows upward movement
        """
        if not hasattr(self, 'previous_ankles'):
            self.previous_ankles = {'left': left_ankle, 'right': right_ankle}
            return False

        prev_left = self.previous_ankles.get('left')
        prev_right = self.previous_ankles.get('right')

        feet_moving_up = False

        # Check left foot upward movement
        if left_ankle is not None and prev_left is not None:
            left_velocity = prev_left[1] - left_ankle[1]  # Positive = moving up
            if left_velocity > self.get_dynamic_jump_velocity() * 0.5:  # 50% of hip threshold
                feet_moving_up = True

        # Check right foot upward movement
        if right_ankle is not None and prev_right is not None:
            right_velocity = prev_right[1] - right_ankle[1]
            if right_velocity > self.get_dynamic_jump_velocity():
                feet_moving_up = True

        return feet_moving_up

    def _detect_jump(self, current_hip):
        """
        Detect jump using velocity-based approach with baseline comparison and foot verification.

        A jump is detected when:
        1. Hip moves upward faster than threshold velocity
        2. Hip is significantly above the established baseline
        3. At least one foot is elevated (both feet off ground or moving upward)
        4. Cooldown period has passed
        """
        if self.jump_cooldown > 0:
            self.jump_cooldown -= 1
            return False

        # Require valid ankle keypoints
        if self.keypoints.left_ankle is None and self.keypoints.right_ankle is None:
            return False

        # Establish baseline (resting hip height)
        self.baseline_frames.append(current_hip[1])
        if len(self.baseline_frames) > self.baseline_window:
            self.baseline_frames.pop(0)

        if len(self.baseline_frames) >= 5:
            self.baseline_hip_y = np.median(self.baseline_frames)
        else:
            return False

        # Calculate hip velocity
        if self.previous_avg_hip is None:
            self.previous_avg_hip = current_hip
            self.previous_ankles = {'left': self.keypoints.left_ankle, 'right': self.keypoints.right_ankle}
            return False

        vertical_velocity = self.previous_avg_hip[1] - current_hip[1]
        distance_from_baseline = self.baseline_hip_y - current_hip[1]

        # Check foot elevation/movement
        feet_elevated = self._check_feet_elevation(self.keypoints.left_ankle, self.keypoints.right_ankle)

        # Detect jump: hip velocity + baseline distance + feet moving
        is_jumping = (
                vertical_velocity > self.get_dynamic_jump_velocity() and
                distance_from_baseline > 50 and
                feet_elevated
        )

        if is_jumping:
            self.jump_cooldown = self.JUMP_COOLDOWN_FRAMES
            self.baseline_frames = []

        self.previous_avg_hip = current_hip.copy()
        self.previous_ankles = {'left': self.keypoints.left_ankle, 'right': self.keypoints.right_ankle}

        return is_jumping

    def _detect_squat(self, avg_hip, avg_shoulder):
        """Detect squat based on hip-to-shoulder distance reduction."""

        current_distance = global_vars.HEIGHT / 2 - avg_hip[1]

        return current_distance < -self.get_dynamic_squat_threshold()

    def _reset_history(self):
        """Reset position history."""
        self.position_history = []

    def evaluate(self, results):
        """
        Evaluate movement direction based on pose keypoints.

        Returns dict with keys: 'left', 'right', 'jump', 'slide'
        Each value is True if movement detected in that direction.
        """
        directions = {
            'left': False,
            'right': False,
            'jump': False,
            'slide': False,
            'start': False
        }

        if len(results[0].keypoints) == 0:
            return directions

        # Get the largest person with at least 80% confidence
        person_idx = self._get_largest_person_with_confidence(results)

        if person_idx is None:
            # Reset history if no valid person detected
            self._reset_history()
            return directions

        keypoints = results[0].keypoints[person_idx]

        self.keypoints = PoseState(keypoints)

        if not self.keypoints.main_kps_available():
            return directions

        # Calculate centers in normalized coordinates
        hip_center = (self.keypoints.left_hip + self.keypoints.right_hip) / 2
        shoulder_center = (self.keypoints.left_shoulder + self.keypoints.right_shoulder) / 2

        if hip_center is None or shoulder_center is None:
            return directions

        # Smooth position using history
        self.position_history.append({
            'hip': hip_center,
            'shoulder': shoulder_center
        })
        if len(self.position_history) > self.history_size:
            self.position_history.pop(0)

        # Use average of history for stability
        avg_hip = np.mean([p['hip'] for p in self.position_history], axis=0)
        avg_shoulder = np.mean([p['shoulder'] for p in self.position_history], axis=0)

        # Detect movements using separate functions
        moved_left, moved_right = self.state_tracker.update(*self._detect_horizontal_movement(avg_hip))
        if moved_left is not None:
            directions['left'] = moved_left
            directions['right'] = moved_right

        directions['jump'] = self._detect_jump(hip_center)
        directions['slide'] = self._detect_squat(avg_hip, avg_shoulder)
        directions['start'] = self._detect_start()

        self.height_measure.update_height(directions, self.keypoints)

        return self._get_directions_historic(directions)

    def _get_directions_historic(self, directions):
        new_directions = {}
        for direction_name, is_active in directions.items():
            if is_active and (self.previous_direction != direction_name):
                new_directions[direction_name] = True
            else:
                new_directions[direction_name] = False

        # Update previous direction (only store if a direction is active)
        active_directions = [name for name, active in new_directions.items() if active]
        if active_directions:
            self.previous_direction = active_directions[0]  # Store first active direction
        elif not any(directions.values()):
            # Reset if no movement detected
            self.previous_direction = None

        return new_directions


# Global evaluator instance
_evaluator = DirectionEvaluator()


def evaluate_directions(results):
    """
    Evaluate movement directions from YOLO pose results.

    Args:
        results: YOLOv8 pose detection results

    Returns:
        dict: Direction flags {'left': bool, 'right': bool, 'jump': bool, 'slide': bool}
    """
    return _evaluator.evaluate(results)
