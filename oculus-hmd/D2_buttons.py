#!/usr/bin/env python

import socket
#import cwiid
import time
import sys, getopt
import thread
import ast
from threading import Thread

port1 = 13216
port2 = 13217
threads = []
def wiimote(threadName,server):
    print 'place wiimote in discoverable mode'
    #wiimote = cwiid.Wiimote()
    #wiimote.rpt_mode = cwiid.RPT_ACC | cwiid.RPT_BTN
    client_socket_wiimote = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    address = ('',port1)
    client_socket_wiimote.bind(address)
    client_socket_wiimote.settimeout(2.0)
    data_dict = dict()
#This creates socket
    flag = False
    while (1):
        #x = wiimote.state['acc'][cwiid.X]
        #y = wiimote.state['acc'][cwiid.Y]
        #z = wiimote.state['acc'][cwiid.Z]
	#data = generate_command(wiimote.state['buttons'])

        #    data = raw_input("Type data to send");
   ##     client_socket_wiimote.sendto(data, (server,port1))
        print "receiving request"
	
	try:
            data_dict = {}
       	    recv_data, addr = client_socket_wiimote.recvfrom(5500)
       	    print recv_data,"\n", addr
       	    flag = True
	    data_dict['360'] = '2'
       	except socket.timeout:
       	    print "timeout\n"
    	    data_dict['360'] = '1'   
        if flag:
            print "Sending data\n"
           # client_socket_wiimote.sendto(data,addr)
            time.sleep(0.3)	
        
            

            datalidar = recv_data.split('\n')
	
            if datalidar:
                for i in datalidar:
                	    #if i[0] and i[0].isdigit():	
                    lines = i.split(',')
                    if(lines[0] == '400'):
                        data_dict[lines[0]] = lines[1]
                    if (lines[0].isdigit() and (int(lines[0]) >= 0 and int(lines[0]) < 360) and len(lines) == 4):
                        data_dict[lines[0]] = lines[1]

            f = open('data','w')
            if('400' in data_dict.keys()):
                print '400 is in the dictionary'
                f.write('400,1' + '\n')
            else:
                f.write('400,0' + '\n')

            for key,value in data_dict.items():
                try:
                    if (ast.literal_eval(key))%5 == 0:
                        if ast.literal_eval(value) != 0:	        
                            f.write(key+','+value+'\n')
                except:
                    print 'EXCEPTION: ', sys.exc_info()[0]
            f.close()

        
                
    return


def generate_command(winput):
    if (winput == 2048):#forward
        data = '2'
    elif (winput == 1024):#reverse
        data = '3'
    elif (winput == 256):#left
        data = '4'
    elif (winput == 512):#right
        data = '5'
    elif (winput == 4):#brake
	data = '6'
    else:
        data = '2'
    return data

def getLidardata(threadName,server):
    address = (server,port2)
    
    server_socket_data = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
   ## server_socket_data.bind(address)
  ##  recv_data, addr = server_socket_data.recvfrom(256)
   ## print recv_data,"\n"    
    while(1):
        print "sending Request",data
        server_socket_data.sendto(data,address)
        time.sleep(1)
    return

def main(argv):
    print argv[0]
    thread1 = Thread(target = wiimote, args = ("thread-1",argv[0]))
    # thread2 = Thread(target = getLidardata, args = ("thread-2",argv[0]))
    try:
        thread1.start()
    except:
        print "Error : unable to start thread 1"
        #   try:
#thread2.start()
# except:
#       print "Error : unable to start thread 2"
        
    threads.append(thread1)
    #    threads.append(thread2)
    print threads,"\n"
    for t in threads:
        t.join()
    print "Exiting Main Thread"
    

    return

if __name__ == "__main__":
    main(sys.argv[1:])
