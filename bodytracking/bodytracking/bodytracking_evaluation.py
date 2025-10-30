import numpy as np
from bodytracking import global_vars

class DirectionEvaluator:
    def __init__(self):
        # Store previous positions for movement detection

        # Thresholds (normalized coordinates, range approximately -0.5 to 0.5)
        self.HORIZONTAL_THRESHOLD = 200  # Movement threshold for left/right
        self.VERTICAL_JUMP_THRESHOLD = 80  # Upward movement threshold
        self.SQUAT_THRESHOLD = 200  # Hip-to-shoulder ratio change for squat
        self.MIN_CONFIDENCE = global_vars.PERSON_MINIMUM_THRESHOLD  # Minimum detection confidence

        # Smoothing window
        self.position_history = []
        self.history_size = 3
        self.previous_direction = None

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

    def _detect_jump(self, avg_hip):
        """Detect upward jump based on hip vertical movement."""


        vertical_delta = global_vars.HEIGHT/2 - avg_hip[1]  # Y increases downward
        return vertical_delta > self.VERTICAL_JUMP_THRESHOLD

    def _detect_squat(self, avg_hip, avg_shoulder):
        """Detect squat based on hip-to-shoulder distance reduction."""

        current_distance = global_vars.HEIGHT/2 - avg_hip[1]

        return current_distance < -self.SQUAT_THRESHOLD

    def reset_history(self):
        """Reset position history."""
        self.position_history = []

    def evaluate(self, results, image_shape):
        """
        Evaluate movement direction based on pose keypoints.

        Returns dict with keys: 'left', 'right', 'jump', 'slide'
        Each value is True if movement detected in that direction.
        """
        directions = {
            'left': False,
            'right': False,
            'jump': False,
            'slide': False
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

        if left_hip is None or right_hip is None or left_shoulder is None or right_shoulder is None:
            return directions

        # Calculate centers in normalized coordinates
        hip_center = (left_hip + right_hip) / 2
        shoulder_center = (left_shoulder + right_shoulder) / 2

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
        moved_left, moved_right = self._detect_horizontal_movement(avg_hip)
        if moved_left is not None:
            directions['left'] = moved_left
            directions['right'] = moved_right

        directions['jump'] = self._detect_jump(avg_hip)
        directions['slide'] = self._detect_squat(avg_hip, avg_shoulder)

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


def evaluate_directions(results, image_shape):
    """
    Evaluate movement directions from YOLO pose results.

    Args:
        results: YOLOv8 pose detection results
        image_shape: Tuple of (height, width) for normalization

    Returns:
        dict: Direction flags {'left': bool, 'right': bool, 'jump': bool, 'slide': bool}
    """
    return _evaluator.evaluate(results, image_shape)
