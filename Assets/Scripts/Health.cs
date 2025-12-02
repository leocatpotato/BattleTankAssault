using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int Max = 100;
    public int Cur = 100;
    public bool IsEnemy;

    public event Action<Health> onHealthChanged;
    public event Action<Health> onDied;

    private bool _isDead;

    private void Awake()
    {
        Cur = Mathf.Clamp(Cur, 0, Max);
    }

    public void TakeDamage(int amount, bool isEnemyBullet)
    {
        if (_isDead) return;
        if (amount <= 0) return;

        if (IsEnemy && isEnemyBullet == true) return;
        if (!IsEnemy && isEnemyBullet == false) return;

        Cur = Mathf.Max(Cur - amount, 0);
        onHealthChanged?.Invoke(this);

        if (IsEnemy && !isEnemyBullet)
        {
            var ai = GetComponent<TankEnemyAI>();
            if (ai != null)
            {
                ai.NotifyDamaged();
            }
        }

        if (Cur <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        if (_isDead) return;
        if (amount <= 0) return;

        int old = Cur;
        Cur = Mathf.Min(Cur + amount, Max);

        if (Cur != old)
        {
            onHealthChanged?.Invoke(this);
        }
    }


    private void Die()
    {
        if (_isDead) return;
        _isDead = true;

        onDied?.Invoke(this);

        if (LevelGameManager.Instance != null)
        {
            if (IsEnemy)
            {
                LevelGameManager.Instance.OnEnemyKilled(this);
            }
            else
            {
                LevelGameManager.Instance.OnPlayerDied(this);
            }
        }

        Destroy(gameObject);
    }
}