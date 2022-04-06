﻿using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WhichArthur: MonoBehaviour
{
    public GameObject personName;
    public GameObject informName;
    public GameObject togArthur;
    public GameObject togBella;
    public GameObject globalPath;
    //public GameObject useW2V;
    public GameObject togEmpathy;
    public GameObject scenario;

    private void Awake()
    {
        informName.SetActive(false);
        personName.SetActive(false);
        togEmpathy.SetActive(false);
        GameObject.Find("InputField").SetActive(false);
        GameObject.Find("Button (1)").SetActive(false);
    }

    private void Start()
    {
        string appPath = Application.dataPath;

#if UNITY_EDITOR
        appPath = appPath.Replace("Assets", "");
#else
        appPath = appPath.Replace("Arthur_Data", "");
#endif

        //D:/Docs/UnityProjects/Arthur/
        globalPath.GetComponentInParent<InputField>().text = appPath;

        GameObject.Find("InputField (1)").SetActive(false);
    }

    private IEnumerator Timer()
    {
        informName.SetActive(true);
        yield return new WaitForSeconds(2);
        informName.SetActive(false);
    }

    public void ChangeMode(int which)
    {
        string nominho = "";
        string whichAgent = "";
        string whichScenario = "";
        bool usingW2v = false;
        bool usingEmpathy = true;
        string absPath = globalPath.GetComponent<Text>().text;

        //if it is chat mode (1), we need first to check if the name was informed
        if (which == 1)
        {
            nominho = personName.GetComponent<Text>().text;

            if(nominho == "")
            {
                StartCoroutine(Timer());

                return;
            }
        }

        //get the selected agent
        if (togArthur.GetComponent<Toggle>().isOn) whichAgent = "Arthur";
        else whichAgent = "Bella";

        //using word2vec or not
        //if (useW2V.GetComponent<Toggle>().isOn) usingW2v = true;

        //using empathy
        if (!togEmpathy.GetComponent<Toggle>().isOn) usingEmpathy = false;

        //get chosen scenario
        whichScenario = scenario.GetComponent<Dropdown>().options[scenario.GetComponent<Dropdown>().value].text;

        StreamWriter sr = File.CreateText("whichArthur.txt");
        //absolute path, chat mode ON/OFF, Arthur or Bella, person name (just set if chat mode is active)
        sr.WriteLine(absPath);
        //sr.WriteLine(usingW2v.ToString());
        sr.WriteLine(usingEmpathy.ToString());
        //C:/Users/55549/Desktop/ArBeBuild
        sr.WriteLine(which);
        sr.WriteLine(whichAgent);
        sr.WriteLine(whichScenario);
        sr.WriteLine(nominho);
        sr.Close();
        SceneManager.LoadScene(1);
    }

    public void ChangeAgent(string who)
    {
        //Debug.Log(who);
        bool togArt = togArthur.GetComponent<Toggle>().isOn;
        bool togBel = togBella.GetComponent<Toggle>().isOn;

        if (togBel)
        {
            togArthur.GetComponent<Toggle>().isOn = false;
        }else if (togArt)
        {
            togBella.GetComponent<Toggle>().isOn = false;
        }else if (!togBel && !togArt)
        {
            togArthur.GetComponent<Toggle>().isOn = true;
        }
    }
}
