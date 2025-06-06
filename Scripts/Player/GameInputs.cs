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

    public InventorySystem inventorySystem;

    void Start()
    {
        inventorySystem = GetComponent<InventorySystem>();
        if (inventorySystem != null)
        {
            invOpen = false;
        }
        else
        {
            Debug.LogWarning("InventorySystem не найден на объекте!");
        }
    }

    public void OnMove(InputValue value)
    {
        if (CanProcessInput())
        {
            MoveInput(value.Get<Vector2>());
        }
    }

    public void OnLook(InputValue value)
    {
        if (cursorInputForLook && CanProcessInput())
        {
            LookInput(value.Get<Vector2>());
        }
    }

    public void OnJump(InputValue value)
    {
        if (CanProcessInput())
        {
            JumpInput(value.isPressed);
        }
    }

    public void OnSprint(InputValue value)
    {
        if (CanProcessInput())
        {
            SprintInput(value.isPressed);
        }
    }

    public void OnAttack(InputValue value)
    {
        if (CanProcessInput())
        {
            AttackInput(value.isPressed);
        }
    }

    public void OnInventory(InputValue value)
    {
        if (value.isPressed)
        {
            if (inventorySystem != null)
            {
                inventorySystem.ToggleInventory();
                invOpen = inventorySystem.invOpen;
            }
        }
    }
    public void OnPickup(InputValue value)
    {
        if (CanProcessInput())
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

    private bool CanProcessInput()
    {
        return inventorySystem == null || !inventorySystem.invOpen;
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