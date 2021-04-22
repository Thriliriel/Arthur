Arthur is an ECA project which aims to provide a virtual agent with a human-like memory and endowed with empathetic behavior, as well many other features.

Arthur was developed in Unity, tested until version 2020.3.2f1. It was coded in C#, but it also has some Prolog statements. Moreover, in order to make it work as intended, you will need the webservice developed in Python and available in:

https://github.com/Thriliriel/RestAPI

Since it is an Unity project (and it already wraps it all together), there is not much to be done. Some important things although:

- Arthur can translate a spoken sentence to text, as well to answer with his own voice. In order to do this Voice part, it was relied on some Windows libraries (for example, SPVoice and DictationRecognizer). It should not be a problem if not using Windows. If so, Arthur has a tick option named "canSpeak", which can be disabled.

- As commented before, Arthur has a minor Prolog part, which deals with his beliefs. To do so, we installed CSProlog 6.0.0 as an Unity dependency. You can install it manually or through NuGet for Unity (which would be installed also: https://github.com/GlitchEnzo/NuGetForUnity).