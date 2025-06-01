using UnityEngine;

[System.Serializable]
public class Health {
    public int currentHealth;
    public int maxHealth;

    public Health(int currentHealth, int maxHealth) {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
    }

    public void AddHealth(int amount) {
        currentHealth += amount;
        if(currentHealth > maxHealth) {
            currentHealth = maxHealth;
        }
    }
    // Можно добавить дополнительные методы и логику по необходимости
}

public class Player : MonoBehaviour {
    public Health health;

    private void Awake() {
        // Инициализация начального здоровья
        health = new Health(100, 100);
    }

    // Здесь можно добавить другие методы и логику для игрока
}
