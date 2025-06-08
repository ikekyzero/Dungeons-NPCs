using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class CombatSystem : MonoBehaviour
{
    [Header("Настройки атаки")]
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackStaminaCost = 10f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private LayerMask enemyLayer;

    private Animator animator;
    private Player player;
    private PlayerController playerController;
    private bool isAttacking = false;
    private int comboStep = 0;
    private float comboTimer = 0f;
    private const float comboWindow = 1f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        player = GetComponent<Player>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }

    public void PerformAttack()
    {
        if (isAttacking || !player.Stamina.Use(attackStaminaCost)) return;

        isAttacking = true;
        animator.SetTrigger(playerController.animIDAttack);
        StartCoroutine(AttackCoroutine(attackDamage));
        AdvanceCombo();
    }

    private IEnumerator AttackCoroutine(float damage)
    {
        yield return null; // Ждем кадр для начала анимации

        yield return new WaitForSeconds(0.2f); // Время до удара


        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward * attackRange / 2, attackRange / 2, enemyLayer);
        foreach (Collider enemy in hitEnemies)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(0.5f); // Время окончания анимации

        isAttacking = false;
    }

    private void AdvanceCombo()
    {
        if (comboTimer > 0)
        {
            comboStep++;
            if (comboStep >= 3) // Максимум 3 удара в комбо
            {
                ResetCombo();
            }
        }
        else
        {
            comboStep = 1;
        }
        comboTimer = comboWindow;
    }

    private void ResetCombo()
    {
        comboStep = 0;
        comboTimer = 0;
    }
    
    private void OnDrawGizmos() => Gizmos.DrawSphere(transform.position + transform.forward * attackRange / 2, attackRange / 2);
}