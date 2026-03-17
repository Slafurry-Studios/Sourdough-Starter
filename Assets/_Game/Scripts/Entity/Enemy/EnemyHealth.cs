using UnityEngine;

public class EnemyHealth : Health
{
    [SerializeField] private GameObject deathVFX;

    protected override void Death()
    {
        // Logika khusus enemy saat mati
        Instantiate(deathVFX, transform.position, Quaternion.identity);
        Destroy(gameObject, 0.5f);
    }
}