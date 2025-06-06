using UnityEngine;
using System;

[System.Serializable]
public class ExperienceSystem
{
    public int Level { get; private set; } = 1;
    public int Experience { get; private set; }
    private int experienceToNextLevel = 100;

    public event Action<int> OnLevelUp;

    public void GainExperience(int amount)
    {
        Experience += amount;
        while (Experience >= experienceToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;
        Experience -= experienceToNextLevel;
        experienceToNextLevel = Mathf.RoundToInt(experienceToNextLevel * 1.5f); // Увеличиваем требуемый опыт
        OnLevelUp?.Invoke(Level);
    }
}