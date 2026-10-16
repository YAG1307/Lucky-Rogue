using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum EnemyType
{
    FallSkeleton,   
    PumpkinMonster,     
    HollowTree 
}

public class EnemyAI : MonoBehaviour
{
    public EnemyType currentEnemyType = EnemyType.FallSkeleton;
    public float decisionDelay = 1.5f; 

    public LaneScore[] lanes; 
    public GameObject cardPrefab; 
    public List<CardSO> cardDatabase = new List<CardSO>();
    public List<CardSO> hand = new List<CardSO>();
    public bool isMoving = false;

    public void Setup(int ante)
    {
        int typeIndex = (ante - 1) % 3;
        currentEnemyType = (EnemyType)typeIndex;
    }

    public void PlayEnemyTurn()
    {
        StartCoroutine(ExecuteTurn());
    }

    private IEnumerator ExecuteTurn()
    {
        yield return new WaitForSeconds(decisionDelay);
        hand.Clear();
        if (cardDatabase.Count == 0 || lanes == null || lanes.Length == 0) yield break;

        int drawAmount = Random.Range(3, 6);
        for (int i = 0; i < drawAmount; i++)
        {
            hand.Add(cardDatabase[Random.Range(0, cardDatabase.Count)]);
        }

        while (hand.Count > 0)
        {
            int bestCardIndex = 0;
            int bestLaneIndex = 0;
            float bestUtilityScore = -999f;

            for (int card = 0; card < hand.Count; card++)
            {
                int cardVal = hand[card].value;

                for (int l = 0; l < lanes.Length; l++)
                {
                    LaneScore lane = lanes[l];
                    if (lane == null) continue;
                    if (lane.enemyLayer != null && lane.enemyLayer.childCount >= 4) continue;

                    int gain = CalculateGain(lane, cardVal);
                    int currentEnemyScore = lane.enemyScore;
                    int currentPlayerScore = lane.playerScore;

                    float utility = gain;

                    if (currentEnemyScore < currentPlayerScore && (currentEnemyScore + gain) >= currentPlayerScore)
                    {
                        utility += 15f; 
                    }
                    else if (currentEnemyScore < currentPlayerScore)
                    {
                        utility += 5f;
                    }
                    else if (currentEnemyScore > currentPlayerScore + 20)
                    {
                        utility -= 5f;
                    }

                    if (utility > bestUtilityScore)
                    {
                        bestUtilityScore = utility;
                        bestCardIndex = card;
                        bestLaneIndex = l;
                    }
                }
            }

            SpawnCard(lanes[bestLaneIndex], hand[bestCardIndex]);
            hand.RemoveAt(bestCardIndex);
            yield return new WaitForSeconds(0.4f);
            GameManager.Instance.SetPlayerInput(true);
        }
    }

    private int CalculateGain(LaneScore lane, int newCardValue)
    {
    List<int> currentValues = new List<int>();
    foreach (Transform child in lane.enemyLayer)
    {
        CardData data = child.GetComponent<CardData>();
        if (data != null) currentValues.Add(data.cardValue);
    }

    int oldScore = LaneScore.ListScore(currentValues);

    currentValues.Add(newCardValue);
    int newScore = LaneScore.ListScore(currentValues);

    return newScore - oldScore;
    }
    void SpawnCard(LaneScore lane, CardSO cardSO)
    {
        GameObject newCard = Instantiate(cardPrefab, lane.enemyLayer);
        CardData data = newCard.GetComponent<CardData>();
        newCard.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
        CanvasGroup cg = newCard.GetComponent<CanvasGroup>();
        if (cg != null) cg.blocksRaycasts = false;
        
        CardDrag dragScript = newCard.GetComponent<CardDrag>();
        if (dragScript != null) dragScript.isPlaced = true;
        
        data.Display(cardSO);
        lane.CalcScore();
    }

    public void PerformCheat()
    {
        Notification.Instance.Show("ENEMY CHEATED!", Color.red, 2.5f);  
        SoundManager.Instance?.PlayCheat();
        switch (currentEnemyType)
        {
            case EnemyType.FallSkeleton:
                SwapCards();
                break;
            case EnemyType.HollowTree:
                RemoveCard();
                break;

            case EnemyType.PumpkinMonster:
                GameManager.Instance.discardDisable = true;
                break;
        }
    }
    void SwapCards()
    {
        if (lanes == null || lanes.Length < 2) return;
        
        List<LaneScore> validLanes = new List<LaneScore>();
        foreach (LaneScore lane in lanes)
        {
            if (lane != null && lane.playerLayer != null && lane.playerLayer.childCount > 0)
            {
                validLanes.Add(lane);
            }
        }

        if (validLanes.Count >= 2)
        {
            int r1 = Random.Range(0, validLanes.Count);
            int r2 = Random.Range(0, validLanes.Count);
            while (r2 == r1) 
            {
                r2 = Random.Range(0, validLanes.Count);
            }

            Transform layerA = validLanes[r1].playerLayer;
            Transform layerB = validLanes[r2].playerLayer;

            Transform cardA = layerA.GetChild(layerA.childCount - 1);
            Transform cardB = layerB.GetChild(layerB.childCount - 1);

            cardA.SetParent(layerB, false);
            cardB.SetParent(layerA, false);

            if (layerA is RectTransform rectA) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectA);
            if (layerB is RectTransform rectB) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rectB);
        }

        foreach (LaneScore lane in lanes)
        {
            if (lane != null) lane.CalcScore();
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateUI();
        }
    }
    void RemoveCard()
    {
        LaneScore playerThreatLane = null;
        int maxPlayerLead = -1;

        foreach (LaneScore lane in lanes)
        {
            if (lane == null) continue;
            int lead = lane.playerScore - lane.enemyScore;
            if (lead > maxPlayerLead && lane.playerLayer.childCount > 0)
            {
                maxPlayerLead = lead;
                playerThreatLane = lane;
            }
        }

        if (playerThreatLane != null && playerThreatLane.playerLayer.childCount > 0)
        {
            Transform topCard = playerThreatLane.playerLayer.GetChild(playerThreatLane.playerLayer.childCount - 1);
            topCard.SetParent(null);
            Destroy(topCard.gameObject);
        }

        foreach (LaneScore lane in lanes)
        {
            if (lane != null) lane.CalcScore();
        }

        if (GameManager.Instance != null) GameManager.Instance.UpdateUI();
    }
     public string GetHand()
    {
    if (cardDatabase == null || cardDatabase.Count == 0) return "EMPTY!";

        List<string> displayValues = new List<string>();

        if (hand != null && hand.Count > 0)
        {
            foreach (CardSO card in hand)
            {
                if (card != null) displayValues.Add(card.value.ToString());
            }
        }
        else
        {
            int previewCount = 5; 
            for (int i = 0; i < previewCount; i++)
            {
                CardSO randomCard = cardDatabase[Random.Range(0, cardDatabase.Count)];
                if (randomCard != null) displayValues.Add(randomCard.value.ToString());
            }
        }

        return "Enemy Hand: " + string.Join(", ", displayValues);
    }
}