using UnityEngine;
using System.Collections;
using SpeechLib;
using System.Xml;
using System.IO;
using System.Threading;

public class SpeakerController : MonoBehaviour
{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX || UNITY_XBOXONE
    private SpVoice voice;
    private MainController mc;
    private AudioSource aus;

    private void Awake()
    {
        mc = GameObject.Find("MainController").GetComponent<MainController>();
        /*if(mc.agentName == "Bella")
            aus = GameObject.Find("Florisbella").GetComponentInChildren<AudioSource>();
        else if (mc.agentName == "Arthur")
            aus = GameObject.Find("Arthur").GetComponentInChildren<AudioSource>();*/
    }

    // Start is called before the first frame update
    void Start()
    {
        //just to be sure
        if(aus == null)
            if (mc.agentName == "Bella")
                aus = GameObject.Find("Florisbella").GetComponentInChildren<AudioSource>();
            else if (mc.agentName == "Arthur")
                aus = GameObject.Find("Arthur").GetComponentInChildren<AudioSource>();

        voice = new SpVoice();

        voice.Volume = 100; // Volume (no xml)
        voice.Rate = 0;  //   Rate (no xml)

        //get the right voice
        SpObjectTokenCategory tokenCat = new SpObjectTokenCategory();
        tokenCat.SetId(SpeechLib.SpeechStringConstants.SpeechCategoryVoices, false);
        ISpeechObjectTokens tokens = tokenCat.EnumerateTokens(null, null);

        //DAVID OR ZIRA
        foreach (SpObjectToken item in tokens)
        {
            //lets get david for Arthur!
            if (mc.agentName == "Arthur" && item.GetDescription(0).Contains("David"))
            {
                voice.Voice = item;
                break;
            }//or Zira for Bella
            else if (mc.agentName == "Bella" && item.GetDescription(0).Contains("Zira"))
            {
                voice.Voice = item;
                break;
            }
        }
    }

    private IEnumerator LoadFile(string path)
    {
        WWW www = new WWW("file://" + path);
        print("loading " + path);

        AudioClip clip = www.GetAudioClip(false);
        while (!clip.isReadyToPlay)
            yield return www;

        clip.name = Path.GetFileName(path);
        aus.clip = clip;

        aus.Play();
    }

    public void SpeakSomething(string text)
    {
        //voice.Speak(text, SpeechVoiceSpeakFlags.SVSFlagsAsync);

        SpeechStreamFileMode SpFileMode = SpeechStreamFileMode.SSFMCreateForWrite;
        SpFileStream SpFileStream = new SpFileStream();
        SpFileStream.Open(mc.absPath+"arthurHasSpoken.wav", SpFileMode, false);
        voice.AudioOutputStream = SpFileStream;
        voice.Speak(text, SpeechVoiceSpeakFlags.SVSFlagsAsync);
        voice.WaitUntilDone(Timeout.Infinite);//Using System.Threading;
        SpFileStream.Close();

        StartCoroutine(LoadFile(mc.absPath + "arthurHasSpoken.wav"));

        //aus.clip = Resources.Load("arthurHasSpoken.wav") as AudioClip;
        //aus.Play();

        //aus.PlayOneShot(Resources.Load<AudioClip>("arthurHasSpoken.wav"));

        //voice.Pause();
    }
#endif
}
