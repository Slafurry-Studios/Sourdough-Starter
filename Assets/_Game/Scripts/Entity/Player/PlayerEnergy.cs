public class PlayerEnergy : Energy
{
    protected override void Start()
    {
        base.Start();

        // Subscribe UI ke event, bukan diurus di base class
        OnEnergyChanged += UpdateUI;
    }

    private void UpdateUI(int current, int max)
    {
        // if (GameplayUIManager.Instance != null)
        //     GameplayUIManager.Instance.UpdateEnergy(current, max);
    }
}