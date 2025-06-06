using UnityEngine;

[CreateAssetMenu(fileName = "DialogueChoice", menuName = "Dialogue/DialogueChoice")]
public class DialogueChoice : ScriptableObject
{
    [Header("Dialogue Settings")]
    [Tooltip("Уникальный идентификатор этого узла диалога")]
    public int DialogueId;

    [Tooltip("Имя персонажа, который говорит")]
    public string CharacterName;

    [Tooltip("Строка диалога")]
    public string Line;

    [Space(10)]
    [Header("Choice Options")]
    [Tooltip("Массив вариантов выбора для игрока")]
    public Choice[] Choices;
}

[System.Serializable]
public class Choice
{
    [Header("Choice Details")]
    [Tooltip("Текст этого выбора")]
    public string Line;

    [Tooltip("ID следующего узла диалога")]
    public int NextDialogueId;

    [Tooltip("Заканчивает ли этот выбор диалог")]
    public bool IsEndOfDialogue;

    [Space(10)]
    [Header("Effects")]
    [Tooltip("Применяет ли этот выбор баффы")]
    public bool HasBuffs;

    [Tooltip("Массив баффов для применения, используется только если HasBuffs = true")]
    public Buff[] Buffs;

    [Tooltip("Открывает ли окно торговли")]
    public bool OpenTrade;

    [Tooltip("Дает ли квесты")]
    public bool GiveQuests;
}

[System.Serializable]
public class Buff
{
    [Tooltip("Характеристика для баффа")]
    public Stat stat;

    [Tooltip("Величина баффа характеристики")]
    public int statBuff;
}

public enum Stat
{
    Strength,
    Agility,
    Intelligence,
    Charisma
}