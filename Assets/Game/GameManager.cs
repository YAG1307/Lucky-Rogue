using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public EnemyDisplay enemyDisplay;
    public int currentAnte = 1;
    public int turnsPerRound = 3;
    public int totalAntes = 9;
    public int playerHP = 3;
    public int enemyHP = 3;
    public int maxHP = 3;
    public int energy = 5;
    public int maxEnergy = 5;
    public int money = 0;
    public int takeout = 5;
    public int currentTurn = 1;
    public LaneScore[] lanes; 
    public Deck deckScript;
    public EnemyAI enemyAI;
    public TMP_Text playerHPText;
    public TMP_Text energyText;
    public TMP_Text moneyText;
    public TMP_Text turnTrackerText;
    public Button endTurnButton;
    public Button discardButton;
    public Button restButton;
    public GameObject winPanel;
    public GameObject losePanel;
    public Image fade;
    public bool discardDisable = false;
    public TMP_Text BonusText;      
    public TMP_Text TotalText;       
    public TMP_Text AnteTrackerText; 
    public Button ContinueButton;   
    public Button MainMenuButton;    
    public Abilities equippedSlot1;
    public Abilities equippedSlot2;
    public bool aceHigh = false;
    public bool laneRestrict = false;
    public bool laneBoost = false; 
    public bool restrictLaneUsedThisRound = false;
    public bool safetyNetUsed = false;
    public GameObject gameClearPanel;
    public GameObject inputBlocker;
    public List<Abilities> allGameAbilities;
    private int quickHealUsesRemaining = 2;
    private int boostLeft = 2;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);
        quickHealUsesRemaining = 2; 
        StartNewMatch();
    }

    public void StartNewMatch()
    {
        PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
        currentAnte = PlayerPrefs.GetInt("CurrentAnte", 1);
        money = PlayerPrefs.GetInt("PlayerMoney", 0); 
        enemyAI.Setup(currentAnte);
        
        string eq1Name = PlayerPrefs.GetString("EquippedSlot1", "");
        string eq2Name = PlayerPrefs.GetString("EquippedSlot2", "");
        equippedSlot1 = allGameAbilities.Find(a => a != null && a.abilityName == eq1Name);
        equippedSlot2 = allGameAbilities.Find(a => a != null && a.abilityName == eq2Name);
        
        playerHP = maxHP; 
        energy = maxEnergy;
        currentTurn = 1;
        discardDisable = false;
        aceHigh = false;       
        safetyNetUsed = false; 

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        EnemyType currentEnemyType = (EnemyType)((currentAnte - 1) % 3);
        int tierOffset = (currentAnte - 1) / 3; 
        int baseEnemyHP = 3 + tierOffset;
        if (currentEnemyType == EnemyType.HollowTree)
        {
            baseEnemyHP += 1;
        }
        enemyHP = baseEnemyHP;

        takeout = (currentEnemyType == EnemyType.HollowTree) ? 3 : 2;

        if (enemyDisplay != null)
        {
            enemyDisplay.SetupDisplay(currentEnemyType, enemyHP, currentAnte);
        }

        ClearBoard();

        if (deckScript != null)
        {
            deckScript.ClearHand();
            deckScript.Shuffle();
            deckScript.DrawCards(5);
        }

        bool hasPurgeAce = (equippedSlot1 != null && equippedSlot1.abilityName == "Purge Ace") ||
                           (equippedSlot2 != null && equippedSlot2.abilityName == "Purge Ace"); 
        if (hasPurgeAce && deckScript != null)
        {
            deckScript.RemoveAces();
            Notification.Instance?.Show("All Aces removed!", Color.cyan, 2f);
        }

        UpdateUI();
    }

    public void SetPlayerInput(bool enable)
    {
        if (inputBlocker != null)
        {
            inputBlocker.SetActive(!enable);
        }
    }

    public void OnEndTurn()
    {
        SoundManager.Instance?.PlayButtonClick();           
        if (energy < 2)
        {
            Notification.Instance?.Show("Low Energy! Look Away!", Color.red, 2.5f);
            return; 
        }
        SetPlayerInput(false);
        energy -= 2;
        discardDisable = false;
        if (enemyAI != null) enemyAI.PlayEnemyTurn();

        if (currentTurn >= turnsPerRound)
        {
            EvaluateMatch();
        }
        else
        {
            currentTurn++;
            if (deckScript != null) deckScript.DrawCards(3);
            UpdateUI();
        }
    }
    
    public void EvaluateMatch()
    {
        int playerLanesWon = 0;
        int enemyLanesWon = 0;
        boostLeft = 2;
        aceHigh = false;

        foreach (LaneScore lane in lanes)
        {
            if (lane == null) continue;
            lane.CalcScore();

            if (lane.playerScore > lane.enemyScore)
            {
                playerLanesWon++;
            }
            else if (lane.enemyScore > lane.playerScore)
            {
                enemyLanesWon++;
            }
        }

        if (playerLanesWon > enemyLanesWon)
        {
            enemyHP = Mathf.Max(0, enemyHP - 1);
            Notification.Instance?.Show("ENEMY LOST HEALTH!", Color.green, 2.5f);
            SoundManager.Instance?.PlayLoseLife();
            if (enemyDisplay != null) enemyDisplay.UpdateHealth(enemyHP);
        }

        else if (enemyLanesWon > playerLanesWon)
        {
            bool hasSafetyNet = (equippedSlot1 != null && equippedSlot1.abilityName == "Safety Net") ||
                                (equippedSlot2 != null && equippedSlot2.abilityName == "Safety Net");

            if (hasSafetyNet && !safetyNetUsed)
            {
                safetyNetUsed = true; 
                Notification.Instance?.Show("Blocked Health Loss!", Color.cyan, 2.5f);
            }
            else
            {
                playerHP = Mathf.Max(0, playerHP - 1); 
                UpdateUI();
                Notification.Instance?.Show("LOST HEALTH!", Color.red, 2.5f);
                SoundManager.Instance?.PlayLoseLife();
            }
        }

        else if(playerLanesWon == enemyLanesWon)
        {
            enemyHP = Mathf.Max(0, enemyHP - 1);
            playerHP = Mathf.Max(0, playerHP - 1); 
            if (enemyDisplay != null) enemyDisplay.UpdateHealth(enemyHP);
            UpdateUI();
            SoundManager.Instance?.PlayLoseLife();
            Notification.Instance?.Show("TIE! LOST HEALTH.", Color.green, 2.5f);
        }

        if (playerHP <= 0)
        {
            if (AnteTrackerText != null)
            {
                AnteTrackerText.text = $"Reached Ante {currentAnte}/{totalAntes}";
            }

            losePanel.SetActive(true);
            inputBlocker.SetActive(true);

            PlayerPrefs.DeleteKey("HasActiveRun");
            PlayerPrefs.DeleteKey("CurrentAnte");
            PlayerPrefs.DeleteKey("PlayerMoney");
            PlayerPrefs.DeleteKey("EquippedSlot1");
            PlayerPrefs.DeleteKey("EquippedSlot2");
            PlayerPrefs.Save();
            UpdateUI();
            return;
        }

        if (enemyHP <= 0)
        {
            int pocketMoney = 0;
            bool hasDeepPockets = (equippedSlot1 != null && equippedSlot1.abilityName == "Deep Pockets") || 
                                (equippedSlot2 != null && equippedSlot2.abilityName == "Deep Pockets");

            if (hasDeepPockets && deckScript != null)
            {
                int cardsLeft = deckScript.GetDeckCount();
                pocketMoney = (cardsLeft / 7) * 1; 
            }

            int bonusMoney = CalculateBonusMoney();
            int totalEarned = takeout + bonusMoney + pocketMoney; 
            money += totalEarned; 

            if (BonusText != null)
            {
                if (pocketMoney > 0)
                    BonusText.text = $"Bonus: ${bonusMoney} | Deep Pockets: +${pocketMoney}";
                else
                    BonusText.text = $"Bonus: ${bonusMoney}";
            }

            if (TotalText != null)
            {
                TotalText.text = $"Total Earned: ${totalEarned}";
            }

            PlayerPrefs.SetInt("PlayerMoney", money);

            if (currentAnte >= totalAntes)
            {
                if (AnteTrackerText != null) AnteTrackerText.text = $"FINAL ANTE COMPLETED!";

                gameClearPanel.SetActive(true);
                inputBlocker.SetActive(true);
                

                PlayerPrefs.DeleteKey("HasActiveRun");
                PlayerPrefs.DeleteKey("CurrentAnte");
                PlayerPrefs.DeleteKey("PlayerMoney");
                PlayerPrefs.DeleteKey("EquippedSlot1");
                PlayerPrefs.DeleteKey("EquippedSlot2");
                PlayerPrefs.Save();
            }
            else
            {
                if (AnteTrackerText != null) AnteTrackerText.text = $"Ante {currentAnte}/{totalAntes} Completed!";


                winPanel.SetActive(true);
                inputBlocker.SetActive(true);
                

                currentAnte++;
                PlayerPrefs.SetInt("CurrentAnte", currentAnte);
                PlayerPrefs.SetInt("HasActiveRun", 1);
                PlayerPrefs.Save();
            }

            UpdateUI();
            return;
        }

        ClearBoard();
        currentTurn = 1;
        energy = maxEnergy;
        discardDisable = false;
        if (deckScript != null)
        {
            deckScript.ClearHand(); 
            deckScript.Shuffle();
            deckScript.DrawCards(5); 
        }

        bool hasPurgeAceRound = (equippedSlot1 != null && equippedSlot1.abilityName == "Purge Ace") ||
                                (equippedSlot2 != null && equippedSlot2.abilityName == "Purge Ace"); 
        if (hasPurgeAceRound && deckScript != null)
        {
            deckScript.RemoveAces();
        }

        UpdateUI();
    }    

    public void OnWinContinueToShop()
    {
        SoundManager.Instance?.PlayButtonClick(); 
        
        if (currentAnte > totalAntes)
        {
            SceneManager.LoadScene("Main Menu");
            return;
        }
        SceneManager.LoadScene("Shop");
    }

    public void OnGoToMainMenu()
    {
        PlayerPrefs.SetString("LastScene", SceneManager.GetActiveScene().name);
        PlayerPrefs.Save();
        SoundManager.Instance?.PlayButtonClick();           
        SceneManager.LoadScene("Main Menu");
    }

    public int CalculateBonusMoney()
    {
        int totalBonus = 0;
        if (lanes == null) return 0;

        foreach (LaneScore lane in lanes)
        {
            if (lane == null || lane.playerLayer == null) continue;
            Dictionary<int, int> valueCounts = new Dictionary<int, int>();
            CardData[] cards = lane.playerLayer.GetComponentsInChildren<CardData>();

            foreach (CardData card in cards)
            {
                if (valueCounts.ContainsKey(card.cardValue))
                    valueCounts[card.cardValue]++;
                else
                    valueCounts[card.cardValue] = 1;
            }
            foreach (var count in valueCounts.Values)
            {
                if (count == 2) totalBonus += 1;
                else if (count == 3) totalBonus += 2;
                else if (count >= 4) totalBonus += 4;
            }
        }
        return totalBonus;
    }

    public void ClearBoard()
    {
        if (lanes == null) return;
        if (restrictLaneUsedThisRound) restrictLaneUsedThisRound = false;
        foreach (LaneScore lane in lanes)
        {
            if (lane == null) continue;
            lane.bonus = 0;
            lane.ResetLaneState();
            if (lane.playerLayer != null) ClearLayer(lane.playerLayer);
            if (lane.enemyLayer != null) ClearLayer(lane.enemyLayer);
        }
    }

    private void ClearLayer(Transform layerTransform)
    {
        if (layerTransform == null) return;
        for (int i = layerTransform.childCount - 1; i >= 0; i--)
        {
            Destroy(layerTransform.GetChild(i).gameObject);
        }
    }

    public void OnDiscard()
    {
        if (discardDisable) return;
        SoundManager.Instance?.PlayButtonClick();           
        if (energy < 1)
        {
            Notification.Instance?.Show("Not enough energy!", Color.red, 1.5f);
            return;
        }

        if (CardData.selectedCard == null) return;

        GameObject cardToDestroy = CardData.selectedCard.gameObject;
        CardData.selectedCard = null;
        energy -= 1;
        if (deckScript != null && cardToDestroy != null)
        {
            deckScript.Discard(cardToDestroy);
            deckScript.DrawCards(1);
        }
        Notification.Instance?.Show("Card Discarded!", Color.white, 1.5f);
        UpdateUI();
    }

    public void ExecuteAbility(Abilities ability)
    {
        if (energy < ability.energyCost)
        {
            Notification.Instance?.Show("Not enough energy!", Color.red, 1.5f);
            return;
        }

        if (ability.abilityName == "Quick Heal")
        {
            if (quickHealUsesRemaining <= 0)
            {
                Notification.Instance?.Show("Limit reached!", Color.red, 1.5f);
                return;
            }

            if (playerHP < maxHP)
            {
                quickHealUsesRemaining--;
                playerHP += 1;
                energy -= ability.energyCost;
                UpdateUI();
                Notification.Instance?.Show($"Healed 1 HP! ({quickHealUsesRemaining} left)", Color.green, 1.5f);
            }
            else return;
        }
        else if (ability.abilityName == "Ace High")
        {
            if(aceHigh) return;
            aceHigh = true;
            energy -= ability.energyCost;
            foreach (LaneScore lane in lanes) if (lane != null) lane.CalcScore();
            Notification.Instance?.Show("Ability Used!", Color.green, 1.5f);
            UpdateUI();
        }
        else if (ability.abilityName == "Lane Boost")
        {
            if(boostLeft <= 0)
            {
                Notification.Instance?.Show("Limit reached!", Color.red, 1.5f);
                return;
            }
            boostLeft--;
            energy -= ability.energyCost;
            laneBoost = true;
            Notification.Instance?.Show("Select a lane!", Color.yellow, 2.5f);
            UpdateUI();
        }
        else if (ability.abilityName == "No Zone") 
        {
            if (restrictLaneUsedThisRound) return;
            energy -= ability.energyCost;
            laneRestrict = true;
            Notification.Instance?.Show("Click a lane!", Color.green, 2.5f);
        }
        else if (ability.abilityName == "Peek Hand")
        {
            energy -= ability.energyCost;
            string enemyCards = enemyAI != null ? enemyAI.GetHand() : "No enemy found!";
            Notification.Instance?.Show(enemyCards, Color.cyan, 3f);
            UpdateUI();
        }
    }

    public void Rest()
    {
        StartCoroutine(RestRout());
        SoundManager.Instance?.PlayButtonClick();           
    }

    private IEnumerator RestRout()
    {
        int energyRestored = Mathf.Min(2, maxEnergy - energy);
        energy = Mathf.Min(energy + 2, maxEnergy);
        if (fade != null) fade.gameObject.SetActive(true);
        Color c = fade != null ? fade.color : Color.black;    
        for (float a = 0; a <= 1f; a += Time.deltaTime * 3f)
        {
            c.a = a;
            if (fade != null) fade.color = c;
            yield return null;
        }     
        if (enemyAI != null) enemyAI.PerformCheat();
        yield return new WaitForSeconds(0.3f); 
        if (fade != null) fade.color = c;
        for (float a = 1f; a >= 0f; a -= Time.deltaTime * 3f)
        {
            c.a = a;
            if (fade != null) fade.color = c;
            yield return null;
        }
        if (fade != null) fade.gameObject.SetActive(false);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (playerHPText != null) playerHPText.text = $"HP: {playerHP}/{maxHP}";
        if (energyText != null) energyText.text = $"Energy: {energy}/{maxEnergy}";
        if (moneyText != null) moneyText.text = $"Money: ${money}";
        if (discardButton != null) discardButton.interactable = !discardDisable && (energy >= 1);
        if (restButton != null) restButton.interactable = (energy < maxEnergy);
        int turnsLeft = (turnsPerRound - currentTurn) + 1;
        if (turnTrackerText != null) turnTrackerText.text = $"Turn: {currentTurn}/{turnsPerRound} ({turnsLeft} Left)";
    }
}