import subprocess
from requests import post


target_mac = b'60:4b:aa:01:59:45'
notification_url = 'http://vcm-12481.vm.duke.edu/notify'


def listen():
    listen_process = subprocess.Popen(['tcpdump', '-I', '-i', 'mon1', '-p', '-e'], stdout=subprocess.PIPE)

    for row in iter(listen_process.stdout.readline, b''):
        if target_mac in row:
            post(url=notification_url, data='found device')
            print('found magic leap')


listen()
