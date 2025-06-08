using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Характеристики врага")]
    [SerializeField] private int maxHealth = 100; // Максимальное здоровье врага
    [SerializeField] private int damage = 10;     // Урон, наносимый врагом
    [SerializeField] private float attackCooldown = 1.5f; // Задержка между атаками
    [SerializeField] private float moveSpeed = 3f; // Скорость движения

    [Header("Точки патрулирования")]
    [SerializeField] private Transform[] patrolPoints; // Массив точек для патрулирования

    private float currentHealth;         // Текущее здоровье врага
    private float lastAttackTime;      // Время последней атаки
    private Transform playerTransform; // Ссылка на трансформ игрока
    private bool isDead = false;       // Флаг, указывающий, мертв ли враг
    private int currentPatrolIndex = 0; // Индекс текущей точки патрулирования

    private void Start()
    {
        currentHealth = maxHealth; // Инициализация здоровья
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform; // Поиск игрока по тегу
        if (patrolPoints.Length > 0)
        {
            transform.position = patrolPoints[0].position; // Начальная позиция — первая точка патруля
        }
    }

    private void Update()
    {
        if (isDead) return; // Если враг мертв, ничего не делаем

        Patrol(); // Патрулирование между точками

        // Проверка расстояния до игрока для атаки
        if (Vector3.Distance(transform.position, playerTransform.position) < 1.5f)
        {
            AttackPlayer(); // Атака игрока, если он в зоне действия
        }
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return; // Если точек нет, ничего не делаем

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        // Переход к следующей точке, если текущая достигнута
        if (Vector3.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void AttackPlayer()
    {
        if (Time.time >= lastAttackTime + attackCooldown) // Проверка задержки атаки
        {
            PlayerController playerController = playerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TakeDamage(damage); // Нанесение урона игроку
                lastAttackTime = Time.time; // Обновление времени последней атаки
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return; // Если враг мертв, урон не наносится

        currentHealth -= damage; // Уменьшение здоровья
        if (currentHealth <= 0)
        {
            Die(); // Смерть врага при достижении 0 здоровья
        }
    }

    private void Die()
    {
        isDead = true; // Установка флага смерти
        Destroy(gameObject, 1f); // Уничтожение объекта через 1 секунду (можно добавить анимацию смерти)
    }

    private void OnTriggerEnter(Collider other)
    {
        // Оставлено для возможной дополнительной логики
    }
}