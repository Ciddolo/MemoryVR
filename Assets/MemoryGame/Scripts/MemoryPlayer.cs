using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;

public class MemoryPlayer : MonoBehaviour
{
    public Transform RightHand;
    public float MaxDistance = 50.0f;
    public bool IsMyTurn;

    private MemoryCard currentCard;

    void Update()
    {
        PointCard();
    }

    private void PointCard()
    {
        Physics.Raycast(RightHand.position, RightHand.forward, out RaycastHit hitInfo, MaxDistance);
        MemoryCard pointedCard = null;
        if (hitInfo.collider)
        {
            if (hitInfo.collider.GetComponent<MemoryCard>())
                pointedCard = hitInfo.collider.GetComponent<MemoryCard>();
        }

        if (!pointedCard)
        {
            DeselectCard();
            return;
        }

        if (currentCard != pointedCard)
            DeselectCard();

        SelectCard(pointedCard);

        if (InputBridge.Instance.RightTriggerDown || InputBridge.Instance.AButtonDown)
            ShowCard();
    }

    private void SelectCard(MemoryCard newCard)
    {
        currentCard = newCard;
        currentCard.SelectCard();
    }

    private void DeselectCard()
    {
        if (!currentCard) return;

        currentCard.DeselectCard();
        currentCard = null;
    }

    private void ShowCard()
    {
        if (!currentCard) return;

        currentCard.ShowCard();
    }
}
