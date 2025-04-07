using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    // aimable layers
    public LayerMask layerMask;

    private Vector3 currentLookTarget = Vector3.zero;
    private CharacterAnimator characterAnimator;

    public Gun gun;

    // launch position of bullets
    public Transform launchPosition;

    private bool isFiring = false;
    public float fireRate = 0.1f; // 연발 속도 (초 단위)

    void Start()
    {
        characterAnimator = GetComponentInChildren<CharacterAnimator>();
    }

    void Update()
    {
        // get mouse inputs
        if (Input.GetMouseButtonDown(0))
        {
            if (!isFiring)
            {
                isFiring = true;
                characterAnimator?.SetBool("IsShooting", true);
                StartCoroutine(FireBulletContinuously());
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isFiring = false;
            characterAnimator?.SetBool("IsShooting", false);
        }
    }

    IEnumerator FireBulletContinuously()
    {
        while (isFiring)
        {
            FireBullet();
            yield return new WaitForSeconds(fireRate);
        }
    }

    void FireBullet()
    {
        // 총알 발사
        gun.FireBullet(launchPosition.position, transform.forward);

        // 애니메이션 트리거 (Shoot)
        if (characterAnimator != null)
        {
            characterAnimator.SetTrigger("Shoot");
        }
    }
}
