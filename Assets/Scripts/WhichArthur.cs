using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WhichArthur: MonoBehaviour
{
    public GameObject personName;
    public GameObject informName;

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

        StreamWriter sr = File.CreateText("whichArthur.txt");
        sr.WriteLine(which);
        sr.WriteLine(nominho);
        sr.Close();
        SceneManager.LoadScene(1);
    }
}
