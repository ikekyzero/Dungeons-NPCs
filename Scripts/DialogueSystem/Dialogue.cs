using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/DialogueStandard")]
public class Dialogue : ScriptableObject
{
    public int DialogueId;
    public string CharacterName;
    public string Line;
    public int NextDialogueId;
    public bool IsEndOfDialogue; // Поле для указания завершения диалога
}