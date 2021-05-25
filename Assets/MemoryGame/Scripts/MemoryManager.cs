using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum MemoryDifficulty
{
    Easy = 6,
    Medium = 8,
    Hard = 12
}

public class MemoryManager : MonoBehaviour, IPunObservable
{
    private readonly float columnsOffset = 0.3f;
    private readonly float rowsOffset = 0.3f;
    private readonly float cardWidth = 0.4f;
    private readonly float cardHeight = 0.6f;

    private Transform cardsParent;

    private List<MemoryPlayer> players;
    private MemoryPlayer currentPlayer;

    private PhotonView view;

    public int difficulty;
    public int syncDifficulty;

    public Vector3 cardsParentPosition;
    public Vector3 syncCardsParentPosition;

    public int activeCards;
    public int syncActiveCards;

    public Vector3[] cardPositions;
    public Vector3[] syncCardPositions;

    void Awake()
    {
        cardsParent = transform.GetChild(0);
        players = new List<MemoryPlayer>();
        view = GetComponent<PhotonView>();

        difficulty = 8;
        syncDifficulty = 8;

        cardsParentPosition = Vector3.zero;
        syncCardsParentPosition = Vector3.zero;

        activeCards = 0;
        syncActiveCards = 0;

        cardPositions = new Vector3[(int)MemoryDifficulty.Hard];
        syncCardPositions = new Vector3[(int)MemoryDifficulty.Hard];        
    }

    private void Update()
    {
        if (!view.IsMine)
        {
            Debug.Log("NOT MINE");
            difficulty = syncDifficulty;
            SetDifficulty(difficulty);

            cardsParentPosition = syncCardsParentPosition;
            cardsParent.position = cardsParentPosition;

            activeCards = syncActiveCards;
            for (int i = 0; i < cardsParent.childCount; i++)
                cardsParent.GetChild(i).gameObject.SetActive(i < activeCards);

            cardPositions = syncCardPositions;
            for (int i = 0; i < activeCards; i++)
                cardsParent.GetChild(i).localPosition = cardPositions[i];
        }
    }

    //public void InstantiateCards()
    //{
    //    for (int i = 0; i < (int)MemoryDifficulty.Hard; i++)
    //    {
    //        GameObject currentCard = PhotonNetwork.Instantiate("MemoryCard", Vector3.zero, Quaternion.identity);
    //        currentCard.transform.SetParent(cardsParent);
    //        currentCard.SetActive(false);
    //    }
    //}

    public void PlaceCards()
    {
        //if (!view.IsMine) return;

        for (int i = 0; i < cardsParent.childCount; i++)
        {
            GameObject currentCard = cardsParent.GetChild(i).gameObject;
            currentCard.GetComponent<MemoryCard>().ResetCard();
            currentCard.SetActive(false);
        }

        activeCards = (int)difficulty;
        int currentColumn;
        int currentRow = -1;

        float tableHeight = 0.5f;

        int rows = 2;
        if (difficulty == (int)MemoryDifficulty.Hard)
            rows = 3;

        float x = activeCards / rows * cardWidth;
        x += (activeCards / rows - 1.0f) * columnsOffset;
        x /= -2.0f;
        float z = rows * cardHeight;
        z += (rows - 1.0f) * rowsOffset;
        z /= -2.0f;

        cardsParent.position = new Vector3(x, tableHeight, z);
        cardsParentPosition = cardsParent.position;

        for (int i = 0; i < activeCards; i++)
        {
            GameObject currentCard = cardsParent.GetChild(i).gameObject;
            currentCard.SetActive(true);

            currentColumn = i % (activeCards / rows);
            if (currentColumn == 0) currentRow++;

            currentCard.transform.localPosition = new Vector3(currentColumn * (cardWidth + columnsOffset), 0.0f, currentRow * (cardHeight + rowsOffset));
            cardPositions[i] = currentCard.transform.localPosition;
        }
    }

    public void SetDifficulty(MemoryDifficulty newDifficulty)
    {
        if (!view.IsMine) return;

        difficulty = (int)newDifficulty;
    }

    public void SetDifficulty(int newDifficulty)
    {
        if (!view.IsMine) return;

        difficulty = newDifficulty;
    }

    public void AddPlayer(MemoryPlayer newPlayer, ref bool result)
    {
        if (!newPlayer)
        {
            result = false;
            return;
        }

        if (players.Count >= 2)
        {
            result = false;
            return;
        }

        if (players.Contains(newPlayer))
        {
            result = false;
            return;
        }

        players.Add(newPlayer);
        result = true;

        if (players.Count >= 2)
            FirstPlayer();
    }

    public void FirstPlayer()
    {
        int index = Random.Range(0.0f, 100.0f) > 50.0f ? 0 : 1;

        currentPlayer = players[index];
        currentPlayer.IsMyTurn = true;
        currentPlayer.MovesCounter = 2;
        GiveOwner();
    }

    public void UsePlayerMove()
    {
        currentPlayer.MovesCounter--;
        if (currentPlayer.MovesCounter <= 0)
            ChangePlayer();
    }

    public void ChangePlayer()
    {
        currentPlayer.IsMyTurn = false;
        RemoveOwner();

        if (currentPlayer == players[0])
            currentPlayer = players[1];
        else
            currentPlayer = players[0];

        currentPlayer.IsMyTurn = true;
        currentPlayer.MovesCounter = 2;
        GiveOwner();
    }

    public void GiveOwner()
    {
        for (int i = 0; i < cardsParent.childCount; i++)
            cardsParent.GetChild(i).GetComponent<PhotonView>().TransferOwnership(currentPlayer.PhotonPlayer);
    }

    public void RemoveOwner()
    {
        for (int i = 0; i < cardsParent.childCount; i++)
            cardsParent.GetChild(i).GetComponent<PhotonView>().RequestOwnership();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && view.IsMine)
        {
            stream.SendNext(difficulty);
            stream.SendNext(cardsParentPosition);
            stream.SendNext(activeCards);

            for (int i = 0; i < cardPositions.Length; i++)
                stream.SendNext(cardPositions[i]);
        }
        else
        {
            syncDifficulty = (int)stream.ReceiveNext();
            syncCardsParentPosition = (Vector3)stream.ReceiveNext();
            syncActiveCards = (int)stream.ReceiveNext();

            for (int i = 0; i < cardPositions.Length; i++)
                syncCardPositions[i] = (Vector3)stream.ReceiveNext();
        }
    }
}
