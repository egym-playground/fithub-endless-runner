const WebSocket = require('ws');
const readline = require('readline');

const wss = new WebSocket.Server({ port: 8080 });

console.log('WebSocket server running on ws://localhost:8080');
console.log('Waiting for Unity client to connect...');

wss.on('connection', function connection(ws) {
    console.log('Unity client connected!');

    ws.on('message', function incoming(message) {
        console.log('Received from Unity:', message.toString());
    });

    ws.on('close', function() {
        console.log('Unity client disconnected');
    });

    // Send a welcome message
    ws.send('Connected to WebSocket server');
});

// Create readline interface for manual testing
const rl = readline.createInterface({
    input: process.stdin,
    output: process.stdout
});

console.log('\nManual controls (type these commands):');
console.log('- left: Move left');
console.log('- right: Move right');
console.log('- jump: Jump');
console.log('- slide: Slide');
console.log('- quit: Exit server\n');

rl.on('line', (input) => {
    const command = input.trim().toLowerCase();

    if (command === 'quit') {
        console.log('Shutting down server...');
        wss.close();
        rl.close();
        process.exit(0);
    }

    // Broadcast command to all connected clients
    wss.clients.forEach(function each(client) {
        if (client.readyState === WebSocket.OPEN) {
            client.send(command);
            console.log(`Sent command: ${command}`);
        }
    });
});
