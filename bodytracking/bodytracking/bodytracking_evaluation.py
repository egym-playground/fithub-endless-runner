import numpy as np
from bodytracking import global_vars
from enum import Enum


class PositionState(Enum):
    LEFT = "left"
    CENTER = "center"
    RIGHT = "right"


class StateTracker:
    """Track player's horizontal position state."""

    def __init__(self):
        self.current_state = PositionState.CENTER
        self.previous_state = PositionState.CENTER

    def update(self, move_left, move_right):
        """
        Update state based on current hip position.

        Returns:
            tuple: (moved_left, moved_right) - signals to send
        """
        match(bool(move_right), bool(move_left), self.current_state):
            case (True, False, PositionState.CENTER):
                self.current_state = PositionState.RIGHT
                moved_left = False
                moved_right = True
            case (False, True, PositionState.CENTER):
                self.current_state = PositionState.LEFT
                moved_left = True
                moved_right = False
            case (False, False, PositionState.LEFT):
                self.current_state = PositionState.CENTER
                moved_left = False
                moved_right = True
            case (False, False, PositionState.RIGHT):
                self.current_state = PositionState.CENTER
                moved_left = True
                moved_right = False
            case _:
                moved_left = False
                moved_right = False

        return moved_left, moved_right

class JointState:
    def __init__(self):
        self.previous_positions = {}


class DirectionEvaluator:
    def __init__(self):
        # Store previous positions for movement detection

        # Thresholds (normalized coordinates, range approximately -0.5 to 0.5)
        self.HORIZONTAL_THRESHOLD = 200  # Movement threshold for left/right
        self.VERTICAL_JUMP_VELOCITY = 15  # Minimum upward velocity (pixels/frame)
        self.JUMP_COOLDOWN_FRAMES = 10  # Frames to wait before detecting another jump
        self.SQUAT_THRESHOLD = 200  # Hip-to-shoulder ratio change for squat
        self.MIN_CONFIDENCE = global_vars.PERSON_MINIMUM_THRESHOLD  # Minimum detection confidence
        self.START_THRESHOLD = 200  # Minimum distance wrists must be above shoulders

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

    @staticmethod
    def _get_keypoint(keypoints, index):
        """Extract keypoint coordinates safely."""
        if keypoints.xy is not None and len(keypoints.xy) > 0:
            xy = keypoints.xy[0].cpu().numpy()
            conf = keypoints.conf[0].cpu().numpy()
            if index < len(xy) and conf[index] > 0.5:
                return xy[index]
        return None

    @staticmethod
    def _normalize_coords(point, image_shape):
        """Normalize coordinates to -0.5 to 0.5 range."""
        if point is None:
            return None
        x_norm = (point[0] / image_shape[1]) - 0.5
        y_norm = (point[1] / image_shape[0]) - 0.5
        return np.array([x_norm, y_norm])

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

        # Find largest bounding box among valid detections
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

        horizontal_delta = global_vars.WIDTH/2 - avg_hip[0]

        moved_right = horizontal_delta > self.HORIZONTAL_THRESHOLD
        moved_left = horizontal_delta < -self.HORIZONTAL_THRESHOLD

        return moved_left, moved_right


    def _detect_start(self, left_wrist, right_wrist, left_shoulder, right_shoulder):
        """Detect start gesture when both wrists are above shoulders by threshold."""
        if left_wrist is None or right_wrist is None or left_shoulder is None or right_shoulder is None:
            return False

        # Check if both wrists are above their respective shoulders by at least threshold (Y increases downward)
        left_distance = left_shoulder[1] - left_wrist[1]
        right_distance = right_shoulder[1] - right_wrist[1]

        left_above = left_distance > self.START_THRESHOLD
        right_above = right_distance > self.START_THRESHOLD

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
            if left_velocity > self.VERTICAL_JUMP_VELOCITY * 0.5:  # 50% of hip threshold
                feet_moving_up = True

        # Check right foot upward movement
        if right_ankle is not None and prev_right is not None:
            right_velocity = prev_right[1] - right_ankle[1]
            if right_velocity > self.VERTICAL_JUMP_VELOCITY * 0.5:
                feet_moving_up = True

        return feet_moving_up

    def _detect_jump(self, current_hip, left_ankle, right_ankle):
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
        if left_ankle is None and right_ankle is None:
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
            self.previous_ankles = {'left': left_ankle, 'right': right_ankle}
            return False

        vertical_velocity = self.previous_avg_hip[1] - current_hip[1]
        distance_from_baseline = self.baseline_hip_y - current_hip[1]

        # Check foot elevation/movement
        feet_elevated = self._check_feet_elevation(left_ankle, right_ankle)

        # Detect jump: hip velocity + baseline distance + feet moving
        is_jumping = (
                vertical_velocity > self.VERTICAL_JUMP_VELOCITY and
                distance_from_baseline > 50 and
                feet_elevated
        )

        if is_jumping:
            self.jump_cooldown = self.JUMP_COOLDOWN_FRAMES
            self.baseline_frames = []

        self.previous_avg_hip = current_hip.copy()
        self.previous_ankles = {'left': left_ankle, 'right': right_ankle}

        return is_jumping

    def _detect_squat(self, avg_hip, avg_shoulder):
        """Detect squat based on hip-to-shoulder distance reduction."""

        current_distance = global_vars.HEIGHT/2 - avg_hip[1]

        return current_distance < -self.SQUAT_THRESHOLD

    def reset_history(self):
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
            self.reset_history()
            return directions

        keypoints = results[0].keypoints[person_idx]

        # Key indices for YOLO COCO format (17 keypoints):
        # 11: left_hip, 12: right_hip, 5: left_shoulder, 6: right_shoulder
        left_hip = self._get_keypoint(keypoints, 11)
        right_hip = self._get_keypoint(keypoints, 12)
        left_shoulder = self._get_keypoint(keypoints, 5)
        right_shoulder = self._get_keypoint(keypoints, 6)
        left_wrist = self._get_keypoint(keypoints, 9)
        right_wrist = self._get_keypoint(keypoints, 10)
        left_ankle = self._get_keypoint(keypoints, 15)
        right_ankle = self._get_keypoint(keypoints, 16)

        if left_hip is None or right_hip is None or left_shoulder is None or right_shoulder is None:
            return directions

        # Calculate centers in normalized coordinates
        hip_center = (left_hip + right_hip) / 2
        shoulder_center = (left_shoulder + right_shoulder) / 2

        if hip_center is None or shoulder_center is None:
            return directions

        # if not self.position_history:
        #     self.previous_avg_hip = np.mean([p['hip'] for p in self.position_history], axis=0)
        # else:
        #     return directions

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

        moved_left, moved_right =self.state_tracker.update(*self._detect_horizontal_movement(avg_hip))
        if moved_left is not None:
            directions['left'] = moved_left
            directions['right'] = moved_right

        directions['jump'] = self._detect_jump(hip_center, left_ankle, right_ankle)
        directions['slide'] = self._detect_squat(avg_hip, avg_shoulder)
        directions['start'] = self._detect_start(left_wrist, right_wrist, left_shoulder, right_shoulder)

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
        image_shape: Tuple of (height, width) for normalization

    Returns:
        dict: Direction flags {'left': bool, 'right': bool, 'jump': bool, 'slide': bool}
    """
    return _evaluator.evaluate(results)
