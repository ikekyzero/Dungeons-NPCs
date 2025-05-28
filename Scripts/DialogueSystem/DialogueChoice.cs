using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueChoice", menuName = "Dialogue/DialogueChoice")]
public class DialogueChoice : ScriptableObject
{
    public int DialogueId;
    public int CharacterID;
    public int CharacterExpression;
    public string Line;
    public bool MakeChoice = false;
    public Choice[] Choices;
}
[System.Serializable]
public class Choice {
    public string Line;
    public int NextDialogueId;
    public bool HasBuffs = false;
    public Buff[] Buffs;
}
[System.Serializable]
public class Buff {
    public Stat stat;
    public int statBuff;
}