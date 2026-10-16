using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.EventSystems;
public class LaneDrop : MonoBehaviour, IDropHandler 
{
    public Vector3 boardCard = new Vector3(0.6f, 0.6f, 1f);
 
    public void OnDrop(PointerEventData eventData)
    {
        CardDrag card = eventData.pointerDrag.GetComponent<CardDrag>();
        CardData cardData = card.GetComponent<CardData>();
        if (card == null) return;
        if (transform.childCount < 4)
        {
            card.transform.SetParent(this.transform);
            card.parentToReturnTo = this.transform;
            card.isPlaced = true;
            card.transform.localScale = boardCard;
            cardData.SetHighlight(false);
            if (CardData.selectedCard == cardData)
            {
                CardData.selectedCard = null;    
            }
        }
        if (GameManager.Instance != null && GameManager.Instance.lanes != null)
        {
            foreach (LaneScore lane in GameManager.Instance.lanes)
            {
                if (lane != null) lane.CalcScore();
            }
        }
    }
}