#!/usr/bin/env python
import random
import time

def main():
	meter_to_milli = 1000;
	distance = 60 * meter_to_milli
	while(1):
		f = open('lidar_data.txt', 'w')
		
		distance = distance - (0.2 * meter_to_milli)
		if (distance < (10 * meter_to_milli)):
			distance = (30 * meter_to_milli)

		for i in range(0, 180, 15):
			line = '{0},{1}\n'.format(i, distance)
			#print line
			f.write(line)
		#print ''
		time.sleep(0.1)

if __name__=="__main__":
	main()
