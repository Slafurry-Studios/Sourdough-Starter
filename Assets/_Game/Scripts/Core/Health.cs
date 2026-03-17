using UnityEngine;
using System;

public abstract class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    private bool isDead = false;

    // ================= EVENTS =================
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    // ================= UNITY LIFECYCLE =================
    protected virtual void Start()
    {
        currentHealth = maxHealth;
        NotifyHealthChanged();
    }

    // ================= PUBLIC API =================
    public virtual void TakeDamage(int amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0);
        NotifyHealthChanged();

        if (currentHealth == 0)
            HandleDeath();
    }

    public virtual void Heal(int amount)
    {
        if (isDead || amount <= 0) return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        NotifyHealthChanged();
    }

    // ================= PROTECTED HOOKS =================

    /// <summary>
    /// Override ini untuk logika tambahan saat mati (animasi, drop item, dll)
    /// Selalu panggil base.OnDeath() jika ingin event tetap jalan
    /// </summary>
    protected virtual void Death()
    {
        // Override di subclass
    }

    // ================= PRIVATE =================
    private void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke();
        Death(); // hook untuk subclass
    }

    private void NotifyHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ================= READ ONLY =================
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;
}