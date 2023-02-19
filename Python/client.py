#! /usr/local/bin/python3
import zmq
import time
import random
import cv2
context = zmq.Context()

socket = context.socket(zmq.REQ)
socket.connect("tcp://localhost:12346")

TIMEOUT = 50000

while True:

	socket.send_string("request")

	poller = zmq.Poller()
	poller.register(socket, zmq.POLLIN)
	evt = dict(poller.poll(TIMEOUT))

	if evt:
		if evt.get(socket) == zmq.POLLIN:
			#time.sleep(3)
			img = cv2.imread('../screen_grab.PNG')
			cv2.imshow('unity screengrab', img)
			cv2.waitKey(0)
			cv2.destroyAllWindows()
			response = socket.recv(zmq.NOBLOCK)
			print(response)
			continue


socket.close()
socket = context.socket(zmq.REQ)
socket.connect("tcp://localhost:12346")
