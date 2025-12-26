using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameTagText; // NUEVO: Referencia al nombre
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private List<Button> optionButtons;

    public Action<int> OnOptionSelected;

    void Awake()
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            int index = i;
            optionButtons[i].onClick.RemoveAllListeners(); 
            optionButtons[i].onClick.AddListener(() => OnOptionSelected?.Invoke(index));
            optionButtons[i].gameObject.SetActive(false);
        }
    }

    // MODIFICADO: Ahora recibe nombre y texto
    public void ShowDialogue(string characterName, string text) 
    {
        if(nameTagText != null) nameTagText.text = characterName;
        dialogueText.text = text;
    }

    public void ShowOptions(List<string> options)
    {
        for (int i = 0; i < optionButtons.Count; i++)
        {
            if (i < options.Count)
            {
                optionButtons[i].gameObject.SetActive(true);
                optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i];
            }
            else optionButtons[i].gameObject.SetActive(false);
        }
    }

    public void HideAllOptions() { foreach(var b in optionButtons) b.gameObject.SetActive(false); }
}