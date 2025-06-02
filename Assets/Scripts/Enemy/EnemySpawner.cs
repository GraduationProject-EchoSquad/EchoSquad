using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;          // �� ������
    public Transform[] spawnPoints;         // 5���� ���� ��ġ
    public float spawnInterval = 20f;       // 20�� ����

    void Start()
    {
        StartCoroutine(SpawnEnemyRoutine());
    }

    IEnumerator SpawnEnemyRoutine()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        int randIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randIndex];
        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
    }
}
