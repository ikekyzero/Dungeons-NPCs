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
            inventoryUI.CreateDraggedItem(item.Item.Image, item.Quantity);
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

        // Очищаем старые кнопки
        inventoryUI.ClearActionPanel();

        // Добавляем новые действия
        if (item.Item.Actions != null && item.Item.Actions.Count > 0)
        {
            foreach (var action in item.Item.Actions)
            {
                inventoryUI.AddAction(action.Name, () => PerformAction(itemIndex, action));
                Debug.Log($"Added action {action.Name} for item {item.Item.Name} at index {itemIndex}");
            }
        }

        // Добавляем действие "Drop", если предмет можно выбросить
        if (item.Item.IsDroppable)
        {
            inventoryUI.AddAction("Выбросить", () => DropItem(itemIndex, item.Quantity));
        }

        // Показываем панель с новыми кнопками
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

        // Закрываем панель действий после выполнения действия
        inventoryUI.HideItemActionPanel();
    }

    private void DropItem(int itemIndex, int quantity)
    {
        InventoryItem item = inventoryData.GetItemAt(itemIndex);
        if (item.IsEmpty) return;

        // Создаём предмет в мире
        if (item.Item.Prefab != null)
        {
            Vector3 dropPosition = transform.position + transform.forward * 1.5f; // Смещение вперёд от игрока
            GameObject droppedItem = Instantiate(item.Item.Prefab, dropPosition, Quaternion.identity);
            Item itemComponent = droppedItem.GetComponent<Item>();
            if (itemComponent != null)
            {
                itemComponent.InventoryItem = item.Item;
                itemComponent.Quantity = quantity;
            }
        }

        inventoryData.RemoveItem(itemIndex, quantity);
        inventoryUI.ResetSelection();
        if (audioSource != null && dropClip != null)
        {
            audioSource.PlayOneShot(dropClip);
        }
        // Закрываем панель действий после удаления предмета
        inventoryUI.HideItemActionPanel();
    }

    public void ToggleInventory()
    {
        _invOpen = !_invOpen;
        if (_invOpen)
        {
            inventoryUI.Show();
            _input.SetCursorState(false);
            Time.timeScale = 0f; // Останавливаем время в игре
        }
        else
        {
            inventoryUI.Hide();
            if (!_input.dialogueOpen)
            {
                _input.SetCursorState(true);
            }
            Time.timeScale = 1f; // Восстанавливаем время в игре
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