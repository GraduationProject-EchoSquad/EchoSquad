/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 10;

    void Start()
    {
        // 5�� �ڿ� �Ѿ� ����
        Destroy(gameObject, 5f);
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            Debug.Log(contact.thisCollider.name + " hit " + contact.otherCollider.name);
        }

        EnemyHP enemyHP = collision.gameObject.GetComponent<EnemyHP>();
        if (enemyHP != null)
        {
            enemyHP.TakeDamage(damage);
        }
        Destroy(gameObject);
    }
}
*/