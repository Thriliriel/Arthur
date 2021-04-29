using System.Collections;
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

    private void Awake()
    {
        informName.SetActive(false);
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

        StreamWriter sr = File.CreateText("whichArthur.txt");
        sr.WriteLine(which);
        sr.WriteLine(whichAgent);
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
