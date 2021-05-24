using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;
using Photon.Realtime;

public class MemoryPlayer : MonoBehaviour
{
    public MemoryManager GameManager;
    public Transform RightHand;

    public float MaxDistance = 50.0f;

    public bool IsMyTurn;
    public int MovesCounter;

    public Player PhotonPlayer;

    private MemoryCard currentCard;

    private void Start()
    {
        GameManager.AddPlayer(this);
    }

    private void Update()
    {
        PointCard();
    }

    private void PointCard()
    {
        if (!IsMyTurn) return;

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
