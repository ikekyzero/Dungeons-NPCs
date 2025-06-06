using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private InventoryData inventoryData;
    [SerializeField] private UIInventoryPage inventoryUI;
    [SerializeField] private AudioClip dropClip;
    [SerializeField] private AudioSource audioSource;

    private GameInputs _input;
    private bool _invOpen = false;
    public bool invOpen => _invOpen;

    private void Awake()
    {
        _input = GetComponent<GameInputs>();
        if (_input == null)
        {
            Debug.LogWarning("GameInputs не найден на объекте!");
        }
        _invOpen = false;
    }

    private void Start()
    {
        inventoryData.Initialize();
        PrepareUI();
        inventoryData.OnInventoryUpdated += UpdateInventoryUI;
        inventoryUI.Hide();
    }

    private void PrepareUI()
    {
        inventoryUI.InitializeInventoryUI(inventoryData.Size);
        inventoryUI.OnDescriptionRequested += HandleDescriptionRequest;
        inventoryUI.OnSwapItems += HandleSwapItems;
        inventoryUI.OnStartDragging += HandleDragging;
        inventoryUI.OnItemActionRequested += HandleItemActionRequest;
    }

    private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState)
    {
        inventoryUI.ResetAllItems();
        foreach (var item in inventoryState)
        {
            inventoryUI.UpdateData(item.Key, item.Value.Item.Image, item.Value.Quantity);
        }
    }

    private void HandleDescriptionRequest(int itemIndex)
    {
        InventoryItem item = inventoryData.GetItemAt(itemIndex);
        if (item.IsEmpty)
        {
            inventoryUI.ResetSelection();
            return;
        }
        string description = PrepareDescription(item);
        inventoryUI.UpdateDescription(itemIndex, item.Item.Image, item.Item.Name, description);
    }

    private string PrepareDescription(InventoryItem item)
    {
        if (item.Item == null)
        {
            Debug.LogWarning("Item is null in PrepareDescription. Returning empty description.", this);
            return "No description available.";
        }

        var sb = new System.Text.StringBuilder();
        if (string.IsNullOrEmpty(item.Item.Description))
        {
            sb.Append("No description provided.");
        }
        else
        {
            sb.Append(item.Item.Description);
        }
        sb.AppendLine();
        if (item.Parameters != null && item.Parameters.Count > 0)
        {
            foreach (var param in item.Parameters)
            {
                sb.Append($"{param.Key}: {param.Value}");
                sb.AppendLine();
            }
        }
        return sb.ToString();
    }

    private void HandleDragging(int itemIndex)
    {
        InventoryItem item = inventoryData.GetItemAt(itemIndex);
        if (!item.IsEmpty)
        {
            inventoryUI.CreateDraggedItem(item.Item.Image, item.Quantity); // Исправлено с item.Value.Quantity на item.Quantity
        }
    }

    private void HandleSwapItems(int itemIndex1, int itemIndex2)
    {
        inventoryData.SwapItems(itemIndex1, itemIndex2);
    }

    private void HandleItemActionRequest(int itemIndex)
    {
        InventoryItem item = inventoryData.GetItemAt(itemIndex);
        if (item.IsEmpty)
        {
            Debug.LogWarning($"Cannot handle action for empty item at index {itemIndex}");
            return;
        }

        if (item.Item.Actions != null && item.Item.Actions.Count > 0)
        {
            foreach (var action in item.Item.Actions)
            {
                inventoryUI.AddAction(action.Name, () => PerformAction(itemIndex, action));
                Debug.Log($"Added action {action.Name} for item {item.Item.Name} at index {itemIndex}");
            }
        }
        else
        {
            Debug.LogWarning($"No actions available for item {item.Item.Name} at index {itemIndex}");
        }

        if (item.Item.IsDroppable)
        {
            inventoryUI.AddAction("Drop", () => DropItem(itemIndex, item.Quantity));
        }

        inventoryUI.ShowItemAction(itemIndex);
    }

    private void PerformAction(int itemIndex, ItemAction action)
    {
        InventoryItem item = inventoryData.GetItemAt(itemIndex);
        if (item.IsEmpty)
        {
            Debug.LogWarning($"Cannot perform action: item at index {itemIndex} is empty");
            return;
        }

        if (action.Execute(gameObject, item.Parameters))
        {
            Debug.Log($"Successfully executed action {action.Name} on {item.Item.Name}");
            if (item.Item.IsConsumable)
            {
                inventoryData.RemoveItem(itemIndex, 1);
                Debug.Log($"Consumed {item.Item.Name}, removed from inventory at index {itemIndex}");
            }
            if (audioSource != null && action.SFX != null)
            {
                audioSource.PlayOneShot(action.SFX);
            }
        }
        else
        {
            Debug.LogWarning($"Failed to execute action {action.Name} on {item.Item.Name}");
        }

        if (inventoryData.GetItemAt(itemIndex).IsEmpty)
        {
            inventoryUI.ResetSelection();
        }
    }

    private void DropItem(int itemIndex, int quantity)
    {
        inventoryData.RemoveItem(itemIndex, quantity);
        inventoryUI.ResetSelection();
        if (audioSource != null && dropClip != null)
        {
            audioSource.PlayOneShot(dropClip);
        }
    }

    public void ToggleInventory()
    {
        _invOpen = !_invOpen;
        if (_invOpen)
        {
            inventoryUI.Show();
            _input.SetCursorState(false);
        }
        else
        {
            inventoryUI.Hide();
            _input.SetCursorState(true);
        }
    }

    public void AddItem(ItemData item, int quantity, Dictionary<string, object> parameters = null)
    {
        inventoryData.AddItem(item, quantity, parameters);
    }

    public void SaveInventory(string filePath)
    {
        inventoryData.SaveToFile(filePath);
    }

    public void LoadInventory(string filePath)
    {
        inventoryData.LoadFromFile(filePath);
        UpdateInventoryUI(inventoryData.GetCurrentInventoryState());
    }
}