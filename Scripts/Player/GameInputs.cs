using UnityEngine;
using UnityEngine.InputSystem;

public class GameInputs : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool attack;
    public bool invOpen;
    public bool pickup;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    private InventorySystem inventorySystem;

    void Start()
    {
        inventorySystem = GetComponent<InventorySystem>();
    }

    public void OnMove(InputValue value)
    {
        if (inventorySystem != null && !inventorySystem.invOpen)
        {
            MoveInput(value.Get<Vector2>());
        }
    }

    public void OnLook(InputValue value)
    {
        if (cursorInputForLook && inventorySystem != null && !inventorySystem.invOpen)
        {
            LookInput(value.Get<Vector2>());
        }
    }

    public void OnJump(InputValue value)
    {
        if (inventorySystem != null && !inventorySystem.invOpen)
        {
            JumpInput(value.isPressed);
        }
    }

    public void OnSprint(InputValue value)
    {
        if (inventorySystem != null && !inventorySystem.invOpen)
        {
            SprintInput(value.isPressed);
        }
    }

    public void OnAttack(InputValue value)
    {
        if (inventorySystem != null && !inventorySystem.invOpen)
        {
            AttackInput(value.isPressed);
        }
    }

    public void OnInventory(InputValue value)
    {
        if (inventorySystem != null && value.isPressed)
        {
            inventorySystem.ToggleInventory();
        }
    }
    public void OnPickup(InputValue value) // Новый метод для подбора
    {
        if (inventorySystem != null && !inventorySystem.invOpen)
        {
            PickupInput(value.isPressed);
        }
    }

    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    } 

    public void LookInput(Vector2 newLookDirection)
    {
        look = newLookDirection;
    }

    public void JumpInput(bool newJumpState)
    {
        jump = newJumpState;
    }

    public void SprintInput(bool newSprintState)
    {
        sprint = newSprintState;
    }

    public void AttackInput(bool newAttackState)
    {
        attack = newAttackState;
    }
    public void PickupInput(bool newPickupState)
    {
        pickup = newPickupState;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    public void SetCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}