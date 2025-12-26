using System;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType { Start, Dialogue, Random, End, InventoryEvent, Condition }
public enum InventoryAction { Add, Remove }

[Serializable]
public class DialogueOptionData {
    public string text;
    public int targetNodeId = -1;
    public int chance = 50;
}

[Serializable]
public class DialogueNodeData {
    public int id;
    public NodeType type;
    public string characterName; // NUEVO: Campo para el nombre del personaje
    public string text;
    public Vector2 position;
    public Vector2 size = new Vector2(250, 180); // Aumentado un poco el alto por defecto
    public List<DialogueOptionData> options = new List<DialogueOptionData>();

    public InventoryAction inventoryAction;
    public string itemName;      
    public string variableName;  
    public int requiredValue;    
}

[CreateAssetMenu(menuName = "Dialogue/Dialogue Graph")]
public class DialogueGraphAsset : ScriptableObject {
    public List<DialogueNodeData> nodes = new List<DialogueNodeData>();
}