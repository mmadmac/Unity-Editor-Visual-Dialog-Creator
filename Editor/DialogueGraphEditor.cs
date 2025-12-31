using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DialogueGraphEditor : EditorWindow
{
    private List<DialogueNodeData> nodes = new();
    private int nextId = 0;
    private int fromNodeId = -1;
    private int fromOptIdx = -1;

    private Vector2 panOffset = Vector2.zero;
    private float zoomLevel = 1.0f;
    private const float minZoom = 0.2f;
    private const float maxZoom = 2.0f;

    private DialogueNodeData draggingNode = null;
    private DialogueGraphAsset loadedAsset;

    [MenuItem("Tools/Dialogue Editor")]
    public static void Open() =>
        GetWindow<DialogueGraphEditor>("Dialogue Editor");

    void OnGUI()
    {
        HandleEvents();

        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height),
            new Color(0.18f, 0.18f, 0.18f));

        DrawGrid(20, 0.05f);
        DrawGrid(100, 0.1f);

        DrawConnections();

        foreach (var node in nodes)
            DrawNode(node);

        DrawPreviewLine();
        DrawTopToolbar();

        if (GUI.changed)
            Repaint();
    }

    // ─────────────────────────────────────────────
    // DIBUJO DE NODOS
    // ─────────────────────────────────────────────

    private void DrawNode(DialogueNodeData node)
    {
        Rect screenRect = ScaleRect(new Rect(node.position, node.size));
        GUI.Box(screenRect, "", (GUIStyle)"window");

        Rect titleRect = new(screenRect.x, screenRect.y, screenRect.width, 20 * zoomLevel);
        GUI.Label(titleRect, node.type.ToString(), EditorStyles.boldLabel);

        if (zoomLevel > 0.4f)
        {
            Rect contentRect = new(screenRect.x + 5, screenRect.y + 20 * zoomLevel,
                screenRect.width - 10, screenRect.height - 25 * zoomLevel);

            GUILayout.BeginArea(contentRect);
            DrawNodeInside(node);
            GUILayout.EndArea();
        }
    }

    private void DrawNodeInside(DialogueNodeData node)
    {
        switch (node.type)
        {
            case NodeType.Dialogue:
                node.characterName = EditorGUILayout.TextField("Personaje", node.characterName);
                node.text = EditorGUILayout.TextArea(node.text, GUILayout.MinHeight(40));
                DrawNodeOptions(node, true);
                break;

            case NodeType.Condition:
                node.variableName = EditorGUILayout.TextField("Variable", node.variableName);
                node.requiredValue = EditorGUILayout.IntField("Cantidad", node.requiredValue);

                if (GUILayout.Button("TRUE →"))
                {
                    fromNodeId = node.id;
                    fromOptIdx = 0;
                }
                if (GUILayout.Button("FALSE →"))
                {
                    fromNodeId = node.id;
                    fromOptIdx = 1;
                }
                break;

            case NodeType.InventoryEvent:
                node.inventoryAction = (InventoryAction)EditorGUILayout.EnumPopup(node.inventoryAction);
                node.itemName = EditorGUILayout.TextField("Item", node.itemName);
                node.requiredValue = EditorGUILayout.IntField("Cantidad", node.requiredValue);

                if (GUILayout.Button("SIGUIENTE →"))
                {
                    fromNodeId = node.id;
                    fromOptIdx = 0;
                }
                break;

            case NodeType.SpriteEvent:
                node.imageIndex = EditorGUILayout.IntField("Image Index", node.imageIndex);
                node.spriteToSet = (Sprite)EditorGUILayout.ObjectField("Sprite", node.spriteToSet, typeof(Sprite), false);

                if (GUILayout.Button("SIGUIENTE →"))
                {
                    fromNodeId = node.id;
                    fromOptIdx = 0;
                }
                break;

            case NodeType.AudioEvent:
                node.audioSourceIndex = EditorGUILayout.IntField("Source Index", node.audioSourceIndex);
                node.audioAction = (AudioAction)EditorGUILayout.EnumPopup("Action", node.audioAction);
                node.audioClip = (AudioClip)EditorGUILayout.ObjectField("Clip", node.audioClip, typeof(AudioClip), false);

                if (node.audioAction == AudioAction.Play)
                    node.loop = EditorGUILayout.Toggle("Loop", node.loop);

                if (GUILayout.Button("SIGUIENTE →"))
                {
                    fromNodeId = node.id;
                    fromOptIdx = 0;
                }
                break;

            case NodeType.Start:
                if (GUILayout.Button("EMPEZAR →"))
                {
                    fromNodeId = node.id;
                    fromOptIdx = 0;
                }
                break;

            case NodeType.End:
                GUILayout.Label("Fin de la conversación");
                break;

            case NodeType.Random:
                DrawNodeOptions(node, false);
                break;
        }
    }

    private void DrawNodeOptions(DialogueNodeData node, bool isDialogue)
    {
        for (int i = 0; i < node.options.Count; i++)
        {
            GUILayout.BeginHorizontal();

            if (isDialogue)
                node.options[i].text = EditorGUILayout.TextField(node.options[i].text);
            else
                node.options[i].chance = EditorGUILayout.IntField(node.options[i].chance, GUILayout.Width(40));

            if (GUILayout.Button("→", GUILayout.Width(25)))
            {
                fromNodeId = node.id;
                fromOptIdx = i;
            }

            if (GUILayout.Button("x", GUILayout.Width(20)))
            {
                node.options.RemoveAt(i);
                break;
            }

            GUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Opción"))
            node.options.Add(new DialogueOptionData());
    }

    // ─────────────────────────────────────────────
    // TOOLBAR
    // ─────────────────────────────────────────────

    private void DrawTopToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("+ DIALOG", EditorStyles.toolbarButton))
            AddNode(NodeType.Dialogue);
        if (GUILayout.Button("+ RANDOM", EditorStyles.toolbarButton))
            AddNode(NodeType.Random);
        if (GUILayout.Button("+ IF", EditorStyles.toolbarButton))
            AddNode(NodeType.Condition);
        if (GUILayout.Button("+ ITEM", EditorStyles.toolbarButton))
            AddNode(NodeType.InventoryEvent);
        if (GUILayout.Button("+ SPRITE", EditorStyles.toolbarButton))
            AddNode(NodeType.SpriteEvent);
        if (GUILayout.Button("+ AUDIO", EditorStyles.toolbarButton))
            AddNode(NodeType.AudioEvent);
        if (GUILayout.Button("+ START", EditorStyles.toolbarButton))
            AddNode(NodeType.Start);
        if (GUILayout.Button("+ END", EditorStyles.toolbarButton))
            AddNode(NodeType.End);

        GUILayout.Space(10);

        if (GUILayout.Button("SAVE", EditorStyles.toolbarButton))
            Save();
        if (GUILayout.Button("LOAD", EditorStyles.toolbarButton))
            Load();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Reset View", EditorStyles.toolbarButton))
        {
            zoomLevel = 1f;
            panOffset = Vector2.zero;
        }

        GUILayout.EndHorizontal();
    }

    // ─────────────────────────────────────────────
    // NODOS / SAVE / LOAD
    // ─────────────────────────────────────────────

    private void AddNode(NodeType type)
    {
        var node = new DialogueNodeData
        {
            id = nextId++,
            type = type,
            position = -panOffset + new Vector2(50, 50),
            size = new Vector2(250, 200)
        };

        if (type is NodeType.Start or NodeType.InventoryEvent or NodeType.SpriteEvent or NodeType.AudioEvent)
            node.options.Add(new DialogueOptionData());

        if (type == NodeType.Condition)
        {
            node.options.Add(new DialogueOptionData { text = "True" });
            node.options.Add(new DialogueOptionData { text = "False" });
        }

        if (type == NodeType.Random)
        {
            node.options.Add(new DialogueOptionData { chance = 50 });
            node.options.Add(new DialogueOptionData { chance = 50 });
        }

        nodes.Add(node);
    }

    private void Save()
    {
        if (loadedAsset == null)
        {
            string path = EditorUtility.SaveFilePanelInProject("Save Dialogue", "Dialogue", "asset", "");
            if (string.IsNullOrEmpty(path)) return;

            loadedAsset = CreateInstance<DialogueGraphAsset>();
            AssetDatabase.CreateAsset(loadedAsset, path);
        }

        loadedAsset.nodes = nodes.Select(CloneNode).ToList();
        EditorUtility.SetDirty(loadedAsset);
        AssetDatabase.SaveAssets();
    }

    private void Load()
    {
        string path = EditorUtility.OpenFilePanel("Load Dialogue", "Assets", "asset");
        if (string.IsNullOrEmpty(path)) return;

        loadedAsset = AssetDatabase.LoadAssetAtPath<DialogueGraphAsset>(FileUtil.GetProjectRelativePath(path));
        if (loadedAsset != null)
        {
            nodes = loadedAsset.nodes.Select(CloneNode).ToList();
            nextId = nodes.Count == 0 ? 0 : nodes.Max(n => n.id) + 1;
        }
    }

    private DialogueNodeData CloneNode(DialogueNodeData n)
    {
        return new DialogueNodeData
        {
            id = n.id,
            type = n.type,
            characterName = n.characterName,
            text = n.text,
            position = n.position,
            size = n.size,
            inventoryAction = n.inventoryAction,
            itemName = n.itemName,
            variableName = n.variableName,
            requiredValue = n.requiredValue,
            imageIndex = n.imageIndex,
            spriteToSet = n.spriteToSet,
            audioSourceIndex = n.audioSourceIndex,
            audioClip = n.audioClip,
            audioAction = n.audioAction,
            loop = n.loop,
            options = n.options.Select(o => new DialogueOptionData
            {
                text = o.text,
                targetNodeId = o.targetNodeId,
                chance = o.chance
            }).ToList()
        };
    }

    // ─────────────────────────────────────────────
    // GRID, INPUT Y CONEXIONES
    // ─────────────────────────────────────────────

    private void DrawGrid(float spacing, float alpha)
    {
        Handles.BeginGUI();
        Handles.color = new Color(1, 1, 1, alpha);

        float s = spacing * zoomLevel;
        float ox = (panOffset.x * zoomLevel) % s;
        float oy = (panOffset.y * zoomLevel) % s;

        for (float x = ox; x < position.width; x += s)
            Handles.DrawLine(new Vector3(x, 0), new Vector3(x, position.height));

        for (float y = oy; y < position.height; y += s)
            Handles.DrawLine(new Vector3(0, y), new Vector3(position.width, y));

        Handles.EndGUI();
    }

    private void DrawConnections()
    {
        Handles.BeginGUI();

        foreach (var n in nodes)
        {
            for (int i = 0; i < n.options.Count; i++)
            {
                if (n.options[i].targetNodeId == -1) continue;

                var target = nodes.Find(t => t.id == n.options[i].targetNodeId);
                if (target == null) continue;

                Vector3 start = GetOutputPos(n, i);
                Vector3 end = GetInputPos(target);
                float tan = 50f * zoomLevel;

                Handles.DrawBezier(start, end,
                    start + Vector3.right * tan,
                    end + Vector3.left * tan,
                    Color.white, null, 3f * zoomLevel);
            }
        }

        Handles.EndGUI();
    }

    private void DrawPreviewLine()
    {
        if (fromNodeId == -1) return;

        var n = nodes.Find(x => x.id == fromNodeId);
        if (n == null) return;

        Vector3 start = GetOutputPos(n, fromOptIdx);

        Handles.BeginGUI();
        Handles.DrawBezier(start, Event.current.mousePosition,
            start + Vector3.right * 50 * zoomLevel,
            (Vector3)Event.current.mousePosition + Vector3.left * 50 * zoomLevel,
            Color.yellow, null, 2f * zoomLevel);
        Handles.EndGUI();

        Repaint();
    }

    private Vector3 GetOutputPos(DialogueNodeData n, int optIdx)
    {
        Rect r = ScaleRect(new Rect(n.position, n.size));

        float yBase = 44 * zoomLevel;
        float spacing = 23.5f * zoomLevel;
        float yOffset = yBase + (optIdx * spacing);

        if (n.type == NodeType.Dialogue)
            yOffset += 80 * zoomLevel;
        if (n.type == NodeType.Condition)
            yOffset = r.height * 0.65f + (optIdx * 20 * zoomLevel);
        if (n.type == NodeType.InventoryEvent || n.type == NodeType.SpriteEvent || n.type == NodeType.AudioEvent)
            yOffset = r.height * 0.85f;
        if (n.type == NodeType.Start)
            yOffset = r.height * 0.65f;
        if (n.type == NodeType.Random)
            yOffset += 18 * zoomLevel;

        return new Vector3(r.xMax, r.y + yOffset, 0);
    }

    private Vector3 GetInputPos(DialogueNodeData n)
    {
        Rect r = ScaleRect(new Rect(n.position, n.size));
        return new Vector3(r.xMin, r.y + 40 * zoomLevel, 0);
    }

    private Rect ScaleRect(Rect r) =>
        new Rect((r.x + panOffset.x) * zoomLevel,
            (r.y + panOffset.y) * zoomLevel,
            r.width * zoomLevel,
            r.height * zoomLevel);

    private void HandleEvents()
    {
        Event e = Event.current;

        if (e.type == EventType.ScrollWheel)
        {
            float oldZ = zoomLevel;
            zoomLevel = Mathf.Clamp(zoomLevel - e.delta.y * 0.02f, minZoom, maxZoom);
            panOffset += (e.mousePosition / oldZ - e.mousePosition / zoomLevel);
            e.Use();
        }

        if (e.type == EventType.MouseDrag && (e.button == 1 || e.button == 2))
        {
            panOffset += e.delta / zoomLevel;
            e.Use();
        }

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            DialogueNodeData hit = nodes.LastOrDefault(n =>
                ScaleRect(new Rect(n.position, n.size)).Contains(e.mousePosition));

            if (hit != null)
            {
                if (fromNodeId != -1 && fromNodeId != hit.id)
                {
                    nodes.Find(n => n.id == fromNodeId).options[fromOptIdx].targetNodeId = hit.id;
                    fromNodeId = -1;
                    e.Use();
                }
                else if (e.mousePosition.y < ScaleRect(new Rect(hit.position, hit.size)).y + 25 * zoomLevel)
                {
                    draggingNode = hit;
                    e.Use();
                }
            }
            else
            {
                fromNodeId = -1;
            }
        }

        if (e.type == EventType.MouseDrag && draggingNode != null)
        {
            draggingNode.position += e.delta / zoomLevel;
            e.Use();
        }

        if (e.type == EventType.MouseUp)
            draggingNode = null;
    }
}
