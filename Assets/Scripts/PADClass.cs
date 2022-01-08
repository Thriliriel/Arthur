using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[System.Serializable]
public class PADClass
{
    //pleasure
    private float pleasure;
    //arousal
    private float arousal;
    //dominance
    private float dominance;
    //boredom
    private float boredom;
    //comfort zone
    private Vector3 comfortZone;

    //possible emotions points in PAD
    public Dictionary<string, Vector3> padEmotions;

    public PADClass() { StartPADEmotions(); boredom = 0; comfortZone = Vector3.zero; }

    public PADClass(float newPleasure, float newArousal, float newDominance)
    {
        StartPADEmotions();
        boredom = 0;

        pleasure = newPleasure;
        arousal = newArousal;
        dominance = newDominance;

        comfortZone = new Vector3(pleasure,arousal,dominance);
    }

    private void StartPADEmotions()
    {
        padEmotions = new Dictionary<string, Vector3>();
        padEmotions.Add("Neutral", new Vector3(0, 0, 0));
        //padEmotions.Add("Joyful", new Vector3(0.76f, 0.48f, 0.35f));
        padEmotions.Add("Friendly", new Vector3(0.69f, 0.35f, 0.3f));
        padEmotions.Add("Happy", new Vector3(0.81f, 0.51f, 0.46f));
        padEmotions.Add("Surprised", new Vector3(0.4f, 0.67f, -0.13f));
        padEmotions.Add("Angry", new Vector3(-0.51f, 0.59f, 0.25f));
        padEmotions.Add("Enraged", new Vector3(-0.44f, 0.72f, 0.32f));
        padEmotions.Add("Frustrated", new Vector3(-0.64f, 0.52f, 0.35f));
        padEmotions.Add("Fearful", new Vector3(-0.64f, 0.6f, -0.43f));
        padEmotions.Add("Confused", new Vector3(-0.53f, 0.27f, -0.32f));
        padEmotions.Add("Depressed", new Vector3(-0.72f, -0.29f, -0.41f));
        padEmotions.Add("Bored", new Vector3(-0.65f, -0.62f, -0.33f));
        padEmotions.Add("Sad", new Vector3(-0.63f, -0.27f, -0.33f));
        padEmotions.Add("Disgust", new Vector3(-0.60f, 0.35f, 0.11f));
    }

    public void SetPleasure(float p)
    {
        pleasure = p;
    }
    public void SetArousal(float a)
    {
        arousal = a;
    }
    public void SetDominance(float d)
    {
        dominance = d;
    }
    public void SetBoredom(float b)
    {
        boredom = b;
    }
    public float GetPleasure()
    {
        return pleasure;
    }
    public float GetArousal()
    {
        return arousal;
    }
    public float GetDominance()
    {
        return dominance;
    }
    public float GetBoredom()
    {
        return boredom;
    }

    //Assuming that the absence ofstimuli is responsible for the emergence of boredom (as proposed by [11]), 
    //the degreeof boredom starts to increase linearly over time
    //The linear increase of boredom can be described by the equation Z(t + 1) = Z(t) - b,
    //where the parameter b is again a personality-related aspect of the emotion system
    public void IncreaseBoredom(float O, float C, float A) //more boredom = less O, less C, less A. A-> more important
    {
        //if max boredom is reached, just whatever
        if(boredom <= -1)
        {
            boredom = -1;
            return;
        }

        //divide by 1000, so the max increment is 0.001
        float pBore = ((0.25f * (O)) + (0.25f * (1-C)) + (0.5f * (1-A))) / 1000;

        boredom -= pBore;
        //Debug.Log("Boredom: " + boredom);
    }

    //reset boredom
    public void ResetBoredom()
    {
        boredom = 0;
    }

    //update the pad (for now, based only on what people say)
    public void UpdatePAD(float polarity, string emotion = "")
    {
        if (emotion == "")
        {
            //P = (P + polarity) / 2
            //A = |polarity| + boredom
            //also, depending on the personality of the agent, it can travel more or less distance on PAD dimensions (farther or closer to the comfort zone). 
            //Sajjadi 2019 considered only the E from OCEAN, but we can try to do a bit more later
            float actualDist = Vector3.Distance(new Vector3(pleasure, arousal, dominance), comfortZone);

            float oldArousal = arousal;

            pleasure = (pleasure + polarity) / 2.0f;
            arousal = Mathf.Abs(polarity) + boredom;
            if (arousal > 1) arousal = 1;
            if (arousal < -1) arousal = -1;

            float newDist = Vector3.Distance(new Vector3(pleasure, arousal, dominance), comfortZone);
            //means it is approaching comfort zone, so has bonus
            if (newDist < actualDist)
            {
                if (polarity > 0) pleasure += 0.05f;
                else if (polarity < 0) pleasure -= 0.05f;

                if (arousal < oldArousal) arousal -= 0.05f;
                else if (arousal > oldArousal) arousal += 0.05f;
            }//otherwise, it is getting further from comfort zone. So, has penality.
            else if (newDist > actualDist)
            {
                if (polarity > 0) pleasure -= 0.05f;
                else if (polarity < 0) pleasure += 0.05f;

                if (arousal < oldArousal) arousal += 0.05f;
                else if (arousal > oldArousal) arousal -= 0.05f;
            }
        }
        else
        {
            //if we have a specific emotion, we change to it.
            if (emotion == "Joy") emotion = "Happy";
            if (emotion == "Sadness") emotion = "Sad";

            Vector3 pe = padEmotions[emotion];
            pleasure = pe.x;
            arousal = pe.y;
        }

        //Debug.Log("P = " + pleasure + ", A = " + arousal);
    }
}

/*
Neutral: P=0. A=0, D=0
Joyful: P=.76, A=.48, D=.35
Friendly: P=.69, A=.35, D=.30
Happy: P=.81, A=.51, D=.46
Surprised: P=.40, A=.67, D=-.13
Angry: P=-.51, A=.59, D=.25
Enraged: P=-.44, A=.72, D=.32
Frustrated: P=-.64, A=.52, D=.35
Fearful: P=-.64, A=.60, D=-.43
Confused: P=-.53, A=.27, D=-.32
Depressed: P=-.72, A=-.29, D=-.41
Bored: P=-.65, A=-.62, D=-.33
Sad: P=-.63, A=-.27, D=-.33
Disgust: P=-.60, A=.35, D=.11
*/
