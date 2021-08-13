#OpenCV module
import cv2
import os
#face recongnition module by Adam Geitgey at https://github.com/ageitgey/face_recognition
import face_recognition
import numpy as np
import time
import fnmatch

##########################################################################################
#FIRST PART:
#GO TRHOUGH THE KNOWN FACES FOLDER AND DETECT ALL THE FACES IN ALL IMAGES
#THEN ADD THIS FACES TO THE KNOWN FACES LIST
##########################################################################################
#OPENING AND INITIALIZING THE WRITE FILE
def load(direc):
	start = time.time()
	#print("Carregando")
	known_faces = []
	known_names = []
	for filename in os.listdir(direc):
		if "meta" not in filename:
			file = os.path.join(direc, filename)
			image = face_recognition.load_image_file(file)
			try:
				image_encoding = face_recognition.face_encodings(image)[0]	#get only the first encoding
				known_faces.append(image_encoding)
				known_names.append(filename[:-4])							#remove filetype from filename
			except Exception as e:
				print("Error in face:" + file)
	np.save("faceFile",known_faces,False,False)
	np.save("nameFile",known_names,False,False)
	#print("Tempo de carregamento do diretório: " + str(time.time() - start) + "segundos")

def loadFaces(direc):
	if ( not os.path.isfile("nameFile.npy")):
		load(direc)

	known_names = np.load("nameFile.npy")
	known_faces = np.load("faceFile.npy")
	#print(known_names)
	#print(known_faces)
	return known_names, known_faces

def saveFaces(direc, faces, names):	
	np.save("faceFile",faces,False,False)
	np.save("nameFile",names,False,False)

#filename may or may not contain file type
def loadImages(filenames, direc='Data'):
	images = []
	for filename in filenames:
		search_res = find(filename + '*',direc)
		if search_res != []:
			actual_filename = search_res[0]
			images.append(face_recognition.load_image_file(actual_filename))
	return images

#from https://stackoverflow.com/questions/1724693/find-a-file-in-python
def find(pattern, direc=''):
	result = []
	for root, dirs, files in os.walk(os.path.join('./' + direc)):
		for name in files:
			if fnmatch.fnmatch(name, pattern):
				result.append(os.path.join(root, name))
	return result

if __name__ == "__main__":
	load("Data")
