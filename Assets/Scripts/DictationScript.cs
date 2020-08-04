#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class DictationScript : MonoBehaviour
{
    public GameObject voiceButton;

    [SerializeField]
    private Text m_Hypotheses;

    [SerializeField]
    private InputField m_Recognitions;

    private DictationRecognizer m_DictationRecognizer;

    [SerializeField]
    private Button goAndTranslate;

    void Start()
    {
        //maybe???
        //InitialSilenceTimeoutSeconds -> The time length in seconds before dictation recognizer session ends due to lack of audio input.
        //Dispose -> Disposes the resources this dictation recognizer uses.

        m_DictationRecognizer = new DictationRecognizer();

        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            Debug.LogFormat("Dictation result: {0}", text);
            m_Recognitions.text = text + "\n";
            goAndTranslate.onClick.Invoke();
        };

        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
            Debug.LogFormat("Dictation hypothesis: {0}", text);
            m_Hypotheses.text += text;
        };

        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (completionCause != DictationCompletionCause.Complete)
            {
                Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);

                //try again
                m_DictationRecognizer.Start();
            }
        };

        m_DictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
        };

        //m_DictationRecognizer.Start();
    }

    public void StartDictator()
    {
        m_DictationRecognizer.Start();

        voiceButton.GetComponent<Text>().text = "...";

        voiceButton.GetComponentInParent<Button>().onClick.RemoveAllListeners();
        voiceButton.GetComponentInParent<Button>().onClick.AddListener(StopDictator);
    }

    public void StopDictator()
    {
        m_DictationRecognizer.Stop();

        voiceButton.GetComponent<Text>().text = "Voice";

        voiceButton.GetComponentInParent<Button>().onClick.RemoveAllListeners();
        voiceButton.GetComponentInParent<Button>().onClick.AddListener(StartDictator);
    }
}
#endif