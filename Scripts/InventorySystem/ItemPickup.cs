using UnityEngine;
using TMPro;

public class ItemPickup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI pickupText; // UI-текст для названия предмета
    [SerializeField] private float rayDistance = 3f; // Дистанция raycast
    [SerializeField] private LayerMask itemLayer; // Слой для предметов

    private GameInputs gameInputs;
    private InventorySystem inventorySystem;
    private Camera mainCamera;
    private Transform playerTransform; // Для игнорирования игрока

    private void Awake()
    {
        gameInputs = GetComponent<GameInputs>();
        inventorySystem = GetComponent<InventorySystem>();
        mainCamera = Camera.main;
        playerTransform = transform; // Предполагаем, что скрипт на игроке
    }

    private void Start()
    {
        if (pickupText != null)
        {
            pickupText.enabled = false;
        }
    }

    private void Update()
    {
        if (gameInputs.invOpen) // Не обрабатываем, если инвентарь открыт
        {
            if (pickupText != null) pickupText.enabled = false;
            return;
        }

        HandleItemHighlight();
        HandleItemPickup();
    }

    private void HandleItemHighlight()
    {
        if (pickupText == null) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, itemLayer))
        {
            if (hit.collider.CompareTag("Item"))
            {
                Item item = hit.collider.GetComponent<Item>();
                if (item != null && item.InventoryItem != null)
                {
                    pickupText.text = item.InventoryItem.Name;
                    pickupText.enabled = true;
                    return;
                }
            }
        }

        pickupText.enabled = false;
    }

    private void HandleItemPickup()
    {
        if (!gameInputs.pickup) return;

        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, itemLayer))
        {
            if (hit.collider.CompareTag("Item"))
            {
                Item item = hit.collider.GetComponent<Item>();
                if (item != null && item.InventoryItem != null)
                {
                    inventorySystem.AddItem(item.InventoryItem, item.Quantity);
                    item.DestroyItem(); // Уничтожаем объект в сцене
                    gameInputs.pickup = false; // Сбрасываем ввод
                }
            }
        }
        else
        {
            gameInputs.pickup = false;
        }
    }
}