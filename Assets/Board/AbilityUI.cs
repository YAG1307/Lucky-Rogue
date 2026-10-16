using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityUI : MonoBehaviour
{
    [System.Serializable]
    public struct AbilityTextPair
    {
        public Abilities abilityData;      
        public TMP_Text customTextField;    
    }
    public GameObject abilityPanel;
    public TMP_Text toggleButtonText;
    public GameObject noAbilitiesText;
    public GameObject slot1Object;
    public Transform slot1TextParent; 
    public GameObject slot2Object;
    public Transform slot2TextParent;
    public List<AbilityTextPair> abilityTextPairs = new List<AbilityTextPair>();
    public Abilities equippedSlot1;
    public Abilities equippedSlot2;
    private bool isOpen = false;

    void Start()
    {
        isOpen = false;
        if (abilityPanel != null) abilityPanel.SetActive(false);
        UpdateDisplay();
    }

    public void TogglePanel()
    {
        isOpen = !isOpen;

        if (abilityPanel != null) abilityPanel.SetActive(isOpen);
        if (toggleButtonText != null) toggleButtonText.text = isOpen ? "View Hand" : "View Abilities";

        if (isOpen) UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (GameManager.Instance != null)
        {
            equippedSlot1 = GameManager.Instance.equippedSlot1;
            equippedSlot2 = GameManager.Instance.equippedSlot2;
        }

        foreach (var pair in abilityTextPairs)
        {
            if (pair.customTextField != null)
                pair.customTextField.gameObject.SetActive(false);
        }

        bool has1 = equippedSlot1 != null;
        if (slot1Object != null) slot1Object.SetActive(has1);
        if (has1 && slot1TextParent != null)
        {
            TMP_Text text1 = GetTextFieldForAbility(equippedSlot1);
            if (text1 != null)
            {
                text1.gameObject.SetActive(true);
                text1.transform.SetParent(slot1TextParent, false);
                
                RectTransform rt = text1.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = Vector2.zero;
                    rt.anchoredPosition = Vector2.zero;
                }
            }
        }

        bool has2 = equippedSlot2 != null;
        if (slot2Object != null) slot2Object.SetActive(has2);
        if (has2 && slot2TextParent != null)
        {
            TMP_Text text2 = GetTextFieldForAbility(equippedSlot2);
            if (text2 != null)
            {
                text2.gameObject.SetActive(true);
                text2.transform.SetParent(slot2TextParent, false);
                
                RectTransform rt = text2.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchorMin = Vector2.zero;
                    rt.anchorMax = Vector2.one;
                    rt.sizeDelta = Vector2.zero;
                    rt.anchoredPosition = Vector2.zero;
                }
            }
        }

        bool empty = !has1 && !has2;
        if (noAbilitiesText != null) noAbilitiesText.SetActive(empty);
    }

    private TMP_Text GetTextFieldForAbility(Abilities ability)
    {
        foreach (var pair in abilityTextPairs)
        {
            if (pair.abilityData == ability) return pair.customTextField;
        }
        return null;
    }

    public void OnClickSlot1() => UseAbility(equippedSlot1);
    public void OnClickSlot2() => UseAbility(equippedSlot2);

    private void UseAbility(Abilities ability)
    {
        SoundManager.Instance?.PlayButtonClick();
        if (ability == null || ability.isPassive) return;

        if (GameManager.Instance != null && GameManager.Instance.energy < ability.energyCost)
        {
            Notification.Instance?.Show("Not enough energy!", Color.red, 1.5f);
            return;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ExecuteAbility(ability);
        }

        TogglePanel();
    }

    public void OnHoverSlot1() => ShowTooltip(equippedSlot1);
    public void OnHoverSlot2() => ShowTooltip(equippedSlot2);

    private void ShowTooltip(Abilities ability)
    {
        if (ability != null)
        {
            Notification.Instance?.Show($"{ability.abilityName}", Color.white, 3f);
        }
    }
}