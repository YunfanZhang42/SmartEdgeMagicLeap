from flask import Flask, render_template, request, g
from werkzeug import secure_filename
from time import sleep

app = Flask(__name__, static_folder='static', static_url_path='/static')
g.can_download = False


@app.route('/upload', methods=['POST'])
def upload_file():
    if request.method == 'POST':
        f = next(iter(request.files.values()))
        f.save('uploaded/' + secure_filename(f.filename))
        return 'file uploaded successfully'


@app.route('/notify', methods=['POST'])
def receive_notify():
    g.can_download = True
    return 'notification received'


@app.route('/push', methods=['POST', 'GET'])
def push():
    while not g.can_download:
        sleep(0.005)
    return 'Start download'


if __name__ == '__main__':
    app.run(host='0.0.0.0', port=80, debug=False)
