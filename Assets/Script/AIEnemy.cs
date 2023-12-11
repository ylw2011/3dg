using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIEnemy : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform target;
    public float ViewDistance=7f;
    public float MeleeDistance = 2f;
    public float WalkSpeed = 1f;
    public int InitHP;
    public int InitAP;
    NavMeshAgent agent;
    private Animator ta;
    private float CurrHP;
    private float CurrAP;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        ta = GetComponent<Animator>();
        agent.stoppingDistance = MeleeDistance;
        agent.speed = WalkSpeed;
        CurrHP=InitHP;
        CurrAP = InitAP;
    }

    // Update is called once per frame
    void Update()
    {
        if (target != null && CurrHP>0)
        {
            if (target.gameObject.GetComponent<Gamekit3D.PlayerController>().m_InAttack)
            {
                agent.isStopped = true;
                ta.SetBool("Walk", false);
                CurrHP--;
                Debug.Log(target.name + " Hit " + gameObject.name + ", left " + CurrHP);
                if(CurrHP<=0)
                    ta.SetTrigger("Die");
                else
                    ta.SetTrigger("Hit");
            }
            else
            {
                float distance = Vector3.Distance(target.position, transform.position);
                if (distance <= ViewDistance)
                {
                    if (distance <= MeleeDistance)
                    {
                        agent.isStopped = true;
                        ta.SetBool("Walk", false);
                        facetarget();
                        ta.SetTrigger("Attack1");
                    }
                    else
                    {
                        agent.isStopped = false;
                        ta.SetBool("Walk", true);
                        agent.SetDestination(target.position);
                    }
                }
                else
                {
                    agent.isStopped=true;                    
                    ta.SetBool("Walk", false);
                }
            }
        }
    }

    void facetarget()
    {
        Vector3 dire = (target.position - transform.position).normalized;
        Quaternion lookr = Quaternion.LookRotation(new Vector3(dire.x, 0, dire.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookr, Time.deltaTime * 3f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, ViewDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, MeleeDistance);
    }
}
