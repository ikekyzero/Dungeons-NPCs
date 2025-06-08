using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Настройки взаимодействия")]
    [SerializeField] private TextMeshProUGUI interactionText; // UI-текст для названия предмета или NPC
    [SerializeField] private float rayDistance = 3f; // Дистанция raycast
    [SerializeField] private LayerMask interactionLayer; // Слой для предметов и NPC

    private GameInputs gameInputs;
    private InventorySystem inventorySystem;
    private Camera mainCamera;
    private Transform playerTransform; // Для игнорирования игрока
    private bool wasPickupPressed = false; // Для отслеживания нажатия кнопки

    private void Start()
    {
        gameInputs = GetComponent<GameInputs>();
        inventorySystem = GetComponent<InventorySystem>();
        mainCamera = Camera.main;
        playerTransform = transform; // Предполагаем, что скрипт на игроке
        if (interactionText != null)
        {
            interactionText.enabled = false;
        }
    }

    private void Update()
    {
        if (gameInputs.invOpen || gameInputs.dialogueOpen)
        {
            if (interactionText != null) interactionText.enabled = false;
            return;
        }
        HandleInteractionHighlight();

        if (gameInputs.pickup && !wasPickupPressed)
        {
            HandleInteraction();
        }
        wasPickupPressed = gameInputs.pickup;
    }

    private void HandleInteractionHighlight()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red); // Визуализация луча в сцене
        
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactionLayer))
        {
            if (hit.collider.CompareTag("Item"))
            {
                Item item = hit.collider.GetComponent<Item>();
                if (item != null && item.InventoryItem != null)
                {
                    interactionText.text = item.InventoryItem.Name;
                    interactionText.enabled = true;
                    return;
                }
            }
            else if (hit.collider.CompareTag("NPC"))
            {
                NPC npc = hit.collider.GetComponent<NPC>();
                if (npc != null)
                {
                    interactionText.text = "Нажмите E для разговора";
                    interactionText.enabled = true;
                    return;
                }
            }
        }

        interactionText.enabled = false;
    }

    private void HandleInteraction()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        Debug.DrawRay(ray.origin, ray.direction * rayDistance, Color.red); // Визуализация луча в сцене
        
        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, interactionLayer))
        {
            if (hit.collider.CompareTag("Item"))
            {
                Item item = hit.collider.GetComponent<Item>();
                if (item != null && item.InventoryItem != null)
                {
                    inventorySystem.AddItem(item.InventoryItem, item.Quantity);
                    item.DestroyItem(); // Уничтожаем объект в сцене
                }
            }
            else if (hit.collider.CompareTag("NPC"))
            {
                NPC npc = hit.collider.GetComponent<NPC>();
                if (npc != null)
                {
                    npc.Interact();
                }
            }
        }
        gameInputs.pickup = false;
    }
}