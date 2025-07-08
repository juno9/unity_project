using UnityEngine;

public class Unit : MonoBehaviour
{
    public int playerId;
    public int maxHealth = 10;
    public int currentHealth;
    public int attackPower = 10;
    public int moveRange = 2;
    public int sightRange = 4; // 시야 범위 추가
    public int attackRange = 1;
    public bool hasMoved = false;
    public bool hasAttacked = false;
    public HexTile currentTile;

    public System.Action<Unit, Unit> OnAttack;
    public System.Action<Unit> OnDeath;

    private void Start()
    {
        currentHealth = maxHealth;
        // 체력 텍스트 컴포넌트 추가
        if (GetComponent<HealthText>() == null)
        {
            gameObject.AddComponent<HealthText>();
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"유닛 {name}이(가) {damage} 데미지를 받았습니다. 남은 체력: {currentHealth}");
        
        // 데미지 텍스트 표시
        if (DamageText.Instance != null)
        {
            DamageText.Instance.ShowDamageText(damage, transform.position);
        }
        
        if (currentHealth <= 0)
        {
            OnDeath?.Invoke(this);
            Destroy(gameObject);
        }
    }

    public void Attack(Unit target)
    {
        if (hasAttacked)
        {
            Debug.Log("이미 공격했습니다.");
            return;
        }

        if (target == null)
        {
            Debug.Log("공격할 대상이 없습니다.");
            return;
        }

        if (target.playerId == playerId)
        {
            Debug.Log("아군을 공격할 수 없습니다.");
            return;
        }

        int distance = GetDistanceToUnit(target);
        if (distance > attackRange)
        {
            Debug.Log($"공격 범위를 벗어났습니다. 거리: {distance}, 공격 범위: {attackRange}");
            return;
        }

        target.TakeDamage(attackPower);
        hasAttacked = true;
        
        OnAttack?.Invoke(this, target);
        Debug.Log($"유닛 {name}이(가) {target.name}을(를) 공격했습니다!");
    }

    public int GetDistanceToUnit(Unit target)
    {
        if (target == null || currentTile == null || target.currentTile == null)
            return int.MaxValue;

        return currentTile.GetDistanceTo(target.currentTile);
    }

    public bool CanAttack(Unit target)
    {
        if (hasAttacked || target == null || target.playerId == playerId)
            return false;

        int distance = GetDistanceToUnit(target);
        return distance <= attackRange;
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