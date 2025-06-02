using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class ThirdPersonController : MonoBehaviour
{
    [Header("Движение игрока")]
    [Tooltip("Скорость передвижения персонажа в м/с")]
    public float moveSpeed = 2.0f;

    [Tooltip("Скорость бега персонажа в м/с")]
    public float sprintSpeed = 5.335f;

    [Tooltip("Скорость поворота персонажа для направления движения")]
    [Range(0.0f, 0.3f)]
    public float rotationSmoothTime = 0.12f;

    [Tooltip("Скорость изменения скорости (ускорение/замедление)")]
    public float speedChangeRate = 10.0f;

    [Header("Прыжок игрока")]
    [Tooltip("Высота прыжка персонажа")]
    public float jumpHeight = 1.2f;

    [Tooltip("Время задержки перед следующим прыжком (0 для мгновенного повторения)")]
    public float jumpTimeout = 0.50f;

    [Tooltip("Время задержки перед вхождением в состояние падения")]
    public float fallTimeout = 0.15f;

    [Header("Проверка земли")]
    [Tooltip("Находится ли персонаж на земле (не встроенная проверка CharacterController)")]
    public bool isGrounded = true;

    [Tooltip("Смещение проверки земли (для неровных поверхностей)")]
    public float groundedOffset = -0.14f;

    [Tooltip("Радиус проверки земли (должен соответствовать радиусу CharacterController)")]
    public float groundedRadius = 0.28f;

    [Tooltip("Слои, которые считаются землей")]
    public LayerMask groundLayers;

    [Header("Камера Cinemachine")]
    [Tooltip("Объект, за которым следует камера Cinemachine")]
    public GameObject cinemachineCameraTarget;

    [Tooltip("Максимальный угол наклона камеры вверх (в градусах)")]
    public float topClamp = 70.0f;

    [Tooltip("Максимальный угол наклона камеры вниз (в градусах)")]
    public float bottomClamp = -30.0f;

    [Tooltip("Дополнительный угол для точной настройки камеры")]
    public float cameraAngleOverride = 0.0f;

    [Tooltip("Фиксация позиции камеры по всем осям")]
    public bool lockCameraPosition = false;

    [Header("Аудио")]
    public AudioClip landingAudioClip;
    public AudioClip[] footstepAudioClips;
    [Range(0, 1)] public float footstepAudioVolume = 0.5f;

    [Header("Здоровье")]
    public int maxHealth = 100;
    private int currentHealth;
    public GameObject lowHealthVignettePanel;
    public GameObject bloodEffect;

    // Переменные для Cinemachine
    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;

    // Переменные движения игрока
    private float currentSpeed;
    private float animationBlend;
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    private float verticalVelocity;
    private float terminalVelocity = 53.0f;

    // Переменные таймаутов
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;

    // Идентификаторы анимации
    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDMotionSpeed;
    private int animIDAttack;
    private int animIDDamage;

    // Ссылки на компоненты
    private PlayerInput playerInput;
    private Animator animator;
    private CharacterController controller;
    private GameInputs gameInputs;
    private GameObject mainCamera;

    private const float threshold = 0.01f;
    private bool hasAnimator;

    private void Awake()
    {
        // Получение ссылки на главную камеру
        if (mainCamera == null)
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;

        hasAnimator = TryGetComponent(out animator);
        controller = GetComponent<CharacterController>();
        gameInputs = GetComponent<GameInputs>();
        playerInput = GetComponent<PlayerInput>();

        AssignAnimationIDs();
        currentHealth = maxHealth;

        // Сброс таймаутов при старте
        jumpTimeoutDelta = jumpTimeout;
        fallTimeoutDelta = fallTimeout;
    }

    private void Update()
    {
        hasAnimator = TryGetComponent(out animator);

        HandleJumpAndGravity();
        CheckGrounded();
        HandleMovement();
        ProcessAttackInput();

        UpdateLowHealthUI();
    }

    private void LateUpdate()
    {
        HandleCameraRotation();
    }

    private void AssignAnimationIDs()
    {
        animIDSpeed = Animator.StringToHash("Speed");
        animIDGrounded = Animator.StringToHash("Grounded");
        animIDJump = Animator.StringToHash("Jump");
        animIDFreeFall = Animator.StringToHash("FreeFall");
        animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        animIDAttack = Animator.StringToHash("Attack");
        animIDDamage = Animator.StringToHash("Damage");
    }

    private void CheckGrounded()
    {
        // Установка позиции сферы с учетом смещения
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);

        // Обновление аниматора, если он используется
        if (hasAnimator)
        {
            animator.SetBool(animIDGrounded, isGrounded);
        }
    }

    private void HandleCameraRotation()
    {
        // Если есть ввод и позиция камеры не зафиксирована
        if (gameInputs.look.sqrMagnitude >= threshold && !lockCameraPosition)
        {
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            cinemachineTargetYaw += gameInputs.look.x * deltaTimeMultiplier;
            cinemachineTargetPitch += gameInputs.look.y * deltaTimeMultiplier;
        }

        // Ограничение поворотов до 360 градусов
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);

        // Установка поворота цели Cinemachine
        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride, cinemachineTargetYaw, 0.0f);
    }

    private void HandleMovement()
    {
        // Установка целевой скорости в зависимости от бега
        float targetSpeed = gameInputs.sprint ? sprintSpeed : moveSpeed;

        // Если нет ввода движения, скорость = 0
        if (gameInputs.move == Vector2.zero) targetSpeed = 0.0f;

        // Текущая горизонтальная скорость
        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = gameInputs.analogMovement ? gameInputs.move.magnitude : 1f;

        // Ускорение или замедление до целевой скорости
        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            currentSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);
            currentSpeed = Mathf.Round(currentSpeed * 1000f) / 1000f; // Округление до 3 знаков
        }
        else
        {
            currentSpeed = targetSpeed;
        }

        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
        if (animationBlend < 0.01f) animationBlend = 0f;

        // Нормализация направления ввода
        Vector3 inputDirection = new Vector3(gameInputs.move.x, 0.0f, gameInputs.move.y).normalized;

        // Поворот персонажа в направлении движения
        if (gameInputs.move != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;

        // Перемещение персонажа
        controller.Move(targetDirection.normalized * (currentSpeed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

        // Обновление аниматора, если он используется
        if (hasAnimator)
        {
            animator.SetFloat(animIDSpeed, animationBlend);
            animator.SetFloat(animIDMotionSpeed, inputMagnitude);
        }
    }

    private void HandleJumpAndGravity()
    {
        if (isGrounded)
        {
            fallTimeoutDelta = fallTimeout;

            if (hasAnimator)
            {
                animator.SetBool(animIDJump, false);
                animator.SetBool(animIDFreeFall, false);
            }

            if (verticalVelocity < 0.0f)
            {
                verticalVelocity = -2f;
            }

            if (gameInputs.jump && jumpTimeoutDelta <= 0.0f)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);

                if (hasAnimator)
                {
                    animator.SetBool(animIDJump, true);
                }
            }

            if (jumpTimeoutDelta >= 0.0f)
            {
                jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            jumpTimeoutDelta = jumpTimeout;

            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                if (hasAnimator)
                {
                    animator.SetBool(animIDFreeFall, true);
                }
            }

            gameInputs.jump = false;
        }

        if (verticalVelocity < terminalVelocity)
        {
            verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }
    }

    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        Gizmos.color = isGrounded ? transparentGreen : transparentRed;
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z), groundedRadius);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f && footstepAudioClips.Length > 0)
        {
            int index = Random.Range(0, footstepAudioClips.Length);
            AudioSource.PlayClipAtPoint(footstepAudioClips[index], transform.TransformPoint(controller.center), footstepAudioVolume);
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(landingAudioClip, transform.TransformPoint(controller.center), footstepAudioVolume);
        }
    }

    private void ProcessAttackInput()
    {
        if (isGrounded && gameInputs.attack)
        {
            gameInputs.attack = false;
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        Debug.Log("Атака выполнена!");
        if (hasAnimator)
        {
            animator.SetTrigger(animIDAttack);
        }
        // Добавить логику атаки (например, обнаружение врагов, нанесение урона)
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Игрок получил {damage} урона, осталось {currentHealth} здоровья");
        if (hasAnimator)
        {
            animator.SetTrigger(animIDDamage);
            bloodEffect.SetActive(true);
            StartCoroutine(DisableBloodEffectCoroutine());
        }
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Игрок умер");
        // Добавить логику смерти (например, проиграть анимацию смерти, перезапустить уровень)
    }

    private void UpdateLowHealthUI()
    {
        if (lowHealthVignettePanel != null)
        {
            lowHealthVignettePanel.SetActive(currentHealth < 30);
        }
    }

    private IEnumerator DisableBloodEffectCoroutine()
    {
        yield return new WaitForSeconds(0.7f);
        if (bloodEffect != null)
        {
            bloodEffect.SetActive(false);
        }
    }

    private bool IsCurrentDeviceMouse => playerInput.currentControlScheme == "KeyboardMouse";
}