using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private Transform ChoiceOptionsParent;
    [SerializeField] private GameObject choiceOptions;
    [SerializeField] private Button continueButton;
    [SerializeField] private Animator npcAnimator;

    private Dictionary<int, ScriptableObject> dialogueNodes;
    private ScriptableObject currentNode;
    private PlayerStats playerStats;
    private InventorySystem inventorySystem;
    private QuestSystem questSystem;
    private bool isDialogueEnded = false;
    private GameInputs gameInputs; // Added to manage input and cursor state

    public bool IsDialogueActive() => dialoguePanel.activeSelf;
    public bool IsDialogueEnded() => isDialogueEnded;
    void Start(){
        gameInputs = GetComponent<GameInputs>(); 
    }
    public void StartDialogue(int startDialogueId, Dictionary<int, ScriptableObject> nodes, PlayerStats stats, InventorySystem invSystem, QuestSystem quests)
    {
        dialogueNodes = nodes;
        if (dialogueNodes.TryGetValue(startDialogueId, out ScriptableObject startNode))
        {
            currentNode = startNode;
            playerStats = stats;
            inventorySystem = invSystem;
            questSystem = quests;
            dialoguePanel.SetActive(true);
            isDialogueEnded = false;
            if (gameInputs != null)
            {
                gameInputs.dialogueOpen = true; 
                gameInputs.SetCursorState(false); 
            }
            else
            {
                Debug.LogWarning("GameInputs не найден на игроке!");
            }
            DisplayCurrentNode();
        }
        else
        {
            Debug.LogError($"Dialogue node with ID {startDialogueId} not found.");
        }
    }

    private void DisplayCurrentNode()
    {
        foreach (Transform child in ChoiceOptionsParent)
        {
            Destroy(child.gameObject);
        }

        if (currentNode is Dialogue dialogue)
        {
            speakerNameText.text = dialogue.CharacterName;
            dialogueText.text = dialogue.Line;
            ChoiceOptionsParent.gameObject.SetActive(false);

            if (!dialogue.IsEndOfDialogue && dialogue.NextDialogueId != 0)
            {
                continueButton.gameObject.SetActive(true);
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(() =>
                {
                    if (dialogueNodes.TryGetValue(dialogue.NextDialogueId, out ScriptableObject nextNode))
                    {
                        currentNode = nextNode;
                        DisplayCurrentNode();
                    }
                    else
                    {
                        Debug.LogError($"Dialogue node with ID {dialogue.NextDialogueId} not found.");
                        EndDialogue();
                    }
                });
            }
            else
            {
                continueButton.gameObject.SetActive(false);
                if (dialogue.IsEndOfDialogue)
                {
                    EndDialogue();
                }
            }
        }
        else if (currentNode is DialogueChoice choice)
        {
            speakerNameText.text = choice.CharacterName;
            dialogueText.text = choice.Line;
            continueButton.gameObject.SetActive(false);
            ChoiceOptionsParent.gameObject.SetActive(true);

            for (int i = 0; i < choice.Choices.Length; i++)
            {
                GameObject buttonObj = Instantiate(choiceOptions, ChoiceOptionsParent);
                Button button = buttonObj.GetComponent<Button>();
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                buttonText.text = choice.Choices[i].Line;
                int index = i;
                button.onClick.AddListener(() => HandleChoice(choice.Choices[index]));
            }
        }
    }

    private void HandleChoice(Choice choice)
    {
        foreach (Transform child in ChoiceOptionsParent)
        {
            Destroy(child.gameObject);
        }
        ChoiceOptionsParent.gameObject.SetActive(false);

        if (choice.HasBuffs)
        {
            foreach (var buff in choice.Buffs)
            {
                ApplyBuff(buff);
            }
        }
        if (choice.OpenTrade)
        {
            OpenTradeWindow();
        }
        if (choice.GiveQuests)
        {
            GiveQuests();
        }
        if (!choice.IsEndOfDialogue && choice.NextDialogueId != 0)
        {
            if (dialogueNodes.TryGetValue(choice.NextDialogueId, out ScriptableObject nextNode))
            {
                currentNode = nextNode;
                DisplayCurrentNode();
            }
            else
            {
                Debug.LogError($"Dialogue node with ID {choice.NextDialogueId} not found.");
                EndDialogue();
            }
        }
        else
        {
            EndDialogue();
        }
    }

    private void ApplyBuff(Buff buff)
    {
        switch (buff.stat)
        {
            case Stat.Charisma:
                playerStats.IncreaseCharisma(buff.statBuff);
                break;
        }
    }

    private void OpenTradeWindow()
    {
        inventorySystem.ToggleInventory();
    }

    private void GiveQuests()
    {
        questSystem.ActivateQuestsForNPC();
    }

    private void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        continueButton.gameObject.SetActive(false);
        ChoiceOptionsParent.gameObject.SetActive(false);
        foreach (Transform child in ChoiceOptionsParent)
        {
            Destroy(child.gameObject);
        }
        isDialogueEnded = true;
        gameInputs.dialogueOpen = false;
        gameInputs.SetCursorState(true);

        CameraManager cameraManager = FindObjectOfType<CameraManager>();
        cameraManager.SwitchToPlayerCamera();
    }
}