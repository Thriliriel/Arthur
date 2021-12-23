using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LipSync : MonoBehaviour
{
    public GameObject mouth;
    public bool isJaw;
    int volume = 40;
    int frqLow = 200;
    int frqHigh = 800;
    int fMax = 24000;
    private float y0;
    public AudioSource audio;
    public int bsIndex;
    private float[] freqData;
    private int nSamples;
    private int qSamples = 0;
    private int posFilter = 0;
    private int sizeFilter = 5;
    private float[] filter;
    private float filterSum;

    // Start is called before the first frame update
    void Start()
    {
        nSamples = 256;
        audio = GetComponent<AudioSource>(); // get AudioSource component
        
        //if it is jaw, get the initial scale
        if (isJaw)
        {
            y0 = mouth.transform.localScale.y;
        }
        else //otherwise, get the initial BS value (in our case, it is the index 24 of the blendshapes, put yours)
        {
            y0 = mouth.GetComponent<SkinnedMeshRenderer>().GetBlendShapeWeight(bsIndex);
        }
        freqData = new float[nSamples];
        //audio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        //if it is jaw, just change the scale of the mouth
        if (isJaw)
        {
            //float newY = y0 + BandVol(frqLow, frqHigh) * (volume/4);
            float newY = y0 - MovingAverage(BandVol(frqLow, frqHigh)) * volume / 4;
            mouth.transform.localScale = new Vector3(mouth.transform.localScale.x, newY, mouth.transform.localScale.z);
        }//otherwise, use the BS
        else
        {
            mouth.GetComponent<SkinnedMeshRenderer>().SetBlendShapeWeight(bsIndex, y0 + BandVol(frqLow, frqHigh) * volume * 10);
        }
    }

    private float BandVol(float fLow, float fHigh) 
    {
        fLow = Mathf.Clamp(fLow, 20, fMax); // limit low...
        fHigh = Mathf.Clamp(fHigh, fLow, fMax); // and high frequencies
        // get spectrum: freqData[n] = vol of frequency n * fMax / nSamples
        audio.GetSpectrumData(freqData, 0, FFTWindow.BlackmanHarris); 
        int n1 = (int)Mathf.Floor(fLow* nSamples / fMax);
        int n2 = (int)Mathf.Floor(fHigh* nSamples / fMax);
        float sum = 0;
        // average the volumes of frequencies fLow to fHigh
        for (var i=n1; i<=n2; i++){
            sum += freqData[i];
        }
        return sum / (n2 - n1 + 1);
    }

    private float MovingAverage(float sample)
    { 
        if (qSamples==0) filter = new float[sizeFilter];
        filterSum += sample - filter[posFilter];
        filter[posFilter++] = sample;
        if (posFilter > qSamples) qSamples = posFilter;
        posFilter = posFilter % sizeFilter;
        return filterSum / qSamples;
    }
}
