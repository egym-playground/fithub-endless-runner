import asyncio
import websockets


async def client():
    uri = "ws://localhost:8080"
    try:
        print(f"Connecting to {uri}...")
        async with websockets.connect(uri) as websocket:
            print(f"Connected to {uri}")
            print("Waiting for messages... (Press Ctrl+C to stop)")

            try:
                async for message in websocket:
                    print(f"Received: {message}")
            except websockets.exceptions.ConnectionClosed:
                print("Connection closed by server.")

    except ConnectionRefusedError:
        print("Could not connect to WebSocket server. Make sure the body tracking app is running.")
    except KeyboardInterrupt:
        print("\nClient stopped.")
    except Exception as e:
        print(f"Error: {e}")


if __name__ == "__main__":
    try:
        asyncio.run(client())
    except KeyboardInterrupt:
        print("\nExiting...")
