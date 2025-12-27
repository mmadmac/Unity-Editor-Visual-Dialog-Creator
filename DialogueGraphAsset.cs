using System;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType 
{ 
    Start, 
    Dialogue, 
    Random, 
    End, 
    InventoryEvent, 
    Condition,
    SpriteEvent // 🔹 NUEVO
}

public enum InventoryAction { Add, Remove }

[Serializable]
public class DialogueOptionData
{
    public string text;
    public int targetNodeId = -1;
    public int chance = 50;
}

[Serializable]
public class DialogueNodeData
{
    public int id;
    public NodeType type;

    public string characterName;
    public string text;

    public Vector2 position;
    public Vector2 size = new Vector2(250, 180);

    public List<DialogueOptionData> options = new List<DialogueOptionData>();

    // Inventory / Condition
    public InventoryAction inventoryAction;
    public string itemName;
    public string variableName;
    public int requiredValue;

    // 🔹 SPRITE EVENT
    public int imageIndex;
    public Sprite spriteToSet;
}

[CreateAssetMenu(menuName = "Dialogue/Dialogue Graph")]
public class DialogueGraphAsset : ScriptableObject
{
    public List<DialogueNodeData> nodes = new List<DialogueNodeData>();
}
