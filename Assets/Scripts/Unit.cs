using UnityEngine;

public class Unit : MonoBehaviour
{
    public int playerId;
    public int maxHealth = 10;
    public int currentHealth;
    public int attackPower = 10;
    public int moveRange = 6;
    public int sightRange = 4; // 시야 범위 추가
    public int attackRange = 15;
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
            
            return;
        }

        if (target == null)
        {
            
            return;
        }

        if (target.playerId == playerId)
        {
            
            return;
        }

        int distance = GetDistanceToUnit(target);
        if (distance > attackRange + 0.1f) // 오차 허용
        {
            
            return;
        }

        // --- 애니메이션 실행 로그 ---
        
        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            
            animator.SetTrigger("doAttack");
        }
        else
        {
            Debug.LogError($"[Animation Log] Animator NOT FOUND on {name} or its children. Animation will not play.");
        }
        // ---

        target.TakeDamage(attackPower);
        hasAttacked = true;
        
        OnAttack?.Invoke(this, target);
        
    }

    public int GetDistanceToUnit(Unit target)
    {
        if (target == null || currentTile == null || target.currentTile == null)
            return int.MaxValue;

        return currentTile.GetDistanceTo(target.currentTile);
    }

    // CanAttack을 원래의 깔끔한 버전으로 되돌립니다.
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
        currentTile = FindFirstObjectByType<HexGrid>().GetTileAt(tile.coordinates);
        
    }

    public void MoveUnit(HexTile targetTile)
    {
        currentTile = FindFirstObjectByType<HexGrid>().GetTileAt(targetTile.coordinates);
        
    }
}