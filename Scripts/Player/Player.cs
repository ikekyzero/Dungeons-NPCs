using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    [Header("Характеристики")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int maxMana = 50;
    [SerializeField] private int maxStamina = 100;
    [SerializeField] private float staminaRegenRate = 5f; // Стамина, восстанавливаемая в секунду
    [SerializeField] private float staminaRegenDelay = 2f; // Задержка перед началом регенерации
    [SerializeField] private float staminaRegenThreshold = 0.1f; // Порог для остановки регенерации

    [Header("Эффекты")]
    [SerializeField] private GameObject bloodParticle;
    [SerializeField] private GameObject bloodVignette;
    [SerializeField] private float lowHealthThreshold = 30f;

    [SerializeField] private HealthSystem health;
    [SerializeField] private ManaSystem mana;
    [SerializeField] private StaminaSystem stamina;
    [SerializeField] private ExperienceSystem experience;

    public HealthSystem Health => health;
    public ManaSystem Mana => mana;
    public StaminaSystem Stamina => stamina;
    public ExperienceSystem Experience => experience;

    public event Action OnDeath;
    public event Action OnRespawn;
    public event Action<float> OnHealthChanged;
    public event Action<float> OnManaChanged;
    public event Action<float> OnStaminaChanged;
    public event Action<int> OnLevelUp;

    private float nextRegenerationTime;

    private void Awake()
    {
        InitializeSystems();
        nextRegenerationTime = 0;
        StartCoroutine(StaminaRegeneration());

        // Инициализация эффектов
        if (bloodParticle != null) bloodParticle.SetActive(false);
        if (bloodVignette != null) bloodVignette.SetActive(false);
    }

    private void InitializeSystems()
    {
        health = new HealthSystem(maxHealth);
        mana = new ManaSystem(maxMana);
        stamina = new StaminaSystem(maxStamina);
        experience = new ExperienceSystem();

        experience.OnLevelUp += (level) => OnLevelUp?.Invoke(level);
    }

    public void TakeDamage(float amount)
    {
        health.TakeDamage(amount);
        OnHealthChanged?.Invoke(health.currentHealth);

        // Активация эффекта крови при получении урона
        if (bloodParticle != null)
        {
            bloodParticle.SetActive(true);
            // Отключаем эффект через 1 секунду
            Invoke(nameof(DisableBloodParticle), 1f);
        }

        // Проверка и обновление эффекта кровавого виньетки
        UpdateBloodVignette();

        if (health.IsDead)
        {
            Die();
        }
    }

    private void DisableBloodParticle()
    {
        if (bloodParticle != null)
        {
            bloodParticle.SetActive(false);
        }
    }

    private void UpdateBloodVignette()
    {
        if (bloodVignette != null)
        {
            bloodVignette.SetActive(health.currentHealth <= lowHealthThreshold);
        }
    }

    public void Heal(float amount)
    {
        health.Heal(amount);
        OnHealthChanged?.Invoke(health.currentHealth);
        UpdateBloodVignette(); // Обновляем виньетку при лечении
    }

    public bool UseMana(float amount)
    {
        if (mana.Use(amount))
        {
            OnManaChanged?.Invoke(mana.currentMana);
            return true;
        }
        return false;
    }

    public bool UseStamina(float amount)
    {
        bool staminaUsed = stamina.Use(amount);
        if (staminaUsed)
        {
            OnStaminaChanged?.Invoke(stamina.currentStamina);
            nextRegenerationTime = Time.time + staminaRegenDelay;
        }
        return staminaUsed;
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Debug.Log("Игрок умер");
    }

    public void Respawn()
    {
        health.Reset();
        mana.Reset();
        stamina.Reset();
        OnRespawn?.Invoke();
        nextRegenerationTime = Time.time; // Немедленная регенерация после возрождения
        UpdateBloodVignette(); // Обновляем виньетку при возрождении
    }

    private System.Collections.IEnumerator StaminaRegeneration()
    {
        while (true)
        {
            if (Time.time >= nextRegenerationTime && 
                stamina.currentStamina < stamina.maxStamina - staminaRegenThreshold)
            {
                float regenAmount = staminaRegenRate * 0.1f; // Регенерация каждые 0.1 секунды
                stamina.Regenerate(regenAmount);
                OnStaminaChanged?.Invoke(stamina.currentStamina);

                // Если стамина достигла максимума, устанавливаем её точно в максимум
                if (stamina.currentStamina >= stamina.maxStamina - staminaRegenThreshold)
                {
                    stamina.currentStamina = stamina.maxStamina;
                    OnStaminaChanged?.Invoke(stamina.currentStamina);
                }
            }
            yield return new WaitForSeconds(0.1f); // Обновление каждые 0.1 секунды
        }
    }
}