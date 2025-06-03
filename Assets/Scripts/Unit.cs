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
    public HexTile currentTile;

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

    public void PlaceUnit(HexTile tile)
    {
        currentTile = tile;
        Debug.Log($"[배치] unit.currentTile set: {currentTile != null}, tile: {tile.coordinates}");
    }

    public void MoveUnit(HexTile targetTile)
    {
        currentTile = targetTile;
        Debug.Log($"[이동] unit.currentTile set: {currentTile != null}, tile: {targetTile.coordinates}");
    }
} 