using UnityEngine;
using System;

public abstract class Energy : MonoBehaviour
{
    [Header("Energy Settings")]
    [SerializeField] private int maxEnergy = 100;
    [SerializeField] private float regainEnergyPerSecond = 10f;

    private float currentEnergy;

    // ================= EVENTS =================
    public event Action<int, int> OnEnergyChanged;

    // ================= UNITY LIFECYCLE =================
    protected virtual void Start()
    {
        currentEnergy = maxEnergy;
        NotifyEnergyChanged();
    }

    protected virtual void Update()
    {
        RegainEnergy();
    }

    // ================= PUBLIC API =================
    public virtual bool UseEnergy(int amount)
    {
        if (amount <= 0 || currentEnergy < amount) return false;

        currentEnergy -= amount;
        NotifyEnergyChanged();
        return true;
    }

    // ================= PROTECTED HOOKS =================

    /// <summary>
    /// Override untuk mengubah regen rate di subclass (contoh: berdasarkan mode/kondisi)
    /// </summary>
    protected virtual float GetRegenRate() => regainEnergyPerSecond;

    protected virtual void RegainEnergy()
    {
        if (currentEnergy >= maxEnergy) return;

        currentEnergy = Mathf.Min(currentEnergy + GetRegenRate() * Time.deltaTime, maxEnergy);
        NotifyEnergyChanged();
    }

    // ================= PRIVATE =================
    protected void NotifyEnergyChanged()
    {
        OnEnergyChanged?.Invoke(CurrentEnergy, maxEnergy);
    }

    // ================= READ ONLY =================
    public int CurrentEnergy => Mathf.CeilToInt(currentEnergy);
    public int MaxEnergy => maxEnergy;
}