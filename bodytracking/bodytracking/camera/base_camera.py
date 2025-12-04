from abc import ABC, abstractmethod


class Camera(ABC):

    @abstractmethod
    def get_frame(self):
        pass

    @abstractmethod
    def stop(self):
        pass

    @abstractmethod
    def height(self):
        pass

    @abstractmethod
    def width(self):
        pass

