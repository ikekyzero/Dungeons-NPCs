using UnityEngine;

[System.Serializable]
public class StaminaSystem
{
    [SerializeField] public float currentStamina;
    [SerializeField] public float maxStamina;

    public StaminaSystem(int maxStamina)
    {
        this.maxStamina = maxStamina;
        this.currentStamina = maxStamina;
    }

    public bool Use(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            return true;
        }
        return false;
    }

    public void Regenerate(float amount)
    {
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
    }

    public void Reset()
    {
        currentStamina = maxStamina;
    }
}