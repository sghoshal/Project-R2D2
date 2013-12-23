import sys
import serial
import time
import socket

from threading import Thread
sys.path.append('../../wiimote')

import pwm_beagleboard

#device = "/dev/ttyACM0"
connection = False
port = 13216
commands = {'2':"setmotor 20 20 60\n",'3':"setmotor -20 -20 60\n",'4':"setmotor 0 30 60\n",'5':"30 0 60",'6':"0 0 0"}
pwm_mode = 100
threads = []

IN1_L = '139'
IN2_L = '138'
PWM_L = '137'
IN1_R = '136'
IN2_R = '135'
PWM_R = '134'

def run(threadName, server1, server2, device):
    global connection
    global pwm_mode
    while(not connection):
        try:
            neato = serial.Serial(device,baudrate=115200,timeout=1.0)
            neato.close()
            neato.open()
            neato.write("testmode on\n")
            print "testmode on\n\r"
            time.sleep(2)
            neato.write("setldsrotation on\n\r")
            time.sleep(2)
            connection = True
        except serial.SerialException:
            print sys.exc_info(),"1"

    try:
        client_socket = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
        client_socket.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR,1)
        client_socket1 = socket.socket(socket.AF_INET,socket.SOCK_DGRAM)
        client_socket1.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR,1)
    except:
        print "Socket ERROR \n"
        
    address1 = (server1,port)
    address2 = (server2,port)
    client_socket.settimeout(2.0)

    while(1):
        try:
            neato.write("getldsscan\n")
            time.sleep(0.1)
            data = ''
            data = neato.read(size=5000)
            if (pwm_mode == '6'):
                data += '\n400,1,0,0\n'
            bytes_sent1 = client_socket1.sendto(data,address2)
            bytes_sent = client_socket.sendto(data,address1)
            # print data, "bytes sent",bytes_sent,"\n"
            try:
                time.sleep(0.1)
                wiimote_data,addr = client_socket.recvfrom(1)
                pwm_mode = wiimote_data
                # print "WIIMOTE DATA: " , wiimote_data, "PWM MODE: ", pwm_mode
                # perform_action(neato,wiimote_data[0])
            except socket.timeout:
                print "\n socket timeout"
                time.sleep(0.1)
        except KeyboardInterrupt:
            neato.write("testmode off\n")
            time.sleep(1)
            neato.close()
            break
        except serial.SerialException:
            print sys.exc_info()[0]
    return


def perform_action(neato,x):
    try:
        neato.write(commands[x])
    except:
        print "could not execute\n"
    return

# IN1_L=139
# IN2_L=138
# PWM_L=137
# IN1_R=136
# IN2_R=135
# PWM_R=134
def run_pwm(threadName, server):
    while True:
        # print "THREAD PWM: Mode PWM = ", pwm_mode
        if (pwm_mode == '2'):
            pwm_beagleboard.start_pwm (PWM_L, 0.01, 20, IN1_L, IN2_L)
            pwm_beagleboard.start_pwm (PWM_R, 0.01, 20, IN1_R, IN2_R)
        elif (pwm_mode == '4'): #LEFT
            pwm_beagleboard.start_pwm (PWM_L, 0.01, 0, IN1_L, IN2_L)
            pwm_beagleboard.start_pwm (PWM_R, 0.01, 20, IN1_R, IN2_R)
        elif (pwm_mode == '5'): #RIGHT
            pwm_beagleboard.start_pwm (PWM_L, 0.01, 20, IN1_L, IN2_L)
            pwm_beagleboard.start_pwm (PWM_L, 0.01, 0, IN1_R, IN2_R)
        elif (pwm_mode == '6'): #BRAKE
            pass
            #pwm_beagleboard.start_pwm (PWM_L, 0.01, 0, IN1_L, IN2_L)
            #pwm_beagleboard.start_pwm (PWM_L, 0.01, 0, IN1_R, IN2_R)
        elif (pwm_mode == '3'): #REVERSE
            pwm_beagleboard.start_pwm (PWM_L, 0.01, 20, IN2_L, IN1_L)
            pwm_beagleboard.start_pwm (PWM_R, 0.01, 20, IN2_R, IN1_R)
        else:
            pass
            #pwm_beagleboard.start_pwm (PWM_L, 0.01, 0, IN2_L, IN1_L)
            #pwm_beagleboard.start_pwm (PWM_L, 0.01, 0, IN2_R, IN1_R)
        
        #Remove this sleep statement. Keep it for terminal display
        #time.sleep(0)

def main(argv):
    print argv[0],argv[1],argv[2]
    thread_neato = Thread(target = run, args = ("thread_neato", argv[0], argv[1], argv[2]))
    thread_pwm = Thread(target = run_pwm, args = ("thread_pwm", argv[0]))
    
    try:
       thread_neato.start() 
    except:
        print "ERROR: Unable to start thread_neato"
    
    try:
       thread_pwm.start() 
    except:
        print "ERROR: Unable to start thread_pwm"
    
    #try:
    #    run(argv[0],argv[1],argv[2])
    #except KeyboardInterrupt:
    #    pass
    
    threads.append(thread_neato)
    threads.append(thread_pwm)
    print "Threads: ", threads, "\n"
    
    for t in threads:
        t.join()
    print "Exiting Main Thread"

    return

if __name__ == "__main__":
    main(sys.argv[1:])
