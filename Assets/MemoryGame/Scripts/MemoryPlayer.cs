using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;
using Photon.Pun;
using Photon.Realtime;

public class MemoryPlayer : MonoBehaviour
{
    public Transform RightHand;

    public float MaxDistance = 50.0f;

    private MemoryCard currentCard;

    private PhotonView view;

    private void Start()
    {
        view = GetComponent<PhotonView>();
    }

    private void Update()
    {
        PointCard();
    }

    private void PointCard()
    {
        Physics.Raycast(RightHand.position, RightHand.forward, out RaycastHit hitInfo, MaxDistance);
        if (hitInfo.collider)
        {
            MemoryCard pointedCard = hitInfo.collider.GetComponent<MemoryCard>();

            if (pointedCard)
                CardInteraction(pointedCard);
            else
                StopCardInteraction();
        }

        if (currentCard && (InputBridge.Instance.RightTriggerDown || InputBridge.Instance.AButtonDown))
            currentCard.ShowCard();
    }

    private void CardInteraction(MemoryCard newCard)
    {
        if (currentCard == newCard) return;

        if (currentCard)
            currentCard.DeselectCard();

        currentCard = newCard;
        currentCard.SelectCard();
    }

    private void StopCardInteraction()
    {
        if (!currentCard) return;

        currentCard.DeselectCard();
        currentCard = null;
    }

    public void RequestOwnership(Transform cardsParent, int activeCards)
    {
        for (int i = 0; i < activeCards; i++)
            cardsParent.GetChild(i).GetComponent<PhotonView>().RequestOwnership();
    }
}
