using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    // spawn시킬 몬스터.
    public GameObject SpawnedMonster = null;

    public List<GameObject> MonsterList = new List<GameObject>();

    // spawn시킬 수 있는 최대마리수
    public int SpawnMaxCount = 50;


    // Start is called before the first frame update
    void Start()
    {
        // 3초 기다렸다가 5초 간격으로 계속 함수호출
        InvokeRepeating("SpawnMonster", 3f, 5f);
    }

    void SpawnMonster()
    {
        if(MonsterList.Count > SpawnMaxCount)
        {
            return;
        }
        Vector3 spawnPos = new Vector3(Random.Range(-100.0f, 100.0f), 1000.0f, Random.Range(-100.0f, 100.0f));

        Ray ray = new Ray(spawnPos, Vector3.down);
        RaycastHit raycasthit = new RaycastHit();
        if(Physics.Raycast(ray, out raycasthit, Mathf.Infinity) == true)
        {
            spawnPos.y = raycasthit.point.y;
        }
        GameObject newMonster = Instantiate(SpawnedMonster, spawnPos, Quaternion.identity);
        MonsterList.Add(newMonster);
    }
}
