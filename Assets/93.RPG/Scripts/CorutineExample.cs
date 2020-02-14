using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorutineExample : MonoBehaviour
{
    public float LastRunTime = 0.0f;

  // corutine 예제
  IEnumerator Start()
    {
        Debug.Log("start 1 : " + Time.time.ToString());
        yield return StartCoroutine(TestCorutine());
        Debug.Log("start 2 : " + Time.time.ToString());

    }
    IEnumerator TestCorutine()
    {
        Debug.LogWarning("Testcoroutine 1 : " + Time.time.ToString());
        yield return new WaitForSeconds(1.0f);
        Debug.LogWarning("Testcoroutine 2 : " + Time.time.ToString());
        yield return new WaitForSeconds(2.0f);
        Debug.LogWarning("Testcoroutine 3 : " + Time.time.ToString());
    }
}
