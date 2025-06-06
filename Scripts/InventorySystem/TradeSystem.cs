using System.Collections.Generic;
using UnityEngine;

public class TradeSystem : MonoBehaviour
{
    [SerializeField] private InventoryData playerInventory;
    [SerializeField] private InventoryData npcInventory;

    public void OpenTradeWindow()
    {
        Debug.Log("Окно торговли открыто");
    }

    public void BuyItem(ItemData item, int quantity)
    {
        playerInventory.AddItem(item, quantity);
        Debug.Log($"Куплено {quantity} x {item.Name}");
    }

    public void SellItem(ItemData item, int quantity)
    {
        playerInventory.RemoveItem(1, quantity); //исправить потом
        Debug.Log($"Продано {quantity} x {item.Name}");
    }
}