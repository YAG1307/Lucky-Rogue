using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class LaneScore : MonoBehaviour
{
    public Transform playerLayer;
    public Transform enemyLayer;
    public TMP_Text playerScoreText;
    public TMP_Text enemyScoreText;
    public int playerScore;
    public int enemyScore;
    public int bonus = 0;
    public GameObject restrictionBox;
    public bool isRestricted = false;

    public void CalcScore()
    {
        if (isRestricted)
        {
            playerScore = 0;
            enemyScore = 0;
            if (playerScoreText != null) playerScoreText.text = "0";
            if (enemyScoreText != null) enemyScoreText.text = "0";
            return;
        }

        playerScore = GetLayerScore(playerLayer) + bonus;
        enemyScore = GetLayerScore(enemyLayer);

        if (playerScoreText != null) playerScoreText.text = playerScore.ToString();
        if (enemyScoreText != null) enemyScoreText.text = enemyScore.ToString();        
    }

    public int GetLayerScore(Transform layer)
    {
        if (layer == null || layer.childCount == 0) return 0;

        List<int> values = new List<int>();
        foreach (Transform child in layer)
        {
            CardData card = child.GetComponent<CardData>();
            if (card != null)
            {
                int val = card.cardValue;
                if (val == 1 && GameManager.Instance != null && GameManager.Instance.aceHigh)
                {
                    val = 11;
                }
                values.Add(val);
            }
        }

        return ListScore(values);
    }

    public static int ListScore(List<int> values)
    {
        if (values == null || values.Count == 0) return 0;

        Dictionary<int, int> cardCounts = new Dictionary<int, int>();
        foreach (int val in values)
        {
            if (!cardCounts.ContainsKey(val))
            {
                cardCounts[val] = 0;
            }
            cardCounts[val]++;
        }

        int total = 0;
        foreach (var entry in cardCounts)
        {
            int val = entry.Key;
            int count = entry.Value;

            if (count == 1)
            {
                total += val;
            }
            else if (count >= 2)
            {
                int pairValue = (val == 1) ? (val * count) : (val * val); 
                int remainingAdditions = (val == 1) ? 0 : ((count - 2) * val);
                total += (pairValue + remainingAdditions);
            }
        }

        return total;
    }

    public void RestrictLane()
    {
        isRestricted = true;
        if (restrictionBox != null) restrictionBox.SetActive(true);
    }

    public void ResetLaneState()
    {
        isRestricted = false;
        if (restrictionBox != null) restrictionBox.SetActive(false);
        if (playerScoreText != null) playerScoreText.text = "0";
        if (enemyScoreText != null) enemyScoreText.text = "0";
    }
}