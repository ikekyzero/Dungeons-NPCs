using UnityEngine;

[System.Serializable]
public class ManaSystem
{
    [SerializeField] public float currentMana;
    [SerializeField] public float maxMana;

    public ManaSystem(int maxMana)
    {
        this.maxMana = maxMana;
        this.currentMana = maxMana;
    }

    public bool Use(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            return true;
        }
        return false;
    }

    public void Regenerate(float amount)
    {
        currentMana = Mathf.Min(maxMana, currentMana + amount);
    }

    public void Reset()
    {
        currentMana = maxMana;
    }
}