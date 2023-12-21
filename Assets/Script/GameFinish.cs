using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFinish : MonoBehaviour
{
    public GameObject GameSuccess;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GlobalSet.enemycount<=0)
        {
            //遊戲達成目標結束
            GameSuccess.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
