The RestAPI provides a webservice where Arthur (or any other other virtual agent) can access some services, like Face Recognition and Tokenization of sentences.

It was developed n Python 3.7.4, so maybe you need to check your version. Moreover, since it provides different services, there are a bunch of imports to be aware of. I will paste many here to make things easier:

from datetime import datetime
import face_recognition
import requests
import pandas as pd
import cherrypy
import nltk
from gensim.models import Word2Vec

You can also run the batch file provided here: "installs.bat". It gets all dependencies listed in the "requirements.txt" file and pip install it. Some of them are already included with the default python package, but others (like nltk) need to be downloaded. In case you dont know how to do it, just open the terminal and type:

pip install "name-of-the-package"

If you have any doubt about other packages, you can open webservice.py to check any missing library.

Concerning NLTK, depending on your installation, you may need to download the stopwords separately. To do so, just run the following command in python:

nltk.download('stopwords')

Just need to run it once, then comment/delete it.

In order to run the project, just run in the command line:

python <dir>/webService.py

If all is good, it should appear that the service is started (it may take a while). Otherwise, check the error displayed (usually some missing library to be "piped" =P)