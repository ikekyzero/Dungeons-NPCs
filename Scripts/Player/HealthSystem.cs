using UnityEngine;

[System.Serializable]
public class HealthSystem
{
    [SerializeField] public float currentHealth;
    [SerializeField] public float maxHealth;
    public bool IsDead => currentHealth <= 0;

    public HealthSystem(int maxHealth)
    {
        this.maxHealth = maxHealth;
        this.currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth = Mathf.Max(0, currentHealth - damage);
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }

    public void Reset()
    {
        currentHealth = maxHealth;
    }
}