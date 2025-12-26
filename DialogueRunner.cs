using UnityEngine;
using System.Collections.Generic;

public class DialogueRunner : MonoBehaviour
{
    [Header("Configuración")]
    public DialogueUI dialogueUI; 
    public DialogueGraphAsset graph; 
    
    private DialogueNodeData currentNode;

    void Start() {
        if (graph == null || graph.nodes.Count == 0) {
            Debug.LogError("No hay grafo asignado o el grafo está vacío.");
            return;
        }

        // 1. Suscribirse a los eventos de la UI
        if (dialogueUI != null) dialogueUI.OnOptionSelected += OnOptionSelected;

        // 2. Buscar el nodo de entrada (START)
        currentNode = graph.nodes.Find(n => n.type == NodeType.Start);
        
        if (currentNode != null) {
            ShowCurrentNode();
        } else {
            Debug.LogError("No se encontró un nodo de tipo START en el grafo.");
        }
    }

    void ShowCurrentNode() {
        if (currentNode == null) return;

        // --- LÓGICA DE NODOS AUTOMÁTICOS (SIN INTERACCIÓN) ---

        if (currentNode.type == NodeType.Start) {
            // Salta automáticamente a la primera conexión
            if (currentNode.options.Count > 0 && currentNode.options[0].targetNodeId != -1) {
                JumpToNode(currentNode.options[0].targetNodeId);
            }
            return;
        }

        if (currentNode.type == NodeType.InventoryEvent) {
            // Ejecutar acción de inventario
            if (currentNode.inventoryAction == InventoryAction.Add)
                InventoryManager.Instance.AddItem(currentNode.itemName, currentNode.requiredValue);
            else
                InventoryManager.Instance.RemoveItem(currentNode.itemName, currentNode.requiredValue);

            // Saltar al siguiente nodo
            if (currentNode.options.Count > 0 && currentNode.options[0].targetNodeId != -1) {
                JumpToNode(currentNode.options[0].targetNodeId);
            }
            return;
        }

        if (currentNode.type == NodeType.Condition) {
            // Comprobar si tenemos el item/cantidad necesaria
            bool success = InventoryManager.Instance.HasEnough(currentNode.variableName, currentNode.requiredValue);
            int nextId = success ? currentNode.options[0].targetNodeId : currentNode.options[1].targetNodeId;
            
            if (nextId != -1) JumpToNode(nextId);
            return;
        }

        if (currentNode.type == NodeType.Random) {
            HandleRandomNode();
            return;
        }

        // --- LÓGICA DE NODOS VISUALES (CON INTERACCIÓN) ---

        if (currentNode.type == NodeType.Dialogue) {
            dialogueUI.HideAllOptions();
            
            // Procesar etiquetas estilo {item} en el texto
            string processedText = ProcessTags(currentNode.text);
            
            // ENVIAR NOMBRE Y TEXTO A LA UI
            // Nota: Asegúrate de que DialogueUI.ShowDialogue acepte (string, string)
            dialogueUI.ShowDialogue(currentNode.characterName, processedText);

            // Preparar y mostrar opciones
            List<string> optTexts = new List<string>();
            foreach (var o in currentNode.options) optTexts.Add(o.text);
            dialogueUI.ShowOptions(optTexts);
        }
        
        if (currentNode.type == NodeType.End) {
            dialogueUI.ShowDialogue("", "Fin de la conversación.");
            dialogueUI.HideAllOptions();
        }
    }

    void HandleRandomNode() {
        if (currentNode.options == null || currentNode.options.Count == 0) return;

        int totalWeight = 0;
        foreach (var opt in currentNode.options) totalWeight += opt.chance;

        int randomValue = Random.Range(0, totalWeight);
        int cursor = 0;

        foreach (var opt in currentNode.options) {
            cursor += opt.chance;
            if (randomValue < cursor) {
                if (opt.targetNodeId != -1) JumpToNode(opt.targetNodeId);
                break;
            }
        }
    }

    string ProcessTags(string input) {
        if (string.IsNullOrEmpty(input)) return "";
        string output = input;
        int start = output.IndexOf('{');
        while (start != -1) {
            int end = output.IndexOf('}', start);
            if (end != -1) {
                string itemName = output.Substring(start + 1, end - start - 1);
                string amount = InventoryManager.Instance.GetAmount(itemName).ToString();
                output = output.Remove(start, end - start + 1).Insert(start, amount);
                start = output.IndexOf('{', start + amount.Length);
            } else break;
        }
        return output;
    }

    void OnOptionSelected(int index) {
        if (currentNode != null && index >= 0 && index < currentNode.options.Count) {
            int nextId = currentNode.options[index].targetNodeId;
            if (nextId != -1) JumpToNode(nextId);
        }
    }

    void JumpToNode(int id) {
        currentNode = graph.nodes.Find(n => n.id == id);
        ShowCurrentNode();
    }
}