using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCTRL : MonoBehaviour
{
    private Animator animator;
    private List<string> emotionsList, agentModeList, gazeModeList;

    public Dropdown ddMenu, ddAgentMode, ddGazeMode;
    public Toggle tgFollowMouse, tgSaccade;
    public Button resetHead;
    public EyeCTRL eyeCTRL;
    public HeadCTRL headCTRL;
    public string oldTrigger;

    private void Awake()
    {
        Application.targetFrameRate = 60;
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = gameObject.GetComponent<Animator>();
        oldTrigger = null;
        CreateEmotionsList();
        //CreateEmotionsDDMenu();
        CreateAgentModeList();
        //CreateAgentModeDDMenu();
        CreateGazeModeList();
        //CreateGazeModeDDMenu();
        //CreateTGFollowMouse();
        //CreateTGSaccade();
        //CreateResetHead();
    }

    // Emotions dropdown menu
    private void CreateEmotionsList()
    {
        emotionsList = new List<string>();
        emotionsList.Add("Anger");
        emotionsList.Add("Disgust");
        emotionsList.Add("Doubt");
        emotionsList.Add("Fear");
        emotionsList.Add("Friendly");
        //emotionsList.Add("Idle1"); // Same animation as "Friendly"
        emotionsList.Add("Joy");
        emotionsList.Add("Sadness");
        emotionsList.Add("Sleep");
        emotionsList.Add("Surprise");
        emotionsList.Add("Worried");
    }

    private void CreateEmotionsDDMenu()
    {
        ddMenu.onValueChanged.AddListener(delegate { PlayAnimation(ddMenu); });
        ddMenu.ClearOptions();
        ddMenu.AddOptions(emotionsList);
        ddMenu.gameObject.SetActive(true);
    }

    private void PlayAnimation(Dropdown menu)
    {
        //Debug.Log("E: " + emotionsList[menu.value]);
        if (oldTrigger != null) animator.ResetTrigger(oldTrigger);
        oldTrigger = emotionsList[menu.value];
        animator.SetTrigger(oldTrigger);
    }

    //new play animation, with string only
    public void PlayAnimation(string menu)
    {
        //get the index according the emotion
        int index;
        switch (menu)
        {
            case "anger":
                index = 0;
                break;
            case "disgust":
                index = 1;
                break;
            case "fear":
                index = 3;
                break;
            case "joy":
                index = 5;
                break;
            case "sadness":
                index = 6;
                break;
            case "sleep":
                index = 7;
                break;
            case "surprise":
                index = 8;
                break;
            default:
                index = 4;
                break;
        }

        //Debug.Log("E: " + emotionsList[menu.value]);
        if (oldTrigger != null) animator.ResetTrigger(oldTrigger);
        oldTrigger = emotionsList[index];
        animator.SetTrigger(oldTrigger);
    }

    // Agent mode dropdown menu
    private void CreateAgentModeList()
    {
        agentModeList = new List<string>();
        agentModeList.Add("Listening");
        agentModeList.Add("Talking");
    }

    private void CreateAgentModeDDMenu()
    {
        ddAgentMode.onValueChanged.AddListener(delegate { ChangeAgentMode(ddAgentMode); });
        ddAgentMode.ClearOptions();
        ddAgentMode.AddOptions(agentModeList);
        ddAgentMode.gameObject.SetActive(true);
    }

    private void ChangeAgentMode(Dropdown menu)
    {
        eyeCTRL.SetAgentMode(agentModeList[menu.value].ToLower());
    }

    // Gaze mode dropdown menu
    private void CreateGazeModeList()
    {
        gazeModeList = new List<string>();
        gazeModeList.Add("Away");
        gazeModeList.Add("Mutual");
    }

    private void CreateGazeModeDDMenu()
    {
        ddGazeMode.onValueChanged.AddListener(delegate { ChangeGazeMode(ddGazeMode); });
        ddGazeMode.ClearOptions();
        ddGazeMode.AddOptions(gazeModeList);
        ddGazeMode.gameObject.SetActive(true);
    }

    private void ChangeGazeMode(Dropdown menu)
    {
        eyeCTRL.SetGazeMode(gazeModeList[menu.value].ToLower());
    }

    //  Follow mouse toggle
    private void CreateTGFollowMouse()
    {
        tgFollowMouse.onValueChanged.AddListener(delegate { ChangeFollowMouse(tgFollowMouse); });
        tgFollowMouse.gameObject.SetActive(true);
    }

    private void ChangeFollowMouse(Toggle tg)
    {
        if (tg.isOn) eyeCTRL.SetFollowMouse(true);
        else eyeCTRL.SetFollowMouse(false);
    }

    // Saccade toggle
    private void CreateTGSaccade()
    {
        tgSaccade.onValueChanged.AddListener(delegate { ChangeSaccade(tgSaccade); });
        tgSaccade.gameObject.SetActive(true);
    }

    private void ChangeSaccade(Toggle tg)
    {
        if (tg.isOn) eyeCTRL.SetSaccade(true);
        else eyeCTRL.SetSaccade(false);
    }

    // Reset
    private void CreateResetHead()
    {
        resetHead.onClick.AddListener(delegate { headCTRL.DefaultPosition(); });
        resetHead.gameObject.SetActive(true);
    }
}
