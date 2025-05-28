using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewQuest", menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questName;
    public string description;
    public bool isCompleted;
    public List<Stat> requiredStats;

    public void Complete()
    {
        isCompleted = true;
        // Логика награды
    }
}
