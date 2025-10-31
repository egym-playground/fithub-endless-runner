from flask import Flask, request, jsonify
from collections import defaultdict

app = Flask(__name__)

_user_store = {}
_active_user = None
_leaderboard = {}


def make_leaderboard_data():
    data = [
        {"playerName": _user_store[key]["firstName"], "score": value["score"], "coins": value["coins"]}
        for key, value in _leaderboard.items()
    ]
    data.sort(key=lambda entry: (-entry["score"], -entry["coins"]))
    data = [{"rank": i + 1, **data[i]} for i in range(len(data))]

    return data


@app.route("/data", methods=["POST"])
def set_data():
    global _active_user
    if not request.is_json:
        return jsonify({"error": "Expected application/json"}), 415
    try:
        payload = request.get_json()
    except Exception:
        return jsonify({"error": "Invalid JSON"}), 400

    user = payload["ObfuscatedId"]

    _user_store[user] = payload
    _active_user = user

    return jsonify({"success": "Data stored"}), 201


@app.route("/score", methods=["POST"])
def set_score():
    global _active_user

    if not request.is_json:
        return jsonify({"error": "Expected application/json"}), 415
    try:
        payload = request.get_json()
    except Exception:
        return jsonify({"error": "Invalid JSON"}), 400

    if _active_user is not None:
        _leaderboard[_active_user] = payload
    else:
        print("No active user found")

    return jsonify({"data": make_leaderboard_data()}), 201


@app.route("/score", methods=["GET"])
def get_score():
    return jsonify({"data": make_leaderboard_data()}), 200


_dummy_leaderboard = [
    {"rank": 1, "playerName": "ProGamer2024", "score": 25847, "coins": 432},
    {"rank": 2, "playerName": "SpeedRunner", "score": 22156, "coins": 389},
    {"rank": 3, "playerName": "CoinCollector", "score": 19875, "coins": 521},
]


@app.route("/scoretest", methods=["GET"])
def get_scoretest():
    return jsonify({"data": _dummy_leaderboard}), 200


@app.route("/scoretest", methods=["POST"])
def set_scoretest():
    if not request.is_json:
        return jsonify({"error": "Expected application/json"}), 415
    try:
        payload = request.get_json()
    except Exception:
        return jsonify({"error": "Invalid JSON"}), 400

    print(f"Received scoretest data {payload}")

    return jsonify({"data": _dummy_leaderboard}), 200


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=True, threaded=False)
