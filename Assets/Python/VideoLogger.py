'''
Registra a diferenca de um frame para outro (quem entrou = new, que permanece = mov, quem saiu = old)
caso alguem nao esteja no banco de dados, tambem guarda sua face para manter a mesma identificacao durante todo o log
'''


import face_recognition as fr
import copy

actions = ['enters at', 'moves to', 'left']

class VideoLogger:
	def __init__(self, video_name, tolerance=0.6):
		self.log_name = video_name[:video_name.index('.')] + '.log'
		self.log = ''
		self.tolerance = tolerance
		self.encodings = []

						#(new,mov,old)
		self.lastFrame = ([],[],[])

	
	def saveLog(self):
		f = open(self.log_name, 'w+')
		f.write(self.log)
		f.close()


	#writes out all changes if there are any
	def logFrame(self, names, locations, encodings, frame_number):
		self._recalculateLastFrame(names, encodings)

		for i in range(len(names)):
						#aka new
			if names[i] in self.lastFrame[0]:
				self._personEnter(names[i], frame_number, locations[i])

		for i in range(len(names)):
						#aka mov
			if names[i] not in self.lastFrame[0]:
				#mov
				self._personMove(names[i], frame_number, locations[i])

					#aka old
		for name in self.lastFrame[2]:
			self._personLeave(name, frame_number)


	def _personAction(self, action, name, frame_number, loc_rec=()):
		string = name + ' ' + actions[action]

		if loc_rec != ():
			x, y, w, z = loc_rec
			string += ' location ' + str(x) + ',' + str(y) + ',' + str(w) + ',' + str(z)

		string += ' in frame ' + str(frame_number) + '\n'
	
		return string


	def _personEnter(self, name,loc_rec,frame_number):
		self.log += self._personAction(0,name,loc_rec,frame_number)


	def _personMove(self, name,loc_rec,frame_number):
		self.log += self._personAction(1,name,loc_rec,frame_number)


	def _personLeave(self, name, frame_number):
		self.log += self._personAction(2, name, frame_number)


	def _getUnkownIndex(self, encoding):
		encs = fr.compare_faces(self.encodings, encoding, self.tolerance)
		if True in encs:
			return encs.index(True)
		else:
			self.encodings.append(encoding)
			return len(self.encodings) - 1


	#also changes the names of 'unknowns' to 'unknown' + id
	def _recalculateLastFrame(self, names, encodings):
		new = []
		mov = []
		old = []

		lf_new, lf_mov, lf_old = self.lastFrame

		lf = lf_new
		lf.extend(lf_mov)

		for i in range(len(names)):
			if names[i] == 'unknown':
				names[i] = names[i] + str(self._getUnkownIndex(encodings[i]))

			if names[i] in lf:
				mov.append(names[i])
			else:
				new.append(names[i])

		#old is the difference between this and the last frame
		old = new.copy()
		old.extend(mov)
		old = list(set(lf) - set(old))

		#update lastFrame
		self.lastFrame = new, mov, old