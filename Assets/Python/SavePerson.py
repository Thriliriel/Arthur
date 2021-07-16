import os 
import numpy as np
import sys
import time
import argparse
from shutil import copyfile
#import base64

def saveNewPerson(image, direc, name):
	#delete namefile and facefile
	if os.path.exists("nameFile.npy"):
		os.remove("nameFile.npy")
	if os.path.exists("faceFile.npy"):
		os.remove("faceFile.npy")

	#save image from string
	#b64 = base64.b64encode(image)
	arq1 = direc+"/"+name+".png"
	copyfile(image, arq1)

	return True

image = sys.argv[1]
direc = sys.argv[2]
name = sys.argv[3]

print(saveNewPerson(image, direc, name))