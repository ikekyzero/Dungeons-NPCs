using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "Dialogue/DialogueStandart")]
public class Dialogue : ScriptableObject
{
    public int DialogueId;
    public int CharacterID;
    public int CharacterExpression;
    public string Line;
    public int NextDialogueId;
}