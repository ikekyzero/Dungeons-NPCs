using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

[CreateAssetMenu]
public class InventoryData : ScriptableObject
{
    [SerializeField] private int size = 10;
    [SerializeField] private List<InventoryItem> items;
    private Dictionary<int, InventoryItem> cachedState;

    public int Size => size;
    public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;

    public void Initialize()
    {
        items = new List<InventoryItem>(new InventoryItem[size]);
        for (int i = 0; i < size; i++)
        {
            items[i] = InventoryItem.Empty;
        }
        cachedState = new Dictionary<int, InventoryItem>();
        UpdateCachedState();
    }

    public void AddItem(ItemData item, int quantity, Dictionary<string, object> parameters = null)
    {
        if (item == null) return;

        if (!item.IsStackable)
        {
            while (quantity > 0 && !IsFull())
            {
                AddToFirstFreeSlot(item, 1, parameters);
                quantity--;
            }
        }
        else
        {
            AddStackableItem(item, quantity, parameters);
        }
        UpdateCachedState();
        NotifyUpdate();
    }

    private void AddStackableItem(ItemData item, int quantity, Dictionary<string, object> parameters)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (!items[i].IsEmpty && items[i].Item.ID == item.ID)
            {
                int remainingCapacity = item.MaxStackSize - items[i].Quantity;
                if (remainingCapacity > 0)
                {
                    int toAdd = Mathf.Min(quantity, remainingCapacity);
                    items[i] = new InventoryItem(item, items[i].Quantity + toAdd, items[i].Parameters);
                    quantity -= toAdd;
                }
            }
        }
        while (quantity > 0 && !IsFull())
        {
            int toAdd = Mathf.Min(quantity, item.MaxStackSize);
            AddToFirstFreeSlot(item, toAdd, parameters);
            quantity -= toAdd;
        }
    }

    private void AddToFirstFreeSlot(ItemData item, int quantity, Dictionary<string, object> parameters)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].IsEmpty)
            {
                items[i] = new InventoryItem(item, quantity, parameters ?? item.DefaultParameters);
                return;
            }
        }
    }

    public void RemoveItem(int index, int quantity)
    {
        if (index < 0 || index >= items.Count || items[index].IsEmpty) return;

        int newQuantity = items[index].Quantity - quantity;
        items[index] = newQuantity <= 0 ? InventoryItem.Empty : new InventoryItem(items[index].Item, newQuantity, items[index].Parameters);
        UpdateCachedState();
        NotifyUpdate();
    }

    public void SwapItems(int index1, int index2)
    {
        if (index1 < 0 || index1 >= items.Count || index2 < 0 || index2 >= items.Count) return;

        var temp = items[index1];
        items[index1] = items[index2];
        items[index2] = temp;
        UpdateCachedState();
        NotifyUpdate();
    }

    public InventoryItem GetItemAt(int index)
    {
        return index >= 0 && index < items.Count ? items[index] : InventoryItem.Empty;
    }

    public Dictionary<int, InventoryItem> GetCurrentInventoryState()
    {
        return cachedState;
    }

    private bool IsFull()
    {
        return items.All(item => !item.IsEmpty);
    }

    private void UpdateCachedState()
    {
        cachedState.Clear();
        for (int i = 0; i < items.Count; i++)
        {
            if (!items[i].IsEmpty)
            {
                cachedState[i] = items[i];
            }
        }
    }

    private void NotifyUpdate()
    {
        OnInventoryUpdated?.Invoke(cachedState);
    }

    public void SaveToFile(string filePath)
    {
        var saveData = new InventorySaveData
        {
            Items = items.Select((item, index) => new InventoryItemSaveData
            {
                Index = index,
                ItemID = item.IsEmpty ? null : item.Item.ID,
                Quantity = item.Quantity,
                Parameters = item.Parameters
            }).ToList()
        };
        File.WriteAllText(filePath, JsonUtility.ToJson(saveData));
    }

    public void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return;

        var saveData = JsonUtility.FromJson<InventorySaveData>(File.ReadAllText(filePath));
        Initialize();
        foreach (var itemData in saveData.Items)
        {
            if (itemData.ItemID != null)
            {
                ItemData item = Resources.Load<ItemData>(itemData.ItemID);
                if (item != null)
                {
                    items[itemData.Index] = new InventoryItem(item, itemData.Quantity, itemData.Parameters);
                }
            }
        }
        UpdateCachedState();
    }
}

[Serializable]
public struct InventoryItem
{
    public ItemData Item;
    public int Quantity;
    public Dictionary<string, object> Parameters;
    public bool IsEmpty => Item == null;

    public InventoryItem(ItemData item, int quantity, Dictionary<string, object> parameters)
    {
        Item = item;
        Quantity = quantity;
        Parameters = new Dictionary<string, object>(parameters ?? new Dictionary<string, object>());
    }

    public static InventoryItem Empty => new InventoryItem(null, 0, new Dictionary<string, object>());
}

[Serializable]
public class InventorySaveData
{
    public List<InventoryItemSaveData> Items;
}

[Serializable]
public class InventoryItemSaveData
{
    public int Index;
    public string ItemID;
    public int Quantity;
    public Dictionary<string, object> Parameters;
}