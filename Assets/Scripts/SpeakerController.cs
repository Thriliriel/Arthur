using UnityEngine;
using System.Collections;
using SpeechLib;
using System.Xml;
using System.IO;

public class SpeakerController : MonoBehaviour
{
    private SpVoice voice;

    // Start is called before the first frame update
    void Start()
    {
        voice = new SpVoice();

        voice.Volume = 100; // Volume (no xml)
        voice.Rate = 0;  //   Rate (no xml)

        //get the right voice
        SpObjectTokenCategory tokenCat = new SpObjectTokenCategory();
        tokenCat.SetId(SpeechLib.SpeechStringConstants.SpeechCategoryVoices, false);
        ISpeechObjectTokens tokens = tokenCat.EnumerateTokens(null, null);

        foreach (SpObjectToken item in tokens)
        {
            //lets get david!
            if (item.GetDescription(0).Contains("David"))
            {
                voice.Voice = item;
                break;
            }
        }
    }

    public void SpeakSomething(string text)
    {
        voice.Speak(text, SpeechVoiceSpeakFlags.SVSFlagsAsync);

        //voice.Pause();
    }
}
