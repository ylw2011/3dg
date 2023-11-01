using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandle : MonoBehaviour
{
    public int numoftime;
    public int x,y;

    // Start is called before the first frame update
    void Start()
    {
        x = 0;
        y = 0;
    }

    // Update is called once per frame
    void Update()
    {       
        if(x<10) x = x + 1;
    }
}
