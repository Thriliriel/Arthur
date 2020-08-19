using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Calibration : MonoBehaviour
{
    private List<string> emotionsList;
    private string actualEmotion;
    private int actualEmotionNumber;
    public Dropdown menu;
    public Button button;

    void Start()
    {
        CreateEmotionsList();
        CreateEmotionsMenu();
        CreateRunButton();
    }

    private void CreateEmotionsList()
    {
        emotionsList = new List<string>();
        emotionsList.Add("Happiness");
        emotionsList.Add("Sadness");
        emotionsList.Add("Surprise");
        emotionsList.Add("Anger");
        emotionsList.Add("Disgust");
        emotionsList.Add("Fear");
    }

    private void CreateEmotionsMenu()
    {
        menu.onValueChanged.AddListener(delegate { SetEmotion(menu); });
        menu.ClearOptions();
        menu.AddOptions(emotionsList);
        //menu.gameObject.SetActive(true);
    }

    private void CreateRunButton()
    {
        actualEmotion = emotionsList[0];
        actualEmotionNumber = 1;
        button.onClick.AddListener(delegate { DoClustering(); });
    }

    private void SetEmotion(Dropdown menu)
    {
        actualEmotion = emotionsList[menu.value];
        actualEmotionNumber = menu.value + 1;
    }
    private void DoClustering()
    {
        string actualUser = "Willian"; // How could we get user's name?
        string picture = actualUser + "-0" + actualEmotionNumber.ToString() + "_img.png";
        string clustering = "\"Clustering\\Logs\\clustering_"+ picture.Replace(".png", "") + ".txt\"";

        // script, user, emotion, picture, extract from all (even datasets), show user picture
        string command = "clustering.bat " + actualUser + " " + actualEmotion + " " + picture + " False False";

        System.IO.Directory.CreateDirectory("Clustering\\Logs");
        ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/k" + command + " > " + clustering);
        processInfo.CreateNoWindow = false;
        //processInfo.UseShellExecute = false;
        //processInfo.RedirectStandardError = true;
        //processInfo.RedirectStandardOutput = true;

        Process process = Process.Start(processInfo);
    }
}
