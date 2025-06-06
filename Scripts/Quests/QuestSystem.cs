using System.Collections.Generic;
using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    private List<Quest> activeQuests = new List<Quest>();

    public void ActivateQuestsForNPC()
    {
        Debug.Log("Квесты активированы для игрока");
        // Пример: activeQuests.Add(new Quest("Название квеста", "Описание"));
    }

    public void CompleteQuest(Quest quest)
    {
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            Debug.Log("Квест завершен: " + quest.Name);
        }
    }
}

[System.Serializable]
public class Quest
{
    public string Name;
    public string Description;
    public bool IsCompleted;

    public Quest(string name, string description)
    {
        Name = name;
        Description = description;
        IsCompleted = false;
    }
}