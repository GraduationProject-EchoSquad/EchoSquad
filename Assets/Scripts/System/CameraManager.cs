using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    public CinemachineCamera Cinemachine;

    public void SetCinemachineTarget(GameObject gameObject)
    {
        if (GameManager.Instance.isTest == false) return;
        if (gameObject == null)
        {
            Cinemachine.Follow = null;
            Cinemachine.gameObject.SetActive(false);
            return;
        }
        Cinemachine.Follow = gameObject.transform;
        Cinemachine.gameObject.SetActive(true);
    }
}
