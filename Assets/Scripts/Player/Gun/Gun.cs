using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletSpeed = 20f;

    public void FireBullet(Vector3 position, Vector3 direction)
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, position, Quaternion.LookRotation(direction));
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * bulletSpeed;
        }
    }
}
