using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ItemData : ScriptableObject
{
    [SerializeField] private bool isStackable;
    [SerializeField] private int maxStackSize = 1;
    [SerializeField] private string itemName;
    [SerializeField, TextArea] private string description;
    [SerializeField] private Sprite image;
    [SerializeField] private GameObject prefab;
    [SerializeField] private List<ItemAction> actions;
    [SerializeField] private Dictionary<string, object> defaultParameters;
    [SerializeField] private bool isConsumable;
    [SerializeField] private bool isDroppable;

    public string ID => name;
    public bool IsStackable => isStackable;
    public int MaxStackSize => maxStackSize;
    public string Name => itemName;
    public string Description => description;
    public Sprite Image => image;
    public GameObject Prefab => prefab;
    public List<ItemAction> Actions => actions;
    public Dictionary<string, object> DefaultParameters => defaultParameters;
    public bool IsConsumable => isConsumable;
    public bool IsDroppable => isDroppable;

    private void OnValidate()
    {
        if (defaultParameters == null)
        {
            defaultParameters = new Dictionary<string, object>();
        }
    }
}

[Serializable]
public class ItemAction
{
    [SerializeField] private string name;
    [SerializeField] private AudioClip sfx;
    [SerializeField] private string actionType;
    [SerializeField] private List<ItemEffect> effects;

    public string Name => name;
    public AudioClip SFX => sfx;

    public bool Execute(GameObject target, Dictionary<string, object> parameters)
    {
        foreach (var effect in effects)
        {
            if (!effect.Apply(target, parameters))
            {
                return false;
            }
        }
        return true;
    }
}

[Serializable]
public class ItemEffect
{
    [SerializeField] private string effectType;
    [SerializeField] private float value;

    public bool Apply(GameObject target, Dictionary<string, object> parameters)
    {
        if (effectType == "Health")
        {
            var player = target.GetComponent<Player>();
            if (player != null)
            {
                player.Heal((int)value);
                return true;
            }
        }
        else if (effectType == "Strength")
        {   
            var stats = target.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.IncreaseStrength((int)value);
                return true;
            }
        }
        return false;
    }
}