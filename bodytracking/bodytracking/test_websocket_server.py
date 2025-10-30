#!/usr/bin/env python3

# Simple test of just the WebSocket server
import sys
import os
import time

# Add the bodytracking directory to the path
sys.path.append("/home/david/Documents/fithub-endless-runner/bodytracking/bodytracking")

from websocket_server import WebSocketServer


def main():
    print("Starting standalone WebSocket server test...")

    server = WebSocketServer()
    server.start()

    print("Server started. Waiting 2 seconds...")
    time.sleep(2)

    print("Sending test notifications...")
    for i in range(5):
        print(f"Sending notification {i+1}")
        server.notify_new_frame()
        time.sleep(1)

    print("Test complete. Press Ctrl+C to exit.")

    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        print("\nShutting down...")
        server.stop()


if __name__ == "__main__":
    main()
