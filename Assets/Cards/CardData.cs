using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CardData : MonoBehaviour, IPointerDownHandler
{
    public CardSO currentCard;
    public static CardData selectedCard;
    public Image cardArtImage; 
    public Image highlight; 
    public int cardValue => currentCard != null ? currentCard.value : 0; 

    void Awake()
    {
        if (highlight != null)
        {
            highlight.enabled = false;
        }
    }

    public void Display(CardSO cardSO)
    {
        currentCard = cardSO;
        if (cardArtImage != null && cardSO != null)
        {
            cardArtImage.sprite = cardSO.art;
            cardArtImage.enabled = true;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (selectedCard == this)
        {
            SetHighlight(false);
            selectedCard = null;
        }
        else
        {
            if (selectedCard != null)
            {
                selectedCard.SetHighlight(false);
            }
            selectedCard = this;
            SetHighlight(true);
            SoundManager.Instance?.PlayCardClick();
            Notification.Instance.Show(cardValue.ToString());
        }
    }

    public void SetHighlight(bool active)
    {
        if (highlight != null)
        {
            highlight.enabled = active;
        }
    }
}