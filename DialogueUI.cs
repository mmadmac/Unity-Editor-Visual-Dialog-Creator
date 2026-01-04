using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private List<TextMeshProUGUI> dialogueTargets = new();
    [SerializeField] private List<Button> optionButtons = new();

    [Header("Sprite Targets")]
    [SerializeField] private List<Image> spriteTargets = new();

    [Header("Audio Targets")]
    [SerializeField] private List<AudioSource> audioSources = new();

    public Action<int> OnOptionSelected;

    void Awake()
    {
        if (optionButtons == null) return;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            int index = i;
            if (optionButtons[i] == null) continue;

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(
                () => OnOptionSelected?.Invoke(index)
            );
            optionButtons[i].gameObject.SetActive(false);
        }
    }

    // ─────────────────────────────────────────────
    // DIÁLOGO
    // ─────────────────────────────────────────────

    /// <summary>
    /// Pinta una línea de diálogo en el target indicado.
    /// </summary>
    public void ShowLine(int targetIndex, string text)
    {
        if (dialogueTargets == null) return;
        if (targetIndex < 0 || targetIndex >= dialogueTargets.Count) return;

        var target = dialogueTargets[targetIndex];
        if (target == null) return;

        target.text = text;
    }

    /// <summary>
    /// Limpia todos los targets de diálogo (opcional, por si lo necesitas).
    /// </summary>
    public void ClearAllDialogueTargets()
    {
        if (dialogueTargets == null) return;

        foreach (var t in dialogueTargets)
        {
            if (t == null) continue;
            t.text = string.Empty;
        }
    }

    public void ShowOptions(List<string> options)
    {
        if (optionButtons == null) return;

        for (int i = 0; i < optionButtons.Count; i++)
        {
            var btn = optionButtons[i];
            if (btn == null) continue;

            if (i < options.Count)
            {
                btn.gameObject.SetActive(true);
                var label = btn.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null)
                {
                    label.text = options[i];
                }
            }
            else
            {
                btn.gameObject.SetActive(false);
            }
        }
    }

    public void HideAllOptions()
    {
        if (optionButtons == null) return;

        foreach (var b in optionButtons)
        {
            if (b == null) continue;
            b.gameObject.SetActive(false);
        }
    }

    // ─────────────────────────────────────────────
    // SPRITES
    // ─────────────────────────────────────────────

    /// <summary>
    /// Cambia el sprite de un Image.
    /// Si el sprite es null, el Image se oculta.
    /// </summary>
    public void SetSprite(int index, Sprite sprite)
    {
        if (spriteTargets == null) return;
        if (index < 0 || index >= spriteTargets.Count) return;

        Image img = spriteTargets[index];
        if (img == null) return;

        if (sprite == null)
        {
            img.sprite = null;
            img.enabled = false;
        }
        else
        {
            img.sprite = sprite;
            img.enabled = true;
        }
    }

    /// <summary>
    /// Oculta todos los Images y elimina sus sprites.
    /// </summary>
    public void ClearAllSprites()
    {
        if (spriteTargets == null) return;

        foreach (var img in spriteTargets)
        {
            if (img == null) continue;
            img.sprite = null;
            img.enabled = false;
        }
    }

    // ─────────────────────────────────────────────
    // AUDIO
    // ─────────────────────────────────────────────

    /// <summary>
    /// Maneja eventos de audio desde el diálogo.
    /// </summary>
    public void HandleAudio(int index, AudioClip clip, AudioAction action, bool loop = false)
    {
        if (audioSources == null) return;
        if (index < 0 || index >= audioSources.Count) return;

        AudioSource src = audioSources[index];
        if (src == null) return;

        switch (action)
        {
            case AudioAction.Play:
                src.clip = clip;
                src.loop = loop;
                src.Play();
                break;

            case AudioAction.PlayOneShot:
                src.loop = false; // one shot nunca hace loop
                if (clip != null)
                    src.PlayOneShot(clip);
                break;

            case AudioAction.Stop:
                src.Stop();
                break;
        }
    }
}
