using UnityEngine;
using System.Collections.Generic;

public class NPC : MonoBehaviour
{
    [SerializeField] private int startDialogueId;
    [SerializeField] private ScriptableObject[] dialogueNodes;
    [SerializeField] private Transform dialogueCameraAnchor;
    private Dictionary<int, ScriptableObject> dialogueDictionary;
    private PlayerStats playerStats;
    private InventorySystem inventorySystem;
    private QuestSystem questSystem;

    private void Start()
    {
        dialogueDictionary = new Dictionary<int, ScriptableObject>();
        foreach (var node in dialogueNodes)
        {
            if (node is Dialogue dialogue)
            {
                dialogueDictionary[dialogue.DialogueId] = dialogue;
            }
            else if (node is DialogueChoice choice)
            {
                dialogueDictionary[choice.DialogueId] = choice;
            }
        }
        playerStats = FindObjectOfType<PlayerStats>();
        inventorySystem = FindObjectOfType<InventorySystem>();
        questSystem = FindObjectOfType<QuestSystem>();
    }

    public void Interact()
    {
        CameraManager cameraManager = FindObjectOfType<CameraManager>();
        cameraManager.SwitchToDialogueCamera(dialogueCameraAnchor);

        DialogueSystem dialogueSystem = FindObjectOfType<DialogueSystem>();
        dialogueSystem.StartDialogue(startDialogueId, dialogueDictionary, playerStats, inventorySystem, questSystem);
    }
}