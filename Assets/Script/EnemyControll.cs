using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControll : MonoBehaviour
{
    public GameObject enemy; // 赋值为你的Enemy预制体
    public float period = 2f; // 指定复制周期
    public int num = 5;

    // Start is called before the first frame update
    void Start()
    {
        GlobalSet.enemycount = num;
        StartCoroutine(SpawnEnemies());
    }

    // Update is called once per frame

    IEnumerator SpawnEnemies()
    {
        // 在指定次数内循环
        for (int i = 0; i < num; i++)
        {
            // 复制Enemy对象
            Vector3 randomOffset = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
            GameObject go=Instantiate(enemy, transform.position + randomOffset, transform.rotation);
            go.SetActive(true);
            // 等待一段时间
            yield return new WaitForSeconds(period);
        }
    }

    void Update()
    {
        
    }
}
