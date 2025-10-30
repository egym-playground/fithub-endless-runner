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

    print("Sending continuous notifications... Press Ctrl+C to stop.")

    try:
        counter = 0
        while True:
            counter += 1
            print(f"Sending notification {counter}: jump")
            server.notify_new_frame("jump")
            time.sleep(1)
    except KeyboardInterrupt:
        print("\nShutting down...")
        server.stop()


if __name__ == "__main__":
    main()
