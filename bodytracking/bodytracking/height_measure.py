from pose_state import PoseState

class HeightMeasure:
    def __init__(self):
        self.kp = None  # type: PoseState
        self.person_height = 0

    def _update_kp(self, keypoints):
        self.kp = keypoints

    def _measure_person_height(self):
        shoulder_center = (self.kp.left_eye + self.kp.right_eye) / 2
        ankle_center = (self.kp.left_ankle + self.kp.right_ankle) / 2
        return abs(ankle_center[1] - shoulder_center[1])

    def _is_body_upright(self):
        # Check if shoulders and ankles are vertically aligned (simple upright check)
        if None in (self.kp.left_shoulder, self.kp.right_shoulder, self.kp.left_ankle, self.kp.right_ankle):
            return False
        shoulder_center = (self.kp.left_shoulder + self.kp.right_shoulder) / 2
        ankle_center = (self.kp.left_ankle + self.kp.right_ankle) / 2
        # Shoulders above ankles by a reasonable margin
        return shoulder_center[1] < ankle_center[1] - 50

    def _update_person_height(self, person_height):
        if abs(self. person_height - person_height) > 100:
            print(f"new height found : {person_height}")
            self.person_height = person_height

    @staticmethod
    def _is_standing_still(directions):
        return all(value is False for value in directions.values())

    def update_height(self, directions, kps):
        self._update_kp(kps)
        if self._is_standing_still(directions) and self._is_body_upright():
            new_person_height = self._measure_person_height()
            self._update_person_height(new_person_height)
        return self.person_height

