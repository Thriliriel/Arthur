using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using Affdex;
using System.Collections;

public class Listener : ImageResultsListener
{
    public Text textArea;
    private MainController mc;
    DebugFeatureViewer dfv;

    public override void onFaceFound(float timestamp, int faceId)
    {
        Debug.Log("Found the face");
    }

    public override void onFaceLost(float timestamp, int faceId)
    {
        Debug.Log("Lost the face");
    }

    //why is it so slow?
    IEnumerator CheckEmotion(Dictionary<int, Face> faces)
    {
        yield return new WaitForSeconds(1f);

        if (faces.Count > 0)
        {
            if (dfv != null)
            {
                //show the face points
                dfv.ShowFace(faces[0]);
            }

            //Debug.Log("Valence: " + faces[0].Emotions[Affdex.Emotions.Valence].ToString());
            mc.foundValence = faces[0].Emotions[Affdex.Emotions.Valence] / 100.0f;

            // Adjust font size to fit the selected platform.
            if ((Application.platform == RuntimePlatform.IPhonePlayer) ||
                (Application.platform == RuntimePlatform.Android))
            {
                textArea.fontSize = 36;
            }
            else
            {
                textArea.fontSize = 18;
            }

            //get the emotions
            string face = faces[0].ToString();
            face = face.Replace("Emotions", "=");
            string[] stuff = face.Split('=');
            stuff = stuff[1].Split('\n');

            float maxEmotion = 0;
            string chosenEmotion = "";

            foreach (string emo in stuff)
            {
                string[] info = emo.Split(':');
                if (info.Length > 1)
                {
                    float thisEmo = System.Convert.ToSingle(info[1]);

                    if (thisEmo > maxEmotion)
                    {
                        maxEmotion = thisEmo;
                        chosenEmotion = info[0].ToLower().Trim();
                    }
                }
            }

            //add this emtion to the mc list, if it is not valence
            if (chosenEmotion == "valence") yield break;

            //if list is full, take the oldest out
            if (mc.foundEmotions.Count >= mc.framesToConsider)
            {
                mc.foundEmotions.RemoveAt(0);
            }

            mc.foundEmotions.Add(chosenEmotion);

            //Debug.Log(chosenEmotion + " - " + maxEmotion.ToString());

            //just show emotion if the list is full
            if (mc.foundEmotions.Count >= mc.framesToConsider)
            {
                //get the emotion with more ocurrences
                int qntInList = 0;
                for (int i = 0; i < mc.framesToConsider; i++)
                {
                    string thisEmotion = mc.foundEmotions[i];
                    int thisQnt = 0;

                    for (int j = 0; j < mc.framesToConsider; j++)
                    {
                        if (thisEmotion == mc.foundEmotions[j])
                        {
                            thisQnt++;
                        }
                    }

                    if (thisQnt > qntInList)
                    {
                        chosenEmotion = thisEmotion;
                        qntInList = thisQnt;
                    }
                }

                //textArea.text = faces[0].ToString();
                //textArea.text = chosenEmotion + " - " + maxEmotion.ToString();
                textArea.text = chosenEmotion;

                //if it is different from the last emotion, we change. 
                //Otherwise, we just update the PAD if all the 5 emotions of the list are the same
                if (mc.foundEmotion != chosenEmotion || qntInList == mc.foundEmotions.Count)
                {
                    //it changes the face. We do not want that now
                    //mc.SetEmotion(chosenEmotion);
                    //update the PAD with the new emotion valence, if not bored.
                    //Deactivated for tests
                    if(!mc.isBored)
                        mc.UpdatePadEmotion(mc.foundValence);
                }
                textArea.CrossFadeColor(Color.white, 0.2f, true, false);
            }
        }
        else
        {
            textArea.CrossFadeColor(new Color(1, 0.7f, 0.7f), 0.2f, true, false);
        }
    }
    
    public override void onImageResults(Dictionary<int, Face> faces)
    {
        if (!mc.isSleeping)
        {
            StartCoroutine(CheckEmotion(faces));
        }
    }

    // Use this for initialization
    void Start () {
        mc = GameObject.Find("MainController").GetComponent<MainController>();
        dfv = GameObject.FindObjectOfType<DebugFeatureViewer>();
    }
}