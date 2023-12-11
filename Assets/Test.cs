using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    Animator an;
    // Start is called before the first frame update
    void Start()
    {
        an = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q)) an.SetTrigger("Attack1");
        //Vector3.Distance(transform.position,GameObject.Find("Ellen").GetComponent<Transform>().position)
        if(Vector3.Distance(transform.position,GameObject.Find("Ellen").transform.position)<3) an.SetTrigger("Attack1");
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 10);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 3);
    }
}
