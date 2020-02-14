using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterExplosionSKill : MonoBehaviour
{
    public float radius = 5.0f;
    public float power = 200.0f;
    public float upwardsModifier = 3.0f;

    private void Start()
    {
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach(Collider col in colliders)
        {
            // fighter를 제외한 나머지만 날려버림
            if (col.gameObject.CompareTag("PlayerAttack") == true)
            {
                continue;
            }
            Rigidbody rigid = col.GetComponent<Rigidbody>();
            if (rigid != null)
            {
                rigid.AddExplosionForce(power, explosionPos, radius, upwardsModifier);
            }
        }
    }
}
