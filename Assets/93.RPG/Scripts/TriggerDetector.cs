using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    public string Tag = string.Empty;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(Tag) == true)
        {
            // dontrequire >> 함수의 처리 결과를 다시 가져올 필요가 없을때 사용
            gameObject.SendMessageUpwards("OnSetTarget", other.gameObject, SendMessageOptions.DontRequireReceiver);
        }
    }
}
