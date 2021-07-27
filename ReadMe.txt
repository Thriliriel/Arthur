Arthur is an ECA project which aims to provide a virtual agent with a human-like memory and endowed with empathetic behavior, as well many other features.

Arthur was developed in Unity, tested until version 2020.3.2f1. It was coded in C#, but it also has some Prolog statements. Moreover, in order to make it work as intended, you will need the webservice developed in Python and available in:

https://github.com/Thriliriel/RestAPI

Since it is an Unity project (and it already wraps it all together), there is not much to be done. Some important things although:

- Arthur can translate a spoken sentence to text, as well to answer with his own voice. In order to do this Voice part, it was relied on some Windows libraries (for example, SPVoice and DictationRecognizer). It should not be a problem if not using Windows. If so, Arthur has a tick option named "canSpeak", which can be disabled.

- As commented before, Arthur has a minor Prolog part, which deals with his beliefs. To do so, we installed CSProlog 6.0.0 as an Unity dependency. You can install it manually or through NuGet for Unity (which would be installed also: https://github.com/GlitchEnzo/NuGetForUnity).

- Arthur has LipSync provided by LavStar. You may need to download it from https://assetstore.unity.com/packages/tools/audio/lavstar-lip-audio-visualization-star-166516#reviews

- It should not be a problem, but, just to be sure, try to avoid using punctuation on sentences, besides "?". Also, proper nouns should have a capital first letter (Ex: "Jack", instead of "jack").

- In the menu panel, the last option allows to choose using Word2Vec for Arthur. It is disabled by default, because it is very heavy (around 6GB RAM usage). Use it at your own risk =P.

- If you have the need to restart all over (memory, people known and such), you can do it in a few steps:
 . Inside the folder AutobiographicalStorage, delete the file "episodicMemory.txt", create a backup of "backupepisodicMemoryWordnet.txt" and rename it to "episodicMemory.txt". This way, the memory will be reseted to the beggining.
 . Inside the folder AutobiographicalStorage, clear both "historic.txt" and "smallTalksUsed.txt".
 . Inside the folder AutobiographicalStorage, enter the "Images" folder and delete all pictures, but Arthur and Bella images.
 . Inside the folder RestAPI, delete the files "facefile.npy", "namefile.npy" and "camImage.png". Also, delete all images inside the folder "Data".
 . In the root folder, open the "nextId.txt" file and reset it to the original values (10020 in first line, 10011 in second line).