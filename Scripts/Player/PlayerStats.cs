using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Характеристики")]
    public int Strength = 10;
    public int Agility = 10;
    public int Intelligence = 10;
    public int Charisma = 10; // Добавляем харизму
    public int Armor = 0;
    public int MaxSleepiness = 100;
    private int currentSleepiness;

    [Header("Нужные объекты")]
    public Slider healthSlider;
    public Slider manaSlider;
    public Slider staminaSlider;

    [Header("Настройки анимации")]
    [SerializeField] private float staminaSliderSpeed = 5f;

    private Player player;
    private float targetStaminaValue;

    private void Start()
    {
        player = GetComponent<Player>();
        currentSleepiness = 0;

        player.OnHealthChanged += UpdateStatsUI;
        player.OnManaChanged += UpdateStatsUI;
        player.OnStaminaChanged += UpdateStatsUI;
        player.OnLevelUp += UpdateStatsUI;

        InitializeSliders();
        UpdateStatsUI(0);
    }

    private void Update()
    {
        if (staminaSlider != null)
        {
            staminaSlider.value = Mathf.Lerp(staminaSlider.value, targetStaminaValue, Time.deltaTime * staminaSliderSpeed);
        }
    }

    private void InitializeSliders()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = player.Health.maxHealth;
            healthSlider.value = player.Health.currentHealth;
        }
        if (manaSlider != null)
        {
            manaSlider.maxValue = player.Mana.maxMana;
            manaSlider.value = player.Mana.currentMana;
        }
        if (staminaSlider != null)
        {
            staminaSlider.maxValue = player.Stamina.maxStamina;
            staminaSlider.value = player.Stamina.currentStamina;
            targetStaminaValue = player.Stamina.currentStamina;
        }
    }

    public void IncreaseSleepiness(int amount)
    {
        currentSleepiness = Mathf.Min(MaxSleepiness, currentSleepiness + amount);
        UpdateStatsUI(0);
    }

    public void DecreaseSleepiness(int amount)
    {
        currentSleepiness = Mathf.Max(0, currentSleepiness - amount);
        UpdateStatsUI(0);
    }

    public void IncreaseStrength(int amount)
    {
        Strength += amount;
        UpdateStatsUI(0);
    }

    public void IncreaseCharisma(int amount) // Новый метод для харизмы
    {
        Charisma += amount;
        UpdateStatsUI(0);
    }

    private void UpdateStatsUI(int _)
    {
        UpdateUI();
    }

    private void UpdateStatsUI(float _)
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (healthSlider != null)
        {
            healthSlider.value = player.Health.currentHealth;
        }
        if (manaSlider != null)
        {
            manaSlider.value = player.Mana.currentMana;
        }
        if (staminaSlider != null)
        {
            targetStaminaValue = player.Stamina.currentStamina;
        }
    }
}