using UnityEngine;
using System.Collections.Generic;

public class DialogueRunner : MonoBehaviour
{
    [Header("Configuración")]
    public DialogueUI dialogueUI;
    public DialogueGraphAsset graph;

    private DialogueNodeData currentNode;

    void Start()
    {
        if (graph == null || graph.nodes.Count == 0)
        {
            Debug.LogError("No hay grafo asignado o está vacío.");
            return;
        }

        if (dialogueUI != null)
            dialogueUI.OnOptionSelected += OnOptionSelected;

        currentNode = graph.nodes.Find(n => n.type == NodeType.Start);

        if (currentNode != null)
            ShowCurrentNode();
        else
            Debug.LogError("No existe un nodo START.");
    }

    void ShowCurrentNode()
    {
        if (currentNode == null) return;

        // ───────── NODOS AUTOMÁTICOS ─────────

        if (currentNode.type == NodeType.Start)
        {
            JumpFirst();
            return;
        }

        if (currentNode.type == NodeType.InventoryEvent)
        {
            if (currentNode.inventoryAction == InventoryAction.Add)
                InventoryManager.Instance.AddItem(
                    currentNode.itemName,
                    currentNode.requiredValue
                );
            else
                InventoryManager.Instance.RemoveItem(
                    currentNode.itemName,
                    currentNode.requiredValue
                );

            JumpFirst();
            return;
        }

        if (currentNode.type == NodeType.Condition)
        {
            if (currentNode.options.Count < 2)
            {
                Debug.LogError("Nodo Condition mal configurado.");
                return;
            }

            bool ok = InventoryManager.Instance.HasEnough(
                currentNode.variableName,
                currentNode.requiredValue
            );

            int next = ok
                ? currentNode.options[0].targetNodeId
                : currentNode.options[1].targetNodeId;

            JumpToNode(next);
            return;
        }

        if (currentNode.type == NodeType.Random)
        {
            HandleRandomNode();
            return;
        }

        // 🖼️ SPRITE EVENT (NO bloquea)
        if (currentNode.type == NodeType.SpriteEvent)
        {
            if (dialogueUI != null)
                dialogueUI.SetSprite(
                    currentNode.imageIndex,
                    currentNode.spriteToSet
                );

            JumpFirst();
            return;
        }

        // 🔊 AUDIO EVENT (NO bloquea)
        if (currentNode.type == NodeType.AudioEvent)
        {
            if (dialogueUI != null)
                dialogueUI.HandleAudio(
    currentNode.audioSourceIndex,
    currentNode.audioClip,
    currentNode.audioAction,
    currentNode.loop // ← pasamos la propiedad loop
);

            JumpFirst();
            return;
        }

        // ───────── NODOS VISUALES ─────────

        if (currentNode.type == NodeType.Dialogue)
        {
            dialogueUI.HideAllOptions();

            string processedText = ProcessTags(currentNode.text);
            dialogueUI.ShowDialogue(
                currentNode.characterName,
                processedText
            );

            List<string> opts = new();
            foreach (var o in currentNode.options)
                opts.Add(o.text);

            dialogueUI.ShowOptions(opts);
        }

        if (currentNode.type == NodeType.End)
        {
            dialogueUI.ShowDialogue("", "Fin de la conversación.");
            dialogueUI.HideAllOptions();
        }
    }

    void JumpFirst()
    {
        if (currentNode.options.Count > 0)
            JumpToNode(currentNode.options[0].targetNodeId);
    }

    void HandleRandomNode()
    {
        int total = 0;
        foreach (var o in currentNode.options)
            total += o.chance;

        int r = Random.Range(0, total);
        int acc = 0;

        foreach (var o in currentNode.options)
        {
            acc += o.chance;
            if (r < acc)
            {
                JumpToNode(o.targetNodeId);
                break;
            }
        }
    }

    string ProcessTags(string input)
    {
        if (string.IsNullOrEmpty(input)) return "";

        string output = input;
        int start = output.IndexOf('{');

        while (start != -1)
        {
            int end = output.IndexOf('}', start);
            if (end == -1) break;

            string key =
                output.Substring(start + 1, end - start - 1);

            string value =
                InventoryManager.Instance
                    .GetAmount(key)
                    .ToString();

            output = output
                .Remove(start, end - start + 1)
                .Insert(start, value);

            start = output.IndexOf('{', start + value.Length);
        }

        return output;
    }

    void OnOptionSelected(int index)
    {
        if (index < 0 || index >= currentNode.options.Count)
            return;

        JumpToNode(currentNode.options[index].targetNodeId);
    }

    void JumpToNode(int id)
    {
        currentNode = graph.nodes.Find(n => n.id == id);
        ShowCurrentNode();
    }
}
