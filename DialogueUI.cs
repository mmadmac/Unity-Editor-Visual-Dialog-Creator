using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameTagText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private List<Button> optionButtons;

    [Header("Sprite Targets")]
    [SerializeField] private List<Image> spriteTargets;

    [Header("Audio Targets")]
    [SerializeField] private List<AudioSource> audioSources;

    public Action<int> OnOptionSelected;

    void Awake()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            int index = i;
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

    public void ShowDialogue(string characterName, string text)
    {
        if (nameTagText != null)
            nameTagText.text = characterName;

        if (dialogueText != null)
            dialogueText.text = text;
    }

    public void ShowOptions(List<string> options)
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i < options.Count)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i]
                    .GetComponentInChildren<TextMeshProUGUI>()
                    .text = options[i];
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void HideAllOptions()
    {
        foreach (var b in optionButtons)
            b.gameObject.SetActive(false);
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
            src.loop = loop; // ← aquí usamos la nueva propiedad
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
