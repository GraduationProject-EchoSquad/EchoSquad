using UnityEngine;

public class EnemyHP : MonoBehaviour
{
    public int maxHealth = 50;
    private int currentHealth;

    Animator enemyAnimator;

    void Start()
    {
        currentHealth = maxHealth;
        enemyAnimator = GetComponent<Animator>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        enemyAnimator.SetTrigger("Die");
        Debug.Log("Zombie died!");
        Destroy(gameObject, 2f);
    }
}
