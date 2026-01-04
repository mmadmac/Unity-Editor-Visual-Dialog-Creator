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
        if (graph == null || graph.nodes == null || graph.nodes.Count == 0)
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
            if (InventoryManager.Instance == null)
            {
                Debug.LogError("InventoryManager.Instance es null.");
                return;
            }

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
            if (currentNode.options == null || currentNode.options.Count < 2)
            {
                Debug.LogError("Nodo Condition mal configurado.");
                return;
            }

            if (InventoryManager.Instance == null)
            {
                Debug.LogError("InventoryManager.Instance es null.");
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
                    currentNode.loop
                );

            JumpFirst();
            return;
        }

        // ───────── NODOS VISUALES ─────────

        if (currentNode.type == NodeType.Dialogue)
        {
            if (dialogueUI == null)
            {
                Debug.LogError("DialogueUI no está asignado.");
                return;
            }

            dialogueUI.HideAllOptions();
            dialogueUI.ClearAllDialogueTargets();

            if (currentNode.lines != null)
            {
                foreach (var line in currentNode.lines)
                {
                    if (line == null) continue;

                    string processed = ProcessTags(line.text);
                    dialogueUI.ShowLine(line.targetIndex, processed);
                }
            }

            List<string> opts = new();
            if (currentNode.options != null)
            {
                foreach (var o in currentNode.options)
                    opts.Add(o.text);
            }

            dialogueUI.ShowOptions(opts);
            return;
        }

        if (currentNode.type == NodeType.End)
        {
            if (dialogueUI != null)
            {
                dialogueUI.ClearAllDialogueTargets();
                dialogueUI.HideAllOptions();

                // Si quieres un mensaje de fin, puedes usar un target fijo:
                dialogueUI.ShowLine(0, "Fin de la conversación.");
            }
        }
    }

    void JumpFirst()
    {
        if (currentNode == null) return;
        if (currentNode.options == null || currentNode.options.Count == 0) return;

        JumpToNode(currentNode.options[0].targetNodeId);
    }

    void HandleRandomNode()
    {
        if (currentNode == null || currentNode.options == null || currentNode.options.Count == 0)
            return;

        int total = 0;
        foreach (var o in currentNode.options)
            total += o.chance;

        if (total <= 0)
        {
            Debug.LogWarning("Nodo Random con total de chances <= 0.");
            return;
        }

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

        if (InventoryManager.Instance == null)
            return input;

        string output = input;
        int start = output.IndexOf('{');

        while (start != -1)
        {
            int end = output.IndexOf('}', start);
            if (end == -1) break;

            string key = output.Substring(start + 1, end - start - 1);
            string value = InventoryManager.Instance.GetAmount(key).ToString();

            output = output
                .Remove(start, end - start + 1)
                .Insert(start, value);

            start = output.IndexOf('{', start + value.Length);
        }

        return output;
    }

    void OnOptionSelected(int index)
    {
        if (currentNode == null || currentNode.options == null) return;
        if (index < 0 || index >= currentNode.options.Count) return;

        JumpToNode(currentNode.options[index].targetNodeId);
    }

    void JumpToNode(int id)
    {
        if (graph == null || graph.nodes == null) return;

        var next = graph.nodes.Find(n => n.id == id);
        if (next == null)
        {
            Debug.LogError($"No se encontró el nodo con id {id}.");
            return;
        }

        currentNode = next;
        ShowCurrentNode();
    }
}
