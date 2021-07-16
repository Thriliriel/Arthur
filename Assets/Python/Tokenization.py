import nltk
import sys
import string
import time
from nltk.sentiment import SentimentIntensityAnalyzer
import pandas as pd

from nltk.corpus import stopwords
from nltk.tokenize import sent_tokenize
from nltk.tokenize import word_tokenize
from nltk.corpus import wordnet
#from nltk.stem import PorterStemmer	
#from nltk.stem import SnowballStemmer
from nltk.stem import WordNetLemmatizer

#nltk.download('stopwords')

class Tokenization:         
    def run(self, text):        
        
        if text == "":
            print("false")

        else:
            #remove punctuations
            #text = text.translate(None, string.punctuation)

            #get the sentiment analisys (positive or negative)
            vader_analyzer = SentimentIntensityAnalyzer()
            sentiResult = vader_analyzer.polarity_scores(text)
            #print(sentiResult)
            sentiResult = sentiResult['compound']

            tokens = word_tokenize(text)
            #print(tokens)

            clean_tokens = tokens[:]
 
            #tokens we want to keep
            keepWords = []
            keepWords.append("no")
            keepWords.append("yes")
            keepWords.append("i")
            keepWords.append("me")
            keepWords.append("myself")
            keepWords.append("you")
            keepWords.append("yourself")

            #lemmatizing (the real word: matei -> stem = mat -> lemm = matar)
            #default: nouns. To change: lemmatizer.lemmatize('playing', pos="v")
            #The result could be a verb, noun, adjective, or adverb
            lemmatizer = WordNetLemmatizer()

            i = 0
            for token in clean_tokens:
                clean_tokens[i] = lemmatizer.lemmatize(token, pos="v")
                i = i + 1
 
            #add the tags (noun, verb, etc...)
            #NN = Noun; NNP = proper noun
            clean_tokens = nltk.pos_tag(clean_tokens)

            #print(clean_tokens)

            sr = stopwords.words()
 
            for token in clean_tokens:
                if token[0] in sr and token[0] not in keepWords:
                    #check if it is a verb
                    if token[1] != "VB":
                        clean_tokens.remove(token)

            #now, if the tag is ponctuation, remove it
            #for token in clean_tokens:
            #    if token[1] == '.' or token[1] == ',':
            #        clean_tokens.remove(token)

            #if the clean_tokens is empty, we use the sentence as a whole, so we can send it to chatbot
            if len(clean_tokens) == 0:
                clean_tokens = [text, "fullText"]

            #writeResult = [clean_tokens, [str(sentiResult)]]
            clean_tokens.append([sentiResult, 0])
            writeResult = clean_tokens

            ##erase the file
            #fl = open(textFile, "w")
            #fl.write("");
            #fl.close()

            ##write on the result file
            #fl = open("../../resultToken.txt", "w")
            #fl.write(str(writeResult));
            #fl.close()

            #print (writeResult)

            #word stemming (working -> work)
            #stemmer = PorterStemmer() #Porter: the most used algorithm for stem. Other choice could be Lancaster
            #print(stemmer.stem('increases'))

            #stemming for other languages
            #stemmer = SnowballStemmer('portuguese')
            #print(stemmer.stem('matei'))
        
        return writeResult
        #return pd.DataFrame(writeResult)
        #return pd.DataFrame(clean_tokens)
        #return pd.DataFrame([sentiResult])

sentence = sys.argv[1]
#print(sentence)
tokens = Tokenization()
print(tokens.run(sentence))