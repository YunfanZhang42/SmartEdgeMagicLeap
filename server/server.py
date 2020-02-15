from flask import Flask, request
from werkzeug import secure_filename
from time import sleep
from flask_sqlalchemy import SQLAlchemy

app = Flask(__name__, static_folder='static', static_url_path='/static')
app.secret_key = 'test'
db = SQLAlchemy(app)


class Record(db.Model):
    id = db.Column(db.Integer, primary_key=True)
    can_download = db.Column(db.Boolean(False), nullable=False)


@app.route('/upload', methods=['POST'])
def upload_file():
    if request.method == 'POST':
        f = next(iter(request.files.values()))
        f.save('uploaded/' + secure_filename(f.filename))
        return 'file uploaded successfully'


@app.route('/notify', methods=['POST'])
def receive_notify():
    record = Record.query.first()
    record.can_download = True
    db.session.commit()
    return 'notification received'


@app.route('/push', methods=['POST', 'GET'])
def push():
    while True:
        record = Record.query.first()
        db.session.close()
        if record.can_download:
            break
        sleep(0.01)


    record.can_download = False
    db.session.commit()
    return 'Start download'


if __name__ == '__main__':
    db.create_all()
    new_record = Record(can_download=False)
    db.session.add(new_record)
    db.session.commit()
    app.run(host='localhost', port=8080, debug=False, threaded=True)
