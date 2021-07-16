from gensim.models import Word2Vec
import gensim.downloader
import pandas as pd

class Wordvec:
	w2v_model = None

	def __init__(self):
		self.w2v_model = gensim.downloader.load('word2vec-google-news-300')
		#self.w2v_model = gensim.downloader.load('glove-twitter-25')
		self.w2v_model.init_sims(replace=True)

	def run(self, text):
		#print("potato - tomato: ", self.w2v_model.similarity("potato", 'tomato'))
		#print("potato - duck: ", self.w2v_model.similarity("potato", 'duck'))
		#print("potato: ", w2v_model.most_similar(positive=["potato"]))
		text = text["text"][0]

		#split by -
		tokens = text.split('-')
		most_sim = []

		for tok in tokens:
			try:
				sim = self.w2v_model.most_similar(positive=[tok], topn=5)

				for si in sim:
					#print(si)
					most_sim.append(si)
			except:
				#print(("", 0))
				most_sim.append(("", 0))
				most_sim.append(("", 0))
				most_sim.append(("", 0))
				most_sim.append(("", 0))
				most_sim.append(("", 0))

		return pd.DataFrame(most_sim)