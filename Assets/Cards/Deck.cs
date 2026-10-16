using UnityEngine;
using System.Collections.Generic;

public class Deck : MonoBehaviour
{
    public List<CardSO> allCards;
    public GameObject cardPrefab;
    public Transform hand;
    public List<CardSO> deck = new List<CardSO>();
    public int maxHandSize = 5;

    void Start()
    {
        Shuffle();
    }

    public void Shuffle()
    {
        deck.Clear(); 
        for (int i = 0; i < 1; i++)
        {
            deck.AddRange(allCards);
        }
        for (int i = 0; i < deck.Count; i++)
        {
            CardSO temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    public void DrawCards(int count)
    {
        int drawnCount = 0;
        for (int i = 0; i < count; i++)
        {
            if (hand.childCount >= maxHandSize) break;
            if (deck.Count == 0) Shuffle(); 
            CardSO cardData = deck[0];
            deck.RemoveAt(0);

            GameObject newCard = Instantiate(cardPrefab, hand);
            CardData cardScript = newCard.GetComponent<CardData>();
            cardScript.Display(cardData);
            drawnCount++;
        }
        
    }

    public void ClearHand()
    {
        for (int i = hand.childCount - 1; i >= 0; i--)
        {
            Transform child = hand.GetChild(i);
            child.SetParent(null); 
            Destroy(child.gameObject);
        }
    }

    public bool Discard(GameObject cardObject)
    {
        if (cardObject == null) return false;
        if (cardObject.transform.parent != hand) return false;
        cardObject.transform.SetParent(null);
        cardObject.SetActive(false);
        Destroy(cardObject, 0.01f);
        return true;
    }
    public int GetDeckCount()
    {
        return deck != null ? deck.Count : 0;
    }
    public void RemoveAces()
    {
        if (deck == null || deck.Count == 0) return;
        deck.RemoveAll(card => card != null && card.value == 1 || card.value == 11);
    }
}