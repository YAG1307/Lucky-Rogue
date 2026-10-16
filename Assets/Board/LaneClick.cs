using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class LaneClick : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance?.PlayButtonClick();
        
        LaneScore lane = GetComponent<LaneScore>();
        if (lane == null) lane = GetComponentInParent<LaneScore>();

        if (GameManager.Instance != null && GameManager.Instance.laneBoost)
        {
            GameManager.Instance.laneBoost = false;
            if (lane != null)
            {
                lane.bonus += 10; 
                lane.CalcScore();
            }
            Notification.Instance?.Show("Lane Boosted!", Color.green, 1.5f);
            
            if (GameManager.Instance != null) GameManager.Instance.UpdateUI();
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.laneRestrict)
        {
            if (lane != null)
            {
                lane.RestrictLane();
                GameManager.Instance.restrictLaneUsedThisRound = true;
            }
            GameManager.Instance.laneRestrict = false;
            Notification.Instance?.Show("Lane Restricted!", Color.red, 1.5f);
            
            if (GameManager.Instance != null) GameManager.Instance.UpdateUI();
            return;
        }

        CardData[] cards = GetComponentsInChildren<CardData>();
        if (cards == null || cards.Length == 0)
        {
            return;
        }
        List<string> values = new List<string>();
        foreach (CardData c in cards)
        {
            values.Add(c.cardValue.ToString());
        }
        string cardList = string.Join(", ", values);
        Notification.Instance?.Show(cardList, Color.white, 2.5f);
    }
}