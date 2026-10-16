using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    [System.Serializable]
    public struct AbilityTextPair
    {
        public Abilities abilityData;         
        public TMP_Text shopCustomText;        
        public TMP_Text inventoryCustomText;    
    }
    public List<Abilities> allGameAbilities; 
    public List<AbilityTextPair> abilityTextPairs = new List<AbilityTextPair>();
    public TMP_Text moneyText;
    public TMP_Text descriptionText; 
    public Button buyButton;
    public Button equipButton;
    public Button continueButton;
    public Button mainMenuButton; 
    public Button[] shopItemSlots = new Button[3]; 
    public Button[] inventoryItemSlots = new Button[8]; 
    public GameObject selectionHighlight;              
    public List<Abilities> purchasedAbilities = new List<Abilities>();
    public Abilities equippedSlot1;
    public Abilities equippedSlot2;
    public TMP_Text equipButtonText;

    private Abilities currentlySelectedShopItem;
    private int selectedShopSlotIndex = -1;

    private Abilities currentlySelectedInventoryItem;
    private int selectedInventoryIndex = -1;

    private List<Abilities> currentShopOfferings = new List<Abilities>();
    private int playerMoney = 0;
    private Color[] originalSlotColors = new Color[8];

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        playerMoney = PlayerPrefs.GetInt("PlayerMoney", 0);
        LoadPersistentData();

        for (int i = 0; i < inventoryItemSlots.Length; i++)
        {
            if (inventoryItemSlots[i] != null)
            {
                Image img = inventoryItemSlots[i].GetComponent<Image>();
                if (img != null) originalSlotColors[i] = img.color;
            }
        }

        GenerateShopStock();
        UpdateShopUI();
        UpdateInventoryUI();
    }

    void SavePersistentData()
    {
        List<string> purchasedNames = new List<string>();
        foreach (var item in purchasedAbilities)
        {
            if (item != null) purchasedNames.Add(item.abilityName);
        }
        PlayerPrefs.SetString("SavedPurchasedAbilities", string.Join(",", purchasedNames));

        PlayerPrefs.SetString("EquippedSlot1", equippedSlot1 != null ? equippedSlot1.abilityName : "");
        PlayerPrefs.SetString("EquippedSlot2", equippedSlot2 != null ? equippedSlot2.abilityName : "");
        PlayerPrefs.SetInt("PlayerMoney", playerMoney);
        PlayerPrefs.Save();
    }

    void LoadPersistentData()
    {
        purchasedAbilities.Clear();
        string savedPurchased = PlayerPrefs.GetString("SavedPurchasedAbilities", "");
        if (!string.IsNullOrEmpty(savedPurchased))
        {
            string[] names = savedPurchased.Split(',');
            foreach (string name in names)
            {
                Abilities found = allGameAbilities.Find(a => a != null && a.abilityName == name);
                if (found != null && !purchasedAbilities.Contains(found))
                {
                    purchasedAbilities.Add(found);
                }
            }
        }

        string eq1Name = PlayerPrefs.GetString("EquippedSlot1", "");
        string eq2Name = PlayerPrefs.GetString("EquippedSlot2", "");
        equippedSlot1 = allGameAbilities.Find(a => a != null && a.abilityName == eq1Name);
        equippedSlot2 = allGameAbilities.Find(a => a != null && a.abilityName == eq2Name);
    }

    void GenerateShopStock()
    {
        currentShopOfferings.Clear();
        List<Abilities> availablePool = new List<Abilities>(allGameAbilities);
        availablePool.RemoveAll(a => purchasedAbilities.Contains(a));

        for (int i = 0; i < 3; i++)
        {
            if (availablePool.Count > 0)
            {
                int randIndex = Random.Range(0, availablePool.Count);
                currentShopOfferings.Add(availablePool[randIndex]);
                availablePool.RemoveAt(randIndex);
            }
            else
            {
                currentShopOfferings.Add(null);
            }
        }
    }

    public void UpdateShopUI()
    {
        if (moneyText != null) moneyText.text = $"{playerMoney}";

        foreach (var pair in abilityTextPairs)
        {
            if (pair.shopCustomText != null)
                pair.shopCustomText.gameObject.SetActive(false);
        }

        for (int i = 0; i < shopItemSlots.Length; i++)
        {
            if (shopItemSlots[i] == null) continue;

            Abilities currentItem = (i < currentShopOfferings.Count) ? currentShopOfferings[i] : null;

            if (currentItem != null)
            {
                bool isPurchased = purchasedAbilities.Contains(currentItem);
                shopItemSlots[i].gameObject.SetActive(true);

                if (!isPurchased)
                {
                    shopItemSlots[i].interactable = true;
                    Image img = shopItemSlots[i].GetComponent<Image>();
                    if (img != null) img.color = Color.white;

                    TMP_Text customText = GetShopTextForAbility(currentItem);
                    if (customText != null)
                    {
                        customText.gameObject.SetActive(true);
                        customText.transform.SetParent(shopItemSlots[i].transform, false);
                        customText.alignment = TextAlignmentOptions.Center;
                        customText.transform.localPosition = Vector3.zero;
                        customText.transform.localScale = Vector3.one;
                    }
                }
                else
                {
                    shopItemSlots[i].gameObject.SetActive(false);
                }
            }
            else
            {
                shopItemSlots[i].gameObject.SetActive(false);
            }
        }

        UpdateHighlightPosition();

        if (buyButton != null)
        {
            buyButton.interactable = (currentlySelectedShopItem != null && 
                                     playerMoney >= currentlySelectedShopItem.cost &&
                                     !purchasedAbilities.Contains(currentlySelectedShopItem));
        }
    }

    public void UpdateInventoryUI()
    {
        foreach (var pair in abilityTextPairs)
        {
            if (pair.inventoryCustomText != null)
                pair.inventoryCustomText.gameObject.SetActive(false);
        }

        for (int i = 0; i < inventoryItemSlots.Length; i++)
        {
            if (inventoryItemSlots[i] == null) continue;

            if (i < purchasedAbilities.Count)
            {
                inventoryItemSlots[i].gameObject.SetActive(true);
                Abilities item = purchasedAbilities[i];

                bool isEquipped = (equippedSlot1 != null && item.abilityName == equippedSlot1.abilityName) ||
                                  (equippedSlot2 != null && item.abilityName == equippedSlot2.abilityName);
                
                Image img = inventoryItemSlots[i].GetComponent<Image>();
                if (img != null) 
                {
                    if (isEquipped)
                        img.color = new Color(0.45f, 0.4f, 0.35f, 1f);
                    else
                        img.color = originalSlotColors[i];
                }

                TMP_Text customText = GetInventoryTextForAbility(item);
                if (customText != null)
                {
                    customText.gameObject.SetActive(true);
                    customText.transform.SetParent(inventoryItemSlots[i].transform, false);
                    
                    // Center alignment fix
                    customText.alignment = TextAlignmentOptions.Center;

                    RectTransform rt = customText.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = Vector2.zero;
                        rt.anchorMax = Vector2.one;
                        rt.sizeDelta = Vector2.zero;
                        rt.anchoredPosition = Vector2.zero;
                    }
                    customText.transform.localScale = Vector3.one;
                }
            }
            else
            {
                inventoryItemSlots[i].gameObject.SetActive(false);
            }
        }

        if (equipButton != null)
        {
            equipButton.interactable = (currentlySelectedInventoryItem != null);
            if (equipButtonText != null)
            {
                if (currentlySelectedInventoryItem != null)
                {
                    bool isEquipped = (equippedSlot1 != null && currentlySelectedInventoryItem.abilityName == equippedSlot1.abilityName) ||
                                      (equippedSlot2 != null && currentlySelectedInventoryItem.abilityName == equippedSlot2.abilityName);
                    equipButtonText.text = isEquipped ? "UNEQUIP" : "EQUIP";
                }
                else
                {
                    equipButtonText.text = "EQUIP";
                }
            }
        }
    }

    private TMP_Text GetShopTextForAbility(Abilities ability)
    {
        foreach (var pair in abilityTextPairs)
        {
            if (pair.abilityData == ability) return pair.shopCustomText;
        }
        return null;
    }

    private TMP_Text GetInventoryTextForAbility(Abilities ability)
    {
        foreach (var pair in abilityTextPairs)
        {
            if (pair.abilityData == ability) return pair.inventoryCustomText;
        }
        return null;
    }

    public void UpdateHighlightPosition()
    {
        if (selectionHighlight == null) return;

        bool validShopSelection = false;
        if (selectedShopSlotIndex >= 0 && selectedShopSlotIndex < shopItemSlots.Length && shopItemSlots[selectedShopSlotIndex] != null)
        {
            if (selectedShopSlotIndex < currentShopOfferings.Count && currentShopOfferings[selectedShopSlotIndex] != null)
            {
                if (!purchasedAbilities.Contains(currentShopOfferings[selectedShopSlotIndex]))
                {
                    validShopSelection = true;
                }
            }
        }

        if (validShopSelection)
        {
            selectionHighlight.SetActive(true);
            selectionHighlight.transform.position = shopItemSlots[selectedShopSlotIndex].transform.position;
            selectionHighlight.transform.localScale = Vector3.one;
        }
        else
        {
            selectionHighlight.SetActive(false);
        }
    }

    public void OnClickInspectShopItem(int slotIndex)
    {
        if (slotIndex >= currentShopOfferings.Count || currentShopOfferings[slotIndex] == null) return;

        Abilities clickedItem = currentShopOfferings[slotIndex];
        if (purchasedAbilities.Contains(clickedItem)) return;

        currentlySelectedShopItem = clickedItem;
        selectedShopSlotIndex = slotIndex;
        
        currentlySelectedInventoryItem = null;
        selectedInventoryIndex = -1;

        if (descriptionText != null)
        {
            descriptionText.text = $"{currentlySelectedShopItem.abilityName}\nCost: ${currentlySelectedShopItem.cost}\n\n{currentlySelectedShopItem.description}";
        }

        UpdateShopUI();
        UpdateInventoryUI();
    }

    public void OnClickBuySelected()
    {
        if (currentlySelectedShopItem == null) return;

        if (playerMoney >= currentlySelectedShopItem.cost)
        {
            playerMoney -= currentlySelectedShopItem.cost;
            purchasedAbilities.Add(currentlySelectedShopItem); 
            SavePersistentData();

            currentlySelectedShopItem = null;
            selectedShopSlotIndex = -1;
            SoundManager.Instance?.PlayShopPurchase();
            if (descriptionText != null) descriptionText.text = "Purchased!";

            UpdateShopUI();
            UpdateInventoryUI();
        }
    }

    public void OnClickInspectInventoryItem(int index)
    {
        if(inventoryItemSlots[index] == null) return;
        if (index >= purchasedAbilities.Count) return;

        currentlySelectedInventoryItem = purchasedAbilities[index];
        selectedInventoryIndex = index;
        SoundManager.Instance?.PlayButtonClick();

        
        currentlySelectedShopItem = null;
        selectedShopSlotIndex = -1;

        if (descriptionText != null && currentlySelectedInventoryItem != null)
        {
            string energyInfo = currentlySelectedInventoryItem.isPassive ? "Type: Passive" : $"Energy Cost: {currentlySelectedInventoryItem.energyCost}";
            descriptionText.text = $"{currentlySelectedInventoryItem.abilityName}\n{energyInfo}\n\n{currentlySelectedInventoryItem.description}";
        }

        UpdateShopUI();
        UpdateInventoryUI();
    }

    public void OnClickEquipSelected()
    {
        if (currentlySelectedInventoryItem == null) return;
        bool isEq1 = equippedSlot1 != null && currentlySelectedInventoryItem.abilityName == equippedSlot1.abilityName;
        bool isEq2 = equippedSlot2 != null && currentlySelectedInventoryItem.abilityName == equippedSlot2.abilityName;
        if (isEq1)
        {
            equippedSlot1 = null;
        }
        else if (isEq2)
        {
            equippedSlot2 = null;
        }
        else if (equippedSlot1 == null)
        {
            equippedSlot1 = currentlySelectedInventoryItem;
        }
        else if (equippedSlot2 == null)
        {
            equippedSlot2 = currentlySelectedInventoryItem;
        }
        else
        {
            equippedSlot1 = currentlySelectedInventoryItem;
        }
        SoundManager.Instance?.PlayButtonClick();
        SavePersistentData();
        UpdateInventoryUI();

        if (descriptionText != null && currentlySelectedInventoryItem != null)
        {
            string energyInfo = currentlySelectedInventoryItem.isPassive ? "Type: Passive" : $"Energy Cost: {currentlySelectedInventoryItem.energyCost}";
            descriptionText.text = $"{currentlySelectedInventoryItem.abilityName}\n{energyInfo}\n\n{currentlySelectedInventoryItem.description}";
        }
    }

    public void OnClickContinue()
    {
        SavePersistentData();
        SceneManager.LoadScene("SampleScene");
    }

    public void OnClickMainMenu()
    {
        SavePersistentData();
        PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        SceneManager.LoadScene("Main Menu");
    }
}