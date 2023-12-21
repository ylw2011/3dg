using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventHandle : MonoBehaviour
{
    public GameObject TriggerObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void RunTrigger()
    {
        TriggerObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnTriggerEnter(Collider other)
    {
        //Debug.Log("Collider Enter");
        RunTrigger();
    }
    void OnTriggerStay(Collider other)
    {
        //Debug.Log("Collider Stay");
    }
    void OnTriggerExit(Collider other)
    {
        //Debug.Log("Collider Exit");
    }

}
