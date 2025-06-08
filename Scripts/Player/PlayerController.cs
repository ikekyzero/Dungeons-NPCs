using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Движение игрока")]
    public float moveSpeed = 2.0f;
    public float sprintSpeed = 5.335f;
    [Range(0.0f, 0.3f)] public float rotationSmoothTime = 0.12f;
    public float speedChangeRate = 10.0f;
    public float sprintStaminaCost = 25f; 

    [Header("Прыжок игрока")]
    public float jumpHeight = 1.2f;
    public float jumpTimeout = 0.50f;
    public float fallTimeout = 0.15f;
    public float jumpStaminaCost = 10f; // Стоимость стамины за прыжок

    [Header("Атака игрока")]
    public float attackStaminaCost = 15f; // Стоимость стамины за атаку

    [Header("Проверка земли")]
    public bool isGrounded = true;
    public float groundedOffset = -0.14f;
    public float groundedRadius = 0.28f;
    public LayerMask groundLayers;

    [Header("Камера Cinemachine")]
    public GameObject cinemachineCameraTarget;
    public float topClamp = 70.0f;
    public float bottomClamp = -30.0f;
    public float cameraAngleOverride = 0.0f;
    public bool lockCameraPosition = false;

    [Header("Аудио")]
    public AudioClip landingAudioClip;
    public AudioClip[] footstepAudioClips;
    [Range(0, 1)] public float footstepAudioVolume = 0.5f;

    private float cinemachineTargetYaw;
    private float cinemachineTargetPitch;
    private float currentSpeed;
    private float animationBlend;
    private float targetRotation = 0.0f;
    private float rotationVelocity;
    private float verticalVelocity;
    private float terminalVelocity = 53.0f;
    private float jumpTimeoutDelta;
    private float fallTimeoutDelta;

    private int animIDSpeed;
    private int animIDGrounded;
    private int animIDJump;
    private int animIDFreeFall;
    private int animIDMotionSpeed;
    public int animIDAttack;
    private int animIDDamage;

    private PlayerInput playerInput;
    private Animator animator;
    private CharacterController controller;
    private GameInputs gameInputs;
    private GameObject mainCamera;
    private Player player;
    private CombatSystem combatSystem;

    private const float threshold = 0.01f;
    private bool hasAnimator;

    private void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        player = GetComponent<Player>();
        combatSystem = GetComponent<CombatSystem>();
    }

    private void Start()
    {
        cinemachineTargetYaw = cinemachineCameraTarget.transform.rotation.eulerAngles.y;
        hasAnimator = TryGetComponent(out animator);
        controller = GetComponent<CharacterController>();
        gameInputs = GetComponent<GameInputs>();
        playerInput = GetComponent<PlayerInput>();
        AssignAnimationIDs();
        jumpTimeoutDelta = jumpTimeout;
        fallTimeoutDelta = fallTimeout;
    }

    private void Update()
    {
        HandleJumpAndGravity();
        CheckGrounded();
        HandleMovement();
        ProcessAttackInput();
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
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - groundedOffset, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, groundedRadius, groundLayers, QueryTriggerInteraction.Ignore);
        if (hasAnimator) animator.SetBool(animIDGrounded, isGrounded);
    }

    private void HandleCameraRotation()
    {
        if (gameInputs.look.sqrMagnitude >= threshold && !lockCameraPosition)
        {
            float deltaTimeMultiplier = 1.0f;
            cinemachineTargetYaw += gameInputs.look.x * deltaTimeMultiplier;
            cinemachineTargetPitch += gameInputs.look.y * deltaTimeMultiplier;
        }
        cinemachineTargetYaw = ClampAngle(cinemachineTargetYaw, float.MinValue, float.MaxValue);
        cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, bottomClamp, topClamp);
        cinemachineCameraTarget.transform.rotation = Quaternion.Euler(cinemachineTargetPitch + cameraAngleOverride, cinemachineTargetYaw, 0.0f);
    }

    private void HandleMovement()
    {
        bool isAttacking = hasAnimator && animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
        float targetSpeed = (gameInputs.sprint && !isAttacking) ? sprintSpeed : moveSpeed;
        if (gameInputs.move == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(controller.velocity.x, 0.0f, controller.velocity.z).magnitude;
        float speedOffset = 0.1f;
        float inputMagnitude = gameInputs.analogMovement ? gameInputs.move.magnitude : 1f;

        // Потребление стамины при беге
        if (gameInputs.sprint && gameInputs.move != Vector2.zero && !isAttacking)
        {
            float staminaCost = sprintStaminaCost * Time.deltaTime; // Уменьшаем стамину пропорционально времени
            if (player.Stamina.Use(staminaCost))
            {
                // Бег продолжается, стамина уменьшается
            }
            else
            {
                // Если стамины не хватает, переключаемся на обычную скорость
                targetSpeed = moveSpeed;
            }
        }

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            currentSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * speedChangeRate);
            currentSpeed = Mathf.Round(currentSpeed * 1000f) / 1000f;
        }
        else
        {
            currentSpeed = targetSpeed;
        }

        animationBlend = Mathf.Lerp(animationBlend, targetSpeed, Time.deltaTime * speedChangeRate);
        if (animationBlend < 0.01f) animationBlend = 0f;

        Vector3 inputDirection = new Vector3(gameInputs.move.x, 0.0f, gameInputs.move.y).normalized;
        if (gameInputs.move != Vector2.zero)
        {
            targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        }

        Vector3 targetDirection = Quaternion.Euler(0.0f, targetRotation, 0.0f) * Vector3.forward;
        controller.Move(targetDirection.normalized * (currentSpeed * Time.deltaTime) + new Vector3(0.0f, verticalVelocity, 0.0f) * Time.deltaTime);

        if (hasAnimator)
        {
            animator.SetFloat(animIDSpeed, animationBlend);
            animator.SetFloat(animIDMotionSpeed, inputMagnitude);
        }
    }

    private void HandleJumpAndGravity()
    {
        bool isAttacking = hasAnimator && animator.GetCurrentAnimatorStateInfo(0).IsName("Attack");
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
            if (gameInputs.jump && jumpTimeoutDelta <= 0.0f && !isAttacking)
            {
                float staminaCost = jumpStaminaCost * 100f * Time.deltaTime;
                if (player.Stamina.Use(staminaCost))
                {
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y);
                    if (hasAnimator) animator.SetBool(animIDJump, true);
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
            else if (hasAnimator)
            {
                animator.SetBool(animIDFreeFall, true);
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
        if (isGrounded)
        {
            if (gameInputs.attack)
            {
                gameInputs.attack = false;
                combatSystem.PerformAttack();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        player.TakeDamage(damage);
        animator.SetTrigger(animIDDamage);
    }
}