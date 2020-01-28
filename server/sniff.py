from scapy.layers.dot11 import Dot11
from scapy.all import *
from requests import post

target_mac = '60:4b:aa:01:59:45'
notification_url = 'http://vcm-12481.vm.duke.edu/notify/'

found = False


def PacketHandler(pkt):
    global found
    if not found and pkt.haslayer(Dot11):
        if pkt.addr1.lower() == target_mac or pkt.addr2.lower() == target_mac or pkt.addr3.lower() == target_mac:
            print('found device')
            post(url=notification_url, data='found device')
            found = True


sniff(iface='en0', prn=PacketHandler)
