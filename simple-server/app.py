from flask import Flask, request, jsonify

app = Flask(__name__)

_store = {"data": {}}


@app.route("/data", methods=["POST"])
def set_data():
    if not request.is_json:
        return jsonify({"error": "Expected application/json"}), 415
    try:
        payload = request.get_json()
    except Exception:
        return jsonify({"error": "Invalid JSON"}), 400

    _store["data"] = payload

    return jsonify({"success": "Data stored"}), 201


@app.route("/data", methods=["GET"])
def get_data():
    return jsonify({"data": _store["data"]})


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000, debug=True)
