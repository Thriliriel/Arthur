using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class EyeCTRLBella : MonoBehaviour
{
    public Vector3 defaultPosition;
    private Coroutine saccadeCoroutine;
    private float xLimit, yLimit, listeningLimit, talkingLimit;
    public bool saccade;
    private string agentMode, gazeMode; // {listening, talking}; {mutual, away}

    public GameObject REye, LEye;

    //main controller
    private MainController mc;

    private GameObject[] iris;
    public Vector3 positionOffsetL, positionOffsetR;

    private void Awake()
    {
        //defaultPosition = transform.position;
        defaultPosition = new Vector3(0, -0.02738886f, 0);
        //xLimit = 1.75f; // Unity world position
        //yLimit = 2.0f; // ""
        xLimit = 0.006f; // Unity world position
        yLimit = 0.004f; // ""
        listeningLimit = 22.7f; // Degrees
        talkingLimit = 27.5f; // ""
        saccade = true;
        saccadeCoroutine = null;
        agentMode = "talking";
        gazeMode = "mutual";
        mc = GameObject.Find("MainController").GetComponent<MainController>();
        //initX = 0f;
        //initY = 0f;
        REye = GameObject.Find("CW_Eye_R_grp");
        LEye = GameObject.Find("CW_Eye_L_grp");
        //ConfigureFocus();
    }

    /*private void ConfigureFocus()
    {
        iris = new GameObject[2];
        iris[0] = LEye;
        iris[1] = REye;
        //positionOffsetL = new Vector3(-0.95f, 1.5f, 7.5f);
        //positionOffsetR = new Vector3(0.95f, 1.5f, 7.5f);
        positionOffsetL = LEye.transform.position;
        positionOffsetR = REye.transform.position;
        iris[0].transform.GetChild(2).GetComponent<PositionConstraint>().translationOffset = positionOffsetL;
        iris[1].transform.GetChild(2).GetComponent<PositionConstraint>().translationOffset = positionOffsetR;
    }*/

    // Update is called once per frame
    void Update()
    {
        if (!mc.isSleeping)
        {
            if (saccade)
            {
                //saccadeOffset = 2f;
                if (saccadeCoroutine == null)
                    saccadeCoroutine = StartCoroutine(Saccade());
            }
            else if (saccadeCoroutine != null)
            {
                StopCoroutine(saccadeCoroutine);
                saccadeCoroutine = null;
            }

            if (!saccade)
            {
                if (saccadeCoroutine != null)
                {
                    StopCoroutine(saccadeCoroutine);
                    saccadeCoroutine = null;
                }
                transform.position = defaultPosition;
            }
        }
    }

    // ---------------------------------------------------------------------------------------------------- //
    // Saccade

    private IEnumerator Saccade()
    {
        /*
         * Needs to implement:
         * - [x] Mutual and away gaze
         * - [ ] Saccade duration
         * - [ ] Saccade velocity
         * - [-] Variable saccade interval
         */

        float magnitude, direction, dirX, dirY, saccadeFrames, time;
        Vector3 newPosition;

        newPosition = new Vector3(0f, 0f, 0f);
        while (true)
        {
            magnitude = -6.9f * Mathf.Log(Random.Range(0f, 15f) / 15.7f);
            //Debug.Log("Mag: " + magnitude);

            // Verifies saccade limits according to Eyes Alive
            // Also converts magnitude from degrees to percentage
            if (agentMode == "listening")
            {
                if (magnitude > listeningLimit) magnitude = listeningLimit;
                magnitude /= listeningLimit;
            }
            else
            {
                if (magnitude > talkingLimit) magnitude = talkingLimit;
                magnitude /= talkingLimit;
            }
            
            direction = Random.Range(0f, 100f);
            //duration = 0.0025f + 0.00024f * magnitude; // 25msec + 2.4msec/deg * A = D0 + d * A = (Equation 1)

            // According to Table 1, Lee et al, Eyes Alive
            if (direction <= 15.54f) // 0 degrees
            {
                dirX = magnitude;
                dirY = 0f;
            }
            else if (direction <= 22f) // 45 degrees
            {
                dirX = magnitude;
                dirY = -1 * magnitude;
            }
            else if (direction <= 39.69f) // 90 degrees
            {
                dirX = 0f;
                dirY = magnitude;
            }
            else if (direction <= 47.13f) // 135 degrees
            {
                dirX = -1 * magnitude;
                dirY = magnitude;
            }
            else if (direction <= 63.93f) // 180 degrees
            {
                dirX = -1 * magnitude;
                dirY = 0f;
            }
            else if (direction <= 71.82f) // 225 degrees
            {
                dirX = -1 * magnitude;
                dirY = -1 * magnitude;
            }
            else if (direction <= 92.2f) // 270 degrees
            {
                dirX = 0f;
                dirY = -1 * magnitude;
            }
            else // 315 degrees
            {
                dirX = magnitude;
                dirY = -1 * magnitude;
            }

            /*
             * As magnitude is converted from degrees to percentage
             * (e.g., 27.5 degrees to 1.0 or 100%), and dirX and
             * dirY are related to magnitude, we multiply dirX and
             * dirY by their respective axis limit in order to
             * obtain the world coordinate equivalent to the inital
             * magnitude in degrees that we've had.
             */

            newPosition.x = xLimit * dirX;
            newPosition.y = defaultPosition.y;
            newPosition.z = yLimit * dirY;

            // Applies transformation
            //transform.Find("Iris").Find("Pupil").transform.localPosition = newPosition;
            REye.transform.Find("CW_pupil_R").transform.localPosition = newPosition;
            LEye.transform.Find("CW_pupil_L").transform.localPosition = newPosition;
            //transform.position = newPosition;

            // Variable interval between saccades
            /*
             * Talking mode:
             * In talking mode, the average mutual gaze and
             * gaze away durations are 93.9±94.9 frames
             * and 27.8±24.0 frames, respectively.
             * 
             * Listening mode:
             * In listening mode, inter-saccadic intervals are obtained
             * using Gaussian random numbers with the duration values
             * given in section 4.3: 237.5±47.1 frames for mutual gaze
             * and 13.0±7.1 frames for gazeaway.
             */

            if (agentMode == "listening")
            {
                if (gazeMode == "mutual") saccadeFrames = Random.Range(190.4f, 284.6f);
                else saccadeFrames = UnityEngine.Random.Range(5.9f, 20.1f);
            }
            else
            {
                if (gazeMode == "mutual") saccadeFrames = Random.Range(0f, 188.8f);
                else saccadeFrames = Random.Range(3.8f, 51.8f);
            }

            /*
             * Considering the amount of frames mencioned
             * above for each agent and gaze modes, and
             * that the video used in "Eyes Alive" was
             * recorded at 30 fps, the random number
             * generated is divided by 30. This is made
             * in order to transform the inter-saccade
             * in order to transform the inter-saccade
             * interval to time, instead of frames
             */

            time = saccadeFrames / 30f;

            //Debug.Log(agentMode + " | " + gazeMode + " | " + time);
            //Debug.Log("MAGNITUDE: " + magnitude + " | " + newPosition.x + " | " + newPosition.y);

            yield return new WaitForSeconds(time);
        }
    }

    // External
    public void SetAgentMode(string agentMode)
    {
        this.agentMode = agentMode;
    }

    public void SetGazeMode(string gazeMode)
    {
        this.gazeMode = gazeMode;
    }

    public void SetSaccade(bool saccade)
    {
        this.saccade = saccade;
    }
}
