import face_recognition
import cv2
import os 
import numpy as np
import sys
import time
import argparse
import datetime
from loader import load, loadFaces, loadImages
import VideoLogger as vl
import pandas as pd
import base64
#import zmq

class FaceRecognition:   
	def mainReco(self, image, direcs, ths, mode):
		save = False
	
		arq1 = image
		modo = mode
		direc = direcs
		th = ths
		n = 3

		arq2 = False

		#print(arq1)
		#print(modo)
		#print(arq2)
		#print(th)
		#print(direc)

		th = float(th)

		#save image from string
		#b64 = base64.b64decode(arq1)
		#arq1 = 'camImage.png'
		#with open(arq1, 'wb') as f:
		#	f.write(b64)

		#parser = argparse.ArgumentParser(description = 'Face Recognition 1:1 1:N')
		#parser.add_argument('-arq1', action = 'store', dest = 'arq1',
		#	required = False,
		#	help = 'Foto a ser comparada')

		#parser.add_argument('-modo', action = 'store', dest = 'modo',
		#	required = True,
		#	help = 'Escolha do modo. 1 para 1:1 e N para 1:N')

		#parser.add_argument('-arq2', action = 'store', dest = 'arq2',
		#	required = False,
		#	help = 'Segunda foto na comparação 1:1')

		#parser.add_argument('-th', action = 'store', dest = 'th',
		#	required = False, default = 0.5, type = float,
		#	help = 'Valor que a distancia dos rostos na comparação 1:1 precisa ser menor para retornar verdadeiro')

		#parser.add_argument('-dir', action = 'store', dest = 'direc',
		#	required = False, default = "Data",
		#	help = 'Diretório das fotos a serem comparadas')

		#parser.add_argument('-n', action = 'store', dest = 'n',
		#	required = False, default = 3, type = int,
		#	help = 'Número de matches a serem retornados na 1:N')

		#parser.add_argument('-reload', action = 'store_true', required = False, dest = 'load',
		#						help = 'Chama a função de load novamente para caso tenha acontecido alguma alteração no diretório de dados')

		#parser.add_argument('-save', action='store_true')	

		#arguments = parser.parse_args()

		start = time.time()

		#if(arguments.load):
		#	load(arguments.direc)
		load(direc)

		if(modo.lower() == "n"):
			#print(comp1N(arq1, n, direc))
			return self.comp1N(arq1, n, direc, th)
		#elif(modo.lower() == "1"): NOT USING THESE OTHERS
		#	print(comp11(arq1, arq2, th, direc))
		#elif(modo.lower() == "c"):
		#	compCamN(n,direc)
		#elif(modo.lower() == "info"):
		#	compInfo(arq1, n, direc)
		#elif(modo.lower() == "vc"):
		#	videoLiveComp(direc, save_video = save, th = th)
		#elif(modo.lower() == "vf"):
		#	videoFileComp(direc, arq1, th = th)
		else:
			print("Erro")

	def saveNewPerson(self, image, direc, name):
		#delete namefile and facefile
		#if os.path.exists("nameFile.npy"):
		#	os.remove("nameFile.npy")
		#if os.path.exists("faceFile.npy"):
		#	os.remove("nameFile.npy")

		#save image from string
		b64 = base64.b64decode(image[0])
		arq1 = direc[0]+"/"+name[0]+".png"
		with open(arq1, 'wb') as f:
			f.write(b64)

		return True

	def run(self, text):        
		result = self.mainReco(text["image"], text["direc"], text["th"], text["mode"])
		if result == "false":
			return pd.DataFrame([result])
		else: 
			return pd.DataFrame(result)

	def runNewPerson(self, text):
		return pd.DataFrame([self.saveNewPerson(text["image"], text["direc"], text["name"])])

	def get_video_name():
		curr_time = datetime.datetime.now()
		return 'face_rec_' +  curr_time.strftime("%Y-%m-%d_%H:%M:%S") + '.avi'

	def resizeConstAR(img, sidelength, fix_horizontal):
		if fix_horizontal:
			r = sidelength / img.shape[1]
			dim = (sidelength, int(img.shape[0] * r ))
		else:
			r = sidelength / img.shape[0]
			dim = (int(img.shape[1] * r ), sidelength)

		return cv2.resize(img, dim, interpolation=cv2.INTER_AREA)

	def comparisonImage(image, matches, names):
		height,width,depth = image.shape
		matches = matches[:3]

		for i in range(len(matches)):
			matches[i] = resizeConstAR(matches[i], height, False)
	
		comp_img = []
		images = [image]
		images.extend(matches)

		#convert from BGR to RGB
		for i in range(len(images)):
			images[i] = images[i][:, :, ::-1]

		#add images to comp_img
		for i in range(height):
			line = []
			for img in images:
				line.extend(img[i])
			comp_img.append(line)

		#get the borders between the  images
		borders = [0,0,0,0]
		for i in range(len(images)-1):
			borders[i+1] = len(images[i][0]) + borders[i]
			#borders = np.concatenate()
			#borders.append(len(images[i][0]) + offset)
	
		borders = borders[1::]
		comp_img = np.array(comp_img)

		#draw lines dividing the images
		for x in borders:
			#							  start  end         color
			cv2.line(comp_img, (x, 0), (x, comp_img.shape[0]-1), (0,0,0), 4)
		
		cv2.rectangle(comp_img, (0, comp_img.shape[0]-25), (comp_img.shape[1]-1, comp_img.shape[0]-1), (0, 0, 0), -1)

		for i in range(len(names)):
			font = cv2.FONT_HERSHEY_DUPLEX
			spacing = 4
			cv2.putText(comp_img, names[i], (borders[i] + spacing, comp_img.shape[0] - spacing), font, 0.7, (255, 255, 255), 1,cv2.LINE_AA)

		while True:	
			cv2.imshow('comp',comp_img)

			if cv2.waitKey(1) & 0xFF == ord('q'):
				break

	#gambiarra pra fazer o opencv desenhar todos retangulos
	def espacador(arr, empty):
		ar = arr.copy()
		size = len(ar)

		if size % 2:
			ar.append(empty)

		for i in range(size):
			if i % 2:
				ar.append(ar[i])
				ar.append(empty)
				ar[i] = empty

		#clip last pos
		ar = ar[:-1:]
		return ar

	#name location = list of tuples of form (name,location)
	#scale = scale of the locations in comparison to the original image
	def drawBoundingBoxes(img, name_location):
		#gambiarra porque por algum motivo o cv2 nao gosta de imprimir meus retangulos em posicoes impares do vetor
		name_location = espacador(name_location, ("",(0,0,0,0)))

		for i in range(0, len(name_location)):
			name, (top, right, bottom, left) = name_location[i]
			#print(name + str((top, right, bottom, left)))
		
			# Draw a box around the face
			rect_color = (0, 0, 255)

			cv2.rectangle(img, (left, top), (right, bottom), rect_color, 2)

		
			# Draw a label with a name below the face
			cv2.rectangle(img, (left, bottom), (right, bottom + 35), rect_color, cv2.FILLED)
			font = cv2.FONT_HERSHEY_DUPLEX
			cv2.putText(img, name, (left + 6, bottom - 6 + 35), font, 0.7, (255, 255, 255), 1,cv2.LINE_AA)

		return img

	def scaleLocation(locations, scale):
		top, right, bottom, left = locations
		top *= int(scale)
		right *= int(scale)
		bottom *= int(scale)
		left *= int(scale)

		return top, right, bottom, left

	def compInfo(foto, num_matches, direc):
		unknown_image = face_recognition.load_image_file(foto)
		unknown_encoding = face_recognition.face_encodings(unknown_image)[0]

		known_names, known_faces = loadFaces(direc)                                     #c1

		face_names = []
		dist = face_recognition.face_distance(known_faces, unknown_encoding)
		top_matches = dist.argsort()[:num_matches + 1]
		worst_match = dist.argsort()[len(dist) -1] 

		print("BEST MATCHES")
		for match in top_matches:
			print(known_names[match] + " : " + str(dist[match]))

		print("WORST MATCH")
		print(known_names[worst_match] + " : " + str(dist[worst_match]))

	def comp1N(self, foto, num_matches, direc, th):
		unknown_image = face_recognition.load_image_file(foto)
		#print(unknown_image)
	
		#here is the problem: it is not being able to load the image
		#if unknown_image.all(None):
			#print("false")
			#return

		unknown_encoding = face_recognition.face_encodings(unknown_image)
	
		known_names, known_faces = loadFaces(direc)

		#print(known_names)

		face_names = []
		dist = face_recognition.face_distance(known_faces, unknown_encoding)
		top_matches = dist.argsort()[:num_matches]

		#print(dist)

		match_names = []

		for match in top_matches:
			match_names.append(known_names[match])

		#img_matches = loadImages(match_names)
		#comparisonImage(unknown_image, img_matches, match_names)

		allMatches = []

		for match in top_matches:
			if dist[match] <= th and known_names[match] != "None" and known_names[match] != "":
				allMatches.append(known_names[match] + ":" + str(dist[match]))
			#print(known_names[match] + " : " + str(dist[match]))
	
		if len(allMatches) == 0 or allMatches[0] == '':
			#print("false")
			return "false"
		else:
			#print(allMatches)
			return allMatches

	def comp11(foto1, foto2, th, direc):
		start = time.time()

		image1 = face_recognition.load_image_file(foto1)
		encoding1 = face_recognition.face_encodings(image1)[0]

		print("Tempo load 1: " + str(time.time() - start) + "segundos")

		start = time.time()
		image2 = face_recognition.load_image_file(foto2)
		# face_location2 = face_recognition.face_locations(image2)
		# top, right, bottom, left = face_location2[0]
		encoding2 = face_recognition.face_encodings(image2)[0]
		print("Tempo load 1: " + str(time.time() - start) + "segundos")


		dist = face_recognition.face_distance([encoding1], encoding2)
	
		return (dist[0] < th)

	def compCamN(num_matches, direc):
		cap = cv2.VideoCapture(0)

		while(True):
			# Capture frame-by-frame
			ret, frame = cap.read()

			#frame = cv2.resize(rawframe, (0, 0), fx=0.25, fy=0.25)
			#frame = frame[:, :, ::-1] #BGR to RGB apperently this is actually worse

			# Our operations on the frame come here
			#gray = cv2.cvtColor(frame, cv2.COLOR_BGR2GRAY)

			# Display the resulting frame
			cv2.imshow('frame',frame)
			if cv2.waitKey(1) & 0xFF == ord('q'):
				cv2.destroyAllWindows()
				#pn = input()
				#save photo
				pn = 'temp_capture'
				fn = pn + '.bmp'
				cv2.imwrite(fn,frame)
				break

		# When everything done, release the capture
		cap.release()

		#compare with the directory
		comp1N(fn,num_matches,direc, th)

		#delete photo
		os.remove(fn)

	# saveVideo [0=no/1=yes/2=ask]
	# to do save_annoted [0/1] save raw video or save video annoted with faces
	def videoLiveComp(direc, save_video = True, save_log = False, th = 0.6): 
		#todo check if camera exists
		video_scale = 0.5
		videoComp(direc, '', save_video, save_log, True, video_scale, th)	

	def videoFileComp(direc, filename, video_scale = 1.0, save_log = False, show_video = False, th = 0.6):
		#todo check if file exists, careful with empty strings
		videoComp(direc, filename, True, save_log, show_video, video_scale, th)

	def videoComp(direc, source, save_video, save_log, show_video, video_scale = 1.0, th = 0.6):
		print('Processando video')
		#source from camera or from file
		if source == '':
			#cap = cv2.VideoCapture(0)
			cap = cv2.VideoCapture(cv2.CAP_DSHOW)
			video_name = get_video_name()
		else:
			#cap = cv2.VideoCapture(source)
			cap = cv2.VideoCapture(source, cv2.CAP_DSHOW)	
			save_video = False
			video_name = source

		if save_video:
			# Get current camera properties
			video_width = cap.get(cv2.CAP_PROP_FRAME_WIDTH)
			video_height = cap.get(cv2.CAP_PROP_FRAME_HEIGHT)
			fps = cap.get(cv2.CAP_PROP_FPS)

			# Define the codec and create VideoWriter object
			# For more deatails about supported codecs visit http://www.fourcc.org/codecs.php
			fourcc = cv2.VideoWriter_fourcc(*'XVID')	
			out = cv2.VideoWriter(video_name,fourcc, fps, (int(video_width),int(video_height)))

		if save_log:
			log = vl.VideoLogger(video_name)

		# Create arrays of known face encodings and their names
		known_names, known_faces = loadFaces(direc)
		#print(known_faces)

		# Initialize some variables
		face_locations = []
		face_encodings = []
		face_names = []
		name_location = []

		frame_count = 0
		process_this_frame = 0
		frame_skip = 1

		while True:
			# Grab a single frame of video
			frame_exists, rawframe = cap.read()
			frame = rawframe

			print(os.getpid())

			if not frame_exists: break

			if save_video:					
				out.write(rawframe)

			print(process_this_frame)

			# Only process every x frames of video to save time
			if not process_this_frame % frame_skip:
				print("Afffe")
				# Resize frame for faster face recognition processing
				if video_scale == 1.0:
					frame == rawframe
				else:
					frame = cv2.resize(rawframe, (0, 0), fx=video_scale, fy=video_scale)

				# Find all the faces and face encodings in the current frame of video
				#face_locations = face_recognition.batch_face_locations([frame])[0]
				face_locations = face_recognition.face_locations(frame, model = 'hog')

				face_encodings = face_recognition.face_encodings(frame, face_locations)
			
				#scale back the face locations
				for i in range(len(face_locations)):
					face_locations[i] = scaleLocation(face_locations[i], 1/video_scale)

				face_names = []

				#for each face in the frame, get the name of corresponding person
				for face_encoding in face_encodings:
					matches = face_recognition.compare_faces(known_faces, face_encoding, th)
					name = "unknown"

					#if matches are found, get the first one, else add to the list of unknown encodings
					if True in matches:
						first_match_index = matches.index(True)
						name = known_names[first_match_index]

					face_names.append(name)
				print("Faces: " + face_names)
			
				#log the frame
				if save_log:
					log.logFrame(face_names, face_locations, face_encodings, frame_count)

				name_location = list(zip(face_names, face_locations))

							
			process_this_frame+=1
			frame_count+=1

			# Display the results
			if show_video: 
				frame = drawBoundingBoxes(rawframe, name_location)

				# Display the resulting image
				cv2.imshow(video_name,frame)

				# Hit 'q' on the keyboard to quit!
				if cv2.waitKey(15) & 0xFF == ord('q'):
					break

		# Release handle to the webcam
		cap.release()

		if save_video:
			out.release()

		if save_log:
			log.saveLog()

		cv2.destroyAllWindows()

	def saveNewPerson(self, image, direc, name):
		#delete namefile and facefile
		#if os.path.exists("nameFile.npy"):
		#	os.remove("nameFile.npy")
		#if os.path.exists("faceFile.npy"):
		#	os.remove("faceFile.npy")

		#save image from string
		b64 = base64.b64decode(image[0])
		arq1 = direc[0]+"/"+name[0]+".png"
		with open(arq1, 'wb') as f:
			f.write(b64)

		return True

image = sys.argv[1]
direc = sys.argv[2]
th = sys.argv[3]
mode = sys.argv[4]

#print(direc)

faces = FaceRecognition()
print(faces.mainReco(image, direc, th, mode))