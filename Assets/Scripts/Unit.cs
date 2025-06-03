using UnityEngine;

public class Unit : MonoBehaviour
{
    public int playerId;
    public int maxHealth = 100;
    public int currentHealth;
    public int attackPower = 20;
    public int moveRange = 2;
    public bool hasMoved = false;
    public bool hasAttacked = false;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void ResetTurn()
    {
        hasMoved = false;
        hasAttacked = false;
    }

    public bool CanAct()
    {
        return !hasMoved || !hasAttacked;
    }
} 