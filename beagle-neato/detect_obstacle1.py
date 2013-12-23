import sys
import serial
import time
import socket

from threading import Thread
sys.path.append('../../wiimote')

import pwm_beagleboard

pwm_mode = 100
connection = False
LEFT_ANGLE = 30
RIGHT_ANGLE = 330
THRESHOLD = 999
threads = []
obstacles = {}

IN1_L = '139'
IN2_L = '138'
PWM_L = '137'
IN1_R = '136'
IN2_R = '135'
PWM_R = '134'

def delay(dur):
    t0 = time.time()
    while True:
        time.sleep(dur)
        t1 = time.time()
        dur = dur - (t1-t0)
        if dur <= 0:
            break
    return



def isbetween(angle):
    if angle > RIGHT_ANGLE and  angle <= 357:
        return 1 #OBSTACE AT RIGHT
    elif angle < LEFT_ANGLE and angle > 2:
        return 2 # OBSTACLE AT LEFT
    else:
        return 0



def getcommand(datalidar):
    global obstacles
    obstacles = {}
    data = datalidar.split('\n')
    if data:
        for i in data:
            #if i[0] and i[0].isdigit():	
            lines = i.split(',')
            if (lines[0].isdigit() and isbetween(int(lines[0])) and len(lines) == 4):
                if int(lines[1]) < THRESHOLD and int(lines[1]) > 300:
                    obstacles[int (lines[0])] = int (lines[1])
        return 




def run(threadName,device):
    fwcount = 0
    queue = []
    global connection
    global pwm_mode
    print device
    count = 0
    while (not connection) :
        print "in connection \n"
        try:
            neato = serial.Serial(device,baudrate=115200,timeout=1.0)
            neato.close()
            neato.open()
            #delay(0.01)
            neato.write("testmode on\n")
            print "testmode on\n \r"
            delay(1.0)
            neato.write("setldsrotation on\n\r")
            delay(2.0)
            connection = True
        except:
            print sys.exc_info()[0]
            
    while True:
        try:
            #pwm_mode = '6'
            #print "stopped \n"
            neato.write("getldsscan\n\r")
            delay(0.01)
            data = neato.read(size=5000)
            getcommand(data)
            if pwm_mode == '7':
                neato.write("testmode off\n\r")
                delay(0.01)
                neato.close()
                break
        except serial.SerialException:
            pwm_mode = '7'
            neato.write("testmode off\n\r")
            delay(0.01)
            neato.close()
            print sys.exc_info()[0]
            break
        except KeyboardInterrupt:
            pwm_mode = '7'
            neato.write("testmode off\n\r")
            delay(1.0)
            neato.close()
            break
    return
        
    #try:
    #    run(argv[0],argv[1],argv[2])
    #except KeyboardInterrupt:
    #    pass
    

def run_pwm(threadName, server):
    global pwm_mode
    while True:
        try:
            #print "THREAD PWM: Mode PWM = ", pwm_mode
            #print server,"\n"
            if (pwm_mode == '2'):
                pwm_beagleboard.start_pwm (PWM_L, 0.01, 22, IN1_L, IN2_L)
                pwm_beagleboard.start_pwm (PWM_R, 0.01, 22, IN1_R, IN2_R)
            elif (pwm_mode == '4'): #LEFT
                pwm_beagleboard.start_pwm (PWM_L, 0.01, 22, IN2_L, IN1_L)
                pwm_beagleboard.start_pwm (PWM_R, 0.01, 22, IN1_R, IN2_R)
            elif (pwm_mode == '5'): #RIGHT
                pwm_beagleboard.start_pwm (PWM_L, 0.01, 22, IN1_L, IN2_L)
                pwm_beagleboard.start_pwm (PWM_L, 0.01, 22, IN2_R, IN1_R)
            elif (pwm_mode == '6'): #BRAKE
                pass
            #pwm_beagleboard.start_pwm (PWM_L, 0.01, 0, IN1_L, IN2_L)
            #pwm_beagleboard.start_pwm (PWM_L, 0.01, 0, IN1_R, IN2_R)
            elif (pwm_mode == '3'): #REVERSE
                pwm_beagleboard.start_pwm (PWM_L, 0.01, 22, IN2_L, IN1_L)
                pwm_beagleboard.start_pwm (PWM_R, 0.01, 22, IN2_R, IN1_R)
            elif (pwm_mode == '7'): #quite thread
                print "Exit Thread 2"
                return
            else:
                pass
            #pwm_beagleboard.start_pwm (PWM_L, 0.01, 0, IN2_L, IN1_L)
            #pwm_beagleboard.start_pwm (PWM_L, 0.01, 0, IN2_R, IN1_R)
            
        #Remove this sleep statement. Keep it for terminal display
        #delay(0)
        except KeyboardInterrupt:
            pwm_mode = '7'
            return


def main(argv):
    #print argv[0],argv[1],argv[2]
    global threads
    fwcount = 0
    queue = []
    global pwm_mode
    count = 0


    thread_neato = Thread(target = run, args = ("thread_neato", argv[0]))
    thread_pwm = Thread(target = run_pwm, args = ("thread_pwm", argv[0]))
    
    try:
       thread_neato.start() 
    except:
        print "ERROR: Unable to start thread_neato"
        return
    
    try:
       thread_pwm.start() 
    except:
        print "ERROR: Unable to start thread_pwm"
        return
    
    #try:
    #    run(argv[0],argv[1],argv[2])
    #except KeyboardInterrupt:
    #    pass
    
    threads.append(thread_neato)
    threads.append(thread_pwm)
    print "Threads: ", threads, "\n"
    while True:
        delay(0.02)
        try:
            if obstacles.keys():
                fwcount = 0
                if isbetween(min(obstacles.keys())) == 2 or isbetween(max (obstacles.keys())) == 2:   # OBSTACLE ON LEFT
                    #print "min is ",min(obstacles.keys()),obstacles[min(obstacles.keys())],"\n"
                    print obstacles
                    for i in range(0,1):
                        if '5' in queue:
                            queue.remove('5')
                        else:
                            queue.append('4')
                    pwm_mode = '5'
                elif isbetween(max(obstacles.keys())) == 1 or isbetween(min(obstacles.keys())) == 1:   # OBSTACLE ON RIGHT
                    #print "max is ",max(obstacles.keys()),obstacles[max(obstacles.keys())],"\n"
                    print obstacles
                    for i in range(0,1):
                        if '4' in queue:
                            queue.remove('4')
                        else:
                            queue.append('5')
                    pwm_mode = '4'
                else:
                    pwm_mode = '6'
                    print "Nothing to do\n"
            else:
                fwcount += 1
                if fwcount >= 10 and queue:
                    pwm_mode = queue.pop(0)
                    #delay(0.1)
                    fwcount = 0
                else:
                    pwm_mode = '2'
                    #delay(0.1)
                    print "Move forward \n"
                #delay(1.0)
            count += 1
            if count == 1000:
                pwm_mode = '7'
                for t in threads:
                    t.join()
                print "Exiting Main Thread"
                break
                # neato.write("testmode off\n\r")
                # delay(0.01)
                # neato.close()
                # break
        except KeyboardInterrupt:
            pwm_mode = '7'
            for t in threads:
                t.join()
            print "Exiting Main Thread"
            # neato.write("testmode off\n\r")
            # delay(1.0)
            # neato.close()
            break
    

    return


if __name__ == "__main__":
    main(sys.argv[1:])
