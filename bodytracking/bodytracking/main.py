#pipe server
from body import BodyThread
import time
import struct
import global_vars
from sys import exit

thread = BodyThread()

try:
    thread.start()

    # Keep main thread alive to catch KeyboardInterrupt
    while not global_vars.KILL_THREADS:
        time.sleep(0.1)

except KeyboardInterrupt:
    print("\nShutting down...")
    global_vars.KILL_THREADS = True
    thread.join(timeout=2.0)  # Wait for thread to finish
    exit(0)
