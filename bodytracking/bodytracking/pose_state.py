class PoseState:
    def __init__(self, keypoints):
        self.left_eye = self._get_keypoint(keypoints, 2)
        self.right_eye = self._get_keypoint(keypoints, 3)
        self.left_shoulder = self._get_keypoint(keypoints, 5)
        self.right_shoulder = self._get_keypoint(keypoints, 6)
        self.left_wrist = self._get_keypoint(keypoints, 9)
        self.right_wrist = self._get_keypoint(keypoints, 10)
        self.left_hip = self._get_keypoint(keypoints, 11)
        self.right_hip = self._get_keypoint(keypoints, 12)
        self.left_ankle = self._get_keypoint(keypoints, 15)
        self.right_ankle = self._get_keypoint(keypoints, 16)

    @staticmethod
    def _get_keypoint(keypoints, index):
        """Extract keypoint coordinates safely."""
        if keypoints.xy is not None and len(keypoints.xy) > 0:
            xy = keypoints.xy[0].cpu().numpy()
            conf = keypoints.conf[0].cpu().numpy()
            if index < len(xy) and conf[index] > 0.5:
                return xy[index]
        return None

    def main_kps_available(self):
        """Check if main keypoints are available."""
        required_kps = [
            self.left_eye, self.right_eye,
            self.left_shoulder, self.right_shoulder,
            self.left_hip, self.right_hip,
            self.left_ankle, self.right_ankle
        ]
        return all(kp is not None for kp in required_kps)