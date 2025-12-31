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
    SpriteEvent, // 🖼️
    AudioEvent   // 🔊
}

public enum InventoryAction
{
    Add,
    Remove
}

public enum AudioAction
{
    Play,
    Stop,
    PlayOneShot
}

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

    // ───────── DIALOGUE ─────────
    public string characterName;
    public string text;

    // ───────── EDITOR DATA ─────────
    public Vector2 position;
    public Vector2 size = new Vector2(250, 180);

    // ───────── CONNECTIONS ─────────
    public List<DialogueOptionData> options = new List<DialogueOptionData>();

    // ───────── INVENTORY / CONDITION ─────────
    public InventoryAction inventoryAction;
    public string itemName;
    public string variableName;
    public int requiredValue;

    // ───────── SPRITE EVENT ─────────
    public int imageIndex;
    public Sprite spriteToSet;

    
    // ───────── AUDIO EVENT ─────────
    public int audioSourceIndex;
    public AudioClip audioClip;
    public AudioAction audioAction;
    public bool loop; // ← nueva propiedad
}

[CreateAssetMenu(menuName = "Dialogue/Dialogue Graph")]
public class DialogueGraphAsset : ScriptableObject
{
    public List<DialogueNodeData> nodes = new List<DialogueNodeData>();
}
