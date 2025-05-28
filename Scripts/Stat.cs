using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPlayerStat", menuName = "Player/Stat")]
public class Stat : ScriptableObject
{
    public string statName; // Название характеристики (логика, красноречие и т.д.)
    public int value; // Значение характеристики
    public int level; // Уровень характеристики

    public void Increase(int amount)
    {
        value += amount; // Увеличение характеристики
    }
}