using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class GameUI : MonoBehaviour
{
    public TextAsset localizationFile; //tw.txt文件
    public Button[] locbutton;    
    string[] localizedText;

    // Start is called before the first frame update
    void Start()
    {
        if (localizationFile != null)
        {
            localizedText = localizationFile.text.Split('\n');
        }
        if (localizedText.Length >= 2)
        {
            locbutton[0].GetComponentInChildren<TMP_Text>().text = localizedText[0].Trim();
            locbutton[1].GetComponentInChildren<TMP_Text>().text = localizedText[1].Trim();
        }
        else
        {
            Debug.LogError("Insufficient lines in the localization file for two buttons.");
        }
    }
    /*void ShowNextLocalizationText()
    {
        if (currentIndex < localizedText.Length)
        {
            localizedButton[currentIndex].GetComponentInChildren<TMP_Text>().text = localizedText[currentIndex].Trim();
            currentIndex++;
        }
        else
        {
            Debug.Log("Reached the end of the localization text.");
        }
    }*/
        // Update is called once per frame
        void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    public void LoadGame()
    {
        //SceneManager.LoadScene(1);
        SceneManager.LoadScene("LV1");
    }
}
