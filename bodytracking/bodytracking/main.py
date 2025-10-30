#pipe server
from body import BodyThread
import time
import struct
import global_vars
from sys import exit


try:
    thread = BodyThread()
    thread.start()
# make this react to keyboard interruptions
except KeyboardInterrupt:

    global_vars.KILL_THREADS = True
    time.sleep(0.5)
    exit()
