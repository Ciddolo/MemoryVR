using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

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

    public List<MemoryPlayer> Players = new List<MemoryPlayer>();

    private Transform cardsParent;

    public Text DebugUI;

    [Header("Photon")]
    private PhotonView view;

    private int currentPlayerIndex;
    private int syncCurrentPlayerIndex;

    private int playerMoves;
    private int syncPlayerMoves;

    private int difficulty;
    private int syncDifficulty;

    private Vector3 cardsParentPosition;
    private Vector3 syncCardsParentPosition;

    private int activeCards;
    private int syncActiveCards;

    private Vector3[] cardPositions;
    private Vector3[] syncCardPositions;

    void Awake()
    {
        cardsParent = transform.GetChild(0);
        view = GetComponent<PhotonView>();

        currentPlayerIndex = -1;
        playerMoves = 2;

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
            currentPlayerIndex = syncCurrentPlayerIndex;

            playerMoves = syncPlayerMoves;

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

        //if (PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[currentPlayerIndex])
        //{
        //    Debug.Log("TRANSFER");
        //    TransferCardsOwner();
        //}

        PrintDebug();
    }

    private void PrintDebug()
    {
        string debug = "";

        debug += "\nI'M HOST [" + view.IsMine + "]";
        debug += "\nCURRENT PLAYER INDEX [" + currentPlayerIndex + "]";
        debug += "\nPLAYER MOVES [" + playerMoves + "]";
        debug += "\nDIFFICULTY [" + difficulty + "]";
        debug += "\nACTIVE CARDS [" + activeCards + "]";

        debug += "\nCARDS OWNER [";
        if (cardsParent.GetChild(0).GetComponent<PhotonView>().Owner != null)
            debug += cardsParent.GetChild(0).GetComponent<PhotonView>().Owner.ToString();
        debug += "]";

        debug += "\nCURRENT PLAYER [ ";
        if (currentPlayerIndex >= 0)
            debug += PhotonNetwork.PlayerList[currentPlayerIndex];
        debug += "]";

        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
            debug += "\nPLAYER NUMBER [" + PhotonNetwork.PlayerList[i].ToString() + "]";

        DebugUI.text = debug;
    }

    public void PlaceCards()
    {
        if (!view.IsMine) return;

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

        FirstPlayer();
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

    public void FirstPlayer()
    {
        currentPlayerIndex = Random.Range(0.0f, 100.0f) > 50.0f ? 0 : 1;

        Players[currentPlayerIndex].RequestOwnership(cardsParent, activeCards);
    }

    public void UsePlayerMove()
    {
        playerMoves--;
        if (playerMoves <= 0)
            ChangePlayer();
    }

    public void ChangePlayer()
    {
        playerMoves = 2;

        currentPlayerIndex = currentPlayerIndex == 0 ? 1 : 0;

        Players[currentPlayerIndex].RequestOwnership(cardsParent, activeCards);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && view.IsMine)
        {
            stream.SendNext(currentPlayerIndex);
            stream.SendNext(playerMoves);
            stream.SendNext(difficulty);
            stream.SendNext(cardsParentPosition);
            stream.SendNext(activeCards);

            for (int i = 0; i < cardPositions.Length; i++)
                stream.SendNext(cardPositions[i]);
        }
        else
        {
            syncCurrentPlayerIndex = (int)stream.ReceiveNext();
            syncPlayerMoves = (int)stream.ReceiveNext();
            syncDifficulty = (int)stream.ReceiveNext();
            syncCardsParentPosition = (Vector3)stream.ReceiveNext();
            syncActiveCards = (int)stream.ReceiveNext();

            for (int i = 0; i < cardPositions.Length; i++)
                syncCardPositions[i] = (Vector3)stream.ReceiveNext();
        }
    }
}
