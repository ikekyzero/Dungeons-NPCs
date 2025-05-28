using UnityEngine;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    // Основные характеристики
    public int maxHealth = 100; // Максимальное здоровье
    public int currentHealth; // Текущее здоровье

    public int maxMana = 50; // Максимальная мана
    public int currentMana; // Текущая мана

    public int strength = 10; // Сила
    public int agility = 10; // Ловкость
    public int intelligence = 10; // Интеллект

    public int level = 1; // Уровень персонажа
    public int experience = 0; // Опыт персонажа

    // Добавляем ссылки на UI элементы для отображения статистики и уведомлений
    public TextMeshProUGUI statsText;
    public TextMeshProUGUI notificationText;

    void Start()
    {
        // Инициализация текущих значений здоровья и маны
        currentHealth = maxHealth;
        currentMana = maxMana;
        UpdateStatsUI();
        if (notificationText != null)
        {
            notificationText.enabled = false;
        }
    }

    // Метод для получения урона
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
            // Здесь можно добавить логику смерти персонажа
        }
        UpdateStatsUI();
    }

    // Метод для восстановления здоровья
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        UpdateStatsUI();
    }

    // Метод для использования маны
    public bool UseMana(int amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            UpdateStatsUI();
            return true;
        }
        return false;
    }

    // Метод для восстановления маны
    public void RegenerateMana(int amount)
    {
        currentMana += amount;
        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }
        UpdateStatsUI();
    }

    // Метод для получения опыта
    public void GainExperience(int amount)
    {
        experience += amount;
        UpdateStatsUI();
        // Здесь можно добавить логику повышения уровня
    }

    // При прокачке навыка (повышении силы) также отображаем уведомление
    public void IncreaseStrength(int amount)
    {
        strength += amount;
        UpdateStatsUI();
        StartCoroutine(ShowSkillUpgradeNotification("Сила повышена!"));
    }

    void Update()
    {
        
    }

    // Метод для обновления текста статистики на канвасе
    private void UpdateStatsUI()
    {
        if (statsText != null)
        {
            statsText.text = $"Уровень: {level}\n" +
                             $"Здоровье: {currentHealth}/{maxHealth}\n" +
                             $"Мана: {currentMana}/{maxMana}\n" +
                             $"Сила: {strength}\n" +
                             $"Ловкость: {agility}\n" +
                             $"Интеллект: {intelligence}\n" +
                             $"Опыт: {experience}";
        }
    }

    // Короутина для отображения уведомления при прокачке навыка
    private System.Collections.IEnumerator ShowSkillUpgradeNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.enabled = true;
            yield return new WaitForSeconds(2.0f);
            notificationText.enabled = false;
        }
    }
}
