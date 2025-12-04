from position_state import PositionState

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