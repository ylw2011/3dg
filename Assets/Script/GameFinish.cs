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
            //�C���F���ؼе���
            GameSuccess.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
