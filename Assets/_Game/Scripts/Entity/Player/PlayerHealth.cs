using UnityEngine;

public class PlayerHealth : Health
{
    protected override void Start()
    {
        base.Start();
        OnDeath += HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        Debug.Log("Player died! Show Game Over.");
    }

    public override void TakeDamage(int amount)
    {
        int reduced = Mathf.Max(amount - 5, 0); // contoh: armor 5
        base.TakeDamage(reduced);
    }
}