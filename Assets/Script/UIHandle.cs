using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHandle : MonoBehaviour
{
    GameObject stone;

    // Start is called before the first frame update
    void Start()
    {
        stone = GameObject.Find("Cube");
        stone.SetActive(false);
        if (stone != null) Debug.Log("Found");
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);
        if (hit.collider != null)
        {
            //Debug.Log("Name:" + hit.collider.name);
            if(hit.collider.name== "RockGrey1(Polybrush Clone)") stone.SetActive(true);
        }
    }
}
