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

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    private InventoryController inventoryController;

    void Start()
    {
        inventoryController = GetComponent<InventoryController>();
    }

    public void OnMove(InputValue value)
    {
        if (inventoryController != null && !inventoryController.IsInventoryOpen)
        {
            MoveInput(value.Get<Vector2>());
        }
    }

    public void OnLook(InputValue value)
    {
        if (cursorInputForLook && inventoryController != null && !inventoryController.IsInventoryOpen)
        {
            LookInput(value.Get<Vector2>());
        }
    }

    public void OnJump(InputValue value)
    {
        if (inventoryController != null && !inventoryController.IsInventoryOpen)
        {
            JumpInput(value.isPressed);
        }
    }

    public void OnSprint(InputValue value)
    {
        if (inventoryController != null && !inventoryController.IsInventoryOpen)
        {
            SprintInput(value.isPressed);
        }
    }

    public void OnAttack(InputValue value)
    {
        if (inventoryController != null && !inventoryController.IsInventoryOpen)
        {
            AttackInput(value.isPressed);
        }
    }

    public void OnInventory(InputValue value)
    {
        InventoryInput(value.isPressed);
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

    public void InventoryInput(bool newInvOpenState)
    {
        invOpen = newInvOpenState;
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