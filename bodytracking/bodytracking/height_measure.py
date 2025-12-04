from pose_state import PoseState
from bodytracking import global_vars

class HeightMeasure:
    def __init__(self):
        self.kp = None  # type: PoseState
        self.person_height = 0
        self.person_width = 0

    def _update_kp(self, keypoints):
        self.kp = keypoints

    def _measure_person_height(self):
        if any(x is None for x in (self.kp.left_eye, self.kp.right_eye, self.kp.left_ankle, self.kp.right_ankle)):
            return 0
        shoulder_center = (self.kp.left_eye + self.kp.right_eye) / 2
        ankle_center = (self.kp.left_ankle + self.kp.right_ankle) / 2
        return abs(ankle_center[1] - shoulder_center[1])

    def _measure_person_width(self):
        # return max width so lowest x and highest x on right
        if any(x is None for x in (self.kp.left_shoulder, self.kp.right_shoulder, self.kp.left_hip, self.kp.right_hip, self.kp.left_wrist, self.kp.right_wrist)):
            return 0
        leftest = min(self.kp.left_shoulder[0], self.kp.left_hip[0], self.kp.left_wrist[0])
        rightest = max(self.kp.right_shoulder[0], self.kp.right_hip[0], self.kp.right_wrist[0])

        return abs(leftest - rightest)

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
            if global_vars.DEBUG:
                print(f"new height found : {person_height}")
            self.person_height = person_height

    def _update_person_width(self, person_width):
        if global_vars.DEBUG:
            print(f"new width found : {person_width}")
        self.person_width = person_width

    @staticmethod
    def _is_standing_still(directions):
        return all(not value for value in directions.values())

    def update_height(self, directions, kps):
        self._update_kp(kps)
        if self._is_standing_still(directions):
            new_person_height = self._measure_person_height()
            new_person_width = self._measure_person_width()

            self._update_person_height(new_person_height)
            self._update_person_width(new_person_width)
        return self.person_height

    def get_height(self):
        return self.person_height

    def get_width(self):
        return self.person_width