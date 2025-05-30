using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    [SerializeField] 
    public UIInventoryPage inventoryUI;
    public int inventorySize = 10;
    private GameInputs _input;
    private bool invOpen = false;

    public bool IsInventoryOpen => invOpen; // Public property to check inventory state

    void Start()
    {
        inventoryUI.InitializeInventorySize(inventorySize);
        _input = GetComponent<GameInputs>();
    }

    void Update()
    {
        if (_input.invOpen)
        {
            invOpen = !invOpen;
            if (invOpen)
            {
                inventoryUI.Show();
                _input.SetCursorState(false);
            }
            else
            {
                inventoryUI.Hide();
                _input.SetCursorState(true);
            }
            _input.invOpen = false;
        }
    }
}