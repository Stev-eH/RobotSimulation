import base64
import datetime
from io import BytesIO
import os
import socket



###################
print('\n==============\nRobot Master Client\n==============\n')
print('\n')

###################
# Get server details
ip_address = input("Enter a ip address: ")
port = input("Enter port: ")

# Create a TCP/IP socket
s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

# Bind the socket to the port
server_address = (ip_address, int(port))
print('connect to server at %s port %s' % server_address)
s.connect(server_address)

s.setsockopt(socket.SOL_SOCKET, socket.SO_KEEPALIVE, 100)

# Enum of message types, must match InterboClient
class MessageType:
    Invalid, Acknowledge, Goodbye, PoseUpdate, InitBrick, GazePos  = range(6)

# Example message: 
# update_pose -180,30,75,-10,90,0,1

# Key Parameters
default_buffer_size = 1024
buffer_size = default_buffer_size

try:
    while True:
        # Wait for a message    
        msg = input("Enter message: ")
        
        if (msg == "exit"):
            msg = "2"
            s.send(msg.encode('utf-8'))
            break         
        elif ("update_pose" in msg):            
            msgSplit = msg.split(' ')
            if (len(msgSplit) == 2):
                poseArr = msgSplit[1]
                msg = "%s\t%s\n" % (str(int(MessageType.PoseUpdate)), poseArr)
            else:
                msg = ''
                print('Invalid message format')
        elif ("init_brick" in msg):            
            msgSplit = msg.split(' ')
            if (len(msgSplit) == 2):
                poseArr = msgSplit[1]
                msg = "%s\t%s\n" % (str(int(MessageType.InitBrick)), poseArr)
            else:
                msg = ''
                print('Invalid message format')
        elif ("gaze_pos" in msg):
            msg = "%s\t%s\n" % (str(int(MessageType.GazePos)), "gaze_pos_request")                

        s.send(msg.encode('utf-8'))
        
        data = s.recv(buffer_size)
        print('received: ', data)

finally:    
    # Clean up the connection
    print('close connection')
    s.close()