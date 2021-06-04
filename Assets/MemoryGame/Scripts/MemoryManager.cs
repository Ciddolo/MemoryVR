using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using BNG;

public enum MemoryDifficulty
{
    Easy = 6,
    Medium = 8,
    Hard = 12
}

public class MemoryManager : MonoBehaviourPun, IPunObservable
{
    public bool IsMyTurn { get { return (PhotonNetwork.IsMasterClient && currentPlayerIndex == 0) || (!PhotonNetwork.IsMasterClient && currentPlayerIndex == 1); } }

    private readonly float columnsOffset = 0.3f;
    private readonly float rowsOffset = 0.3f;
    private readonly float cardWidth = 0.4f;
    private readonly float cardHeight = 0.6f;

    public List<GameObject> RegisteredPlayers = new List<GameObject>();
    private bool arePlayersRegistered;

    private Transform cardsParent;
    private List<MemoryCard> showedCards = new List<MemoryCard>();

    public Text InfoUI;

    private const float SHOWTIME = 2.0f;
    private float showTimer;
    private bool showTime;

    public Text HostScoreUI;
    private int hostScore;
    public Text GuestScoreUI;
    private int guestScore;

    private int test = 0;

    [Header("Photon")]
    private PhotonView view;
    public PhotonView View { get { return view; } }

    private MemoryChangeOwner ownerChanger;
    public MemoryChangeOwner OwnerChanger { get { return ownerChanger; } }

    private int currentPlayerIndex;
    private int syncCurrentPlayerIndex;

    public int PlayerMoves;
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
        ownerChanger = GetComponent<MemoryChangeOwner>();

        hostScore = 0;
        guestScore = 0;
        PrintScores();

        showTimer = 3.0f;

        currentPlayerIndex = -1;

        PlayerMoves = 2;
        syncPlayerMoves = 2;

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
        RegisterPlayers();

        CheckOwnership();

        Showtime();

        SyncFromOwner();

        PrintInfo();

        if (InputBridge.Instance.RightGripDown || Input.GetKeyDown(KeyCode.O))
        {
            currentPlayerIndex = PhotonNetwork.IsMasterClient ? 0 : 1;
            RequestOwnership();
        }
    }

    private void RegisterPlayers()
    {
        if (arePlayersRegistered) return;

        GameObject[] memoryPLayers = GameObject.FindGameObjectsWithTag("MemoryPlayer");

        if (memoryPLayers.Length < 2) return;

        if (PhotonNetwork.IsMasterClient)
        {
            if (memoryPLayers[0].name == "MyRemotePlayer")
            {
                RegisteredPlayers.Add(memoryPLayers[0]);
                RegisteredPlayers.Add(memoryPLayers[1]);
            }
            else
            {
                RegisteredPlayers.Add(memoryPLayers[1]);
                RegisteredPlayers.Add(memoryPLayers[0]);
            }
        }
        else
        {
            if (memoryPLayers[0].name != "MyRemotePlayer")
            {
                RegisteredPlayers.Add(memoryPLayers[0]);
                RegisteredPlayers.Add(memoryPLayers[1]);
            }
            else
            {
                RegisteredPlayers.Add(memoryPLayers[1]);
                RegisteredPlayers.Add(memoryPLayers[0]);
            }
        }

        arePlayersRegistered = RegisteredPlayers.Count >= 2;
    }

    private void CheckOwnership()
    {
        if (!IsMyTurn || view.IsMine) return;

        RequestOwnership();
    }

    private void Showtime()
    {
        if (!showTime) return;

        showTimer -= Time.deltaTime;

        if (showTimer <= 0.0f)
        {
            showedCards[0].HideCard();
            showedCards[1].HideCard();

            showedCards.Clear();

            PlayerMoves = 2;

            showTimer = 3.0f;
            showTime = false;

            currentPlayerIndex = currentPlayerIndex == 0 ? 1 : 0;
        }
    }

    private void TransferOwnership()
    {
        int currentPlayerId = int.Parse(PhotonNetwork.PlayerListOthers[0].UserId);

        ownerChanger.TransferOwnership(currentPlayerId);

        for (int i = 0; i < activeCards; i++)
            cardsParent.GetChild(i).GetComponent<MemoryChangeOwner>().TransferOwnership(currentPlayerId);
    }

    private void RequestOwnership()
    {
        ownerChanger.RequestOwnership();

        for (int i = 0; i < activeCards; i++)
            cardsParent.GetChild(i).GetComponent<MemoryChangeOwner>().RequestOwnership();
    }

    private void SyncFromOwner()
    {
        if (view.IsMine) return;

        PlayerMoves = syncPlayerMoves;

        currentPlayerIndex = syncCurrentPlayerIndex;

        difficulty = syncDifficulty;
        SetDifficulty(difficulty);

        cardsParentPosition = syncCardsParentPosition;
        cardsParent.position = cardsParentPosition;

        if (PhotonNetwork.IsMasterClient) return;

        activeCards = syncActiveCards;
        for (int i = 0; i < cardsParent.childCount; i++)
            cardsParent.GetChild(i).gameObject.SetActive(i < activeCards);

        cardPositions = syncCardPositions;
        for (int i = 0; i < activeCards; i++)
            cardsParent.GetChild(i).localPosition = cardPositions[i];
    }

    private void PrintInfo()
    {
        string info = "";

        if (PhotonNetwork.IsMasterClient)
            info += "<color=white>I'M</color> <color=red>HOST</color>";
        else
            info += "<color=white>I'M</color> <color=cyan>GUEST</color>";

        if (currentPlayerIndex >= 0)
        {
            if (IsMyTurn)
                info += "\n<color=white>MY TURN</color>";
            else
                info += "\n<color=grey>OPPONENT TURN</color>";
        }

        info += "\n<color=white>REMAINING MOVES:</color> <color=green>" + PlayerMoves + "</color>";

        if (test == 0)
            info += "\nNONE";
        else if (test == 1)
            info += "\nWRITE";
        else if (test == 2)
            info += "\nREAD";

        info += "\nMANAGER OWNER: " + view.Owner;

        if (cardsParent.GetChild(0).gameObject.activeSelf)
            info += "\nCURRENT INDEX: " + currentPlayerIndex + " CARDS OWNER: " + cardsParent.GetChild(0).GetComponent<MemoryCard>().View.Owner;

        InfoUI.text = info;
    }

    private void PrintScores()
    {
        HostScoreUI.text = "<color=red>HOST</color><color=white>\n" + hostScore + "</color>";
        GuestScoreUI.text = "<color=cyan>GUEST</color><color=white>\n" + guestScore + "</color>";
    }

    public void PlaceCards()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        ownerChanger.RequestOwnership();

        hostScore = 0;
        guestScore = 0;
        PrintScores();

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

        for (int i = 0; i < activeCards; i++)
        {
            Vector3 temp = cardsParent.GetChild(i).localPosition;
            int randomIndex = Random.Range(i, activeCards);
            cardsParent.GetChild(i).localPosition = cardsParent.GetChild(randomIndex).localPosition;
            cardsParent.GetChild(randomIndex).localPosition = temp;
        }

        currentPlayerIndex = Random.Range(0.0f, 100.0f) > 50.0f ? 0 : 1;
    }

    public void SetDifficulty(MemoryDifficulty newDifficulty)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        difficulty = (int)newDifficulty;
    }

    public void SetDifficulty(int newDifficulty)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        difficulty = newDifficulty;
    }

    public void UsePlayerMove(MemoryCard showedCard)
    {
        showedCards.Add(showedCard);

        if (--PlayerMoves > 0) return;

        if (showedCards[0].Code == showedCards[1].Code)
        {
            showedCards[0].WasFound = true;
            showedCards[1].WasFound = true;

            showedCards.Clear();

            PlayerMoves = 2;
            if (currentPlayerIndex == 0)
                hostScore++;
            else
                guestScore++;

            PrintScores();
        }
        else
            showTime = true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && view.IsMine)
        {
            test = 1;
            stream.SendNext(PlayerMoves);
            stream.SendNext(currentPlayerIndex);
            stream.SendNext(difficulty);
            stream.SendNext(cardsParentPosition);
            stream.SendNext(activeCards);

            for (int i = 0; i < cardPositions.Length; i++)
                stream.SendNext(cardPositions[i]);
        }
        else
        {
            test = 2;
            syncPlayerMoves = (int)stream.ReceiveNext();
            syncCurrentPlayerIndex = (int)stream.ReceiveNext();
            syncDifficulty = (int)stream.ReceiveNext();
            syncCardsParentPosition = (Vector3)stream.ReceiveNext();
            syncActiveCards = (int)stream.ReceiveNext();

            for (int i = 0; i < cardPositions.Length; i++)
                syncCardPositions[i] = (Vector3)stream.ReceiveNext();
        }
    }
}
