import asyncio
import websockets
import threading


class WebSocketServer:
    def __init__(self, host="localhost", port=8080):
        self.host = host
        self.port = port
        self.clients = set()
        self.loop = None
        self.server = None

    def start(self):
        def run_server():
            try:
                self.loop = asyncio.new_event_loop()
                asyncio.set_event_loop(self.loop)
                self.loop.run_until_complete(self._start_server())
                self.loop.run_forever()
            except Exception as e:
                print(f"WebSocket server thread error: {e}")

        thread = threading.Thread(target=run_server, daemon=True)
        thread.start()

    async def _start_server(self):
        self.server = await websockets.serve(self._handle_client, self.host, self.port)
        print(f"WebSocket server started on ws://{self.host}:{self.port}")

    async def _handle_client(self, websocket):
        print(f"Client connected from {websocket.remote_address}")
        self.clients.add(websocket)
        try:
            await websocket.wait_closed()
        except websockets.exceptions.ConnectionClosed:
            print("Client disconnected")
        except Exception as e:
            print(f"Error in client handler: {e}")
        finally:
            print("Removing client from set")
            self.clients.discard(websocket)

    def notify_new_frame(self, directions):
        for direction in directions:
            print("print to server: ", direction)

            if self.loop and self.clients:
                asyncio.run_coroutine_threadsafe(self._broadcast(direction), self.loop)

    async def _broadcast(self, message):
        if not self.clients:
            return

        disconnected = set()
        for client in self.clients.copy():
            try:
                await client.send(message)
            except websockets.exceptions.ConnectionClosed:
                disconnected.add(client)
            except Exception:
                disconnected.add(client)

        for client in disconnected:
            self.clients.discard(client)

    def stop(self):
        if self.loop:
            self.loop.call_soon_threadsafe(self.loop.stop)
