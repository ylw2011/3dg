using UnityEngine;
using UnityEngine.SceneManagement;

public class UIHandle : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnButtonStartClick()    
    {
        Debug.Log("Button Start Clicked");
        SceneManager.LoadScene("LV1"); 
    }

    public void OnButtonExitClick()    
    {
        Debug.Log("Button Exit Clicked");
        Application.Quit(); 
    }
}
