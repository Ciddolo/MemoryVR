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

public class MemoryManager : MonoBehaviour
{
    public MemoryDifficulty GetDifficulty { get { return difficulty; } }

    public GameObject CardPrefab;

    public float ColumnsOffset = 0.3f;
    public float RowsOffset = 0.3f;

    private MemoryDifficulty difficulty = MemoryDifficulty.Medium;

    private Transform cardsParent;

    private PhotonView view;

    private List<MemoryPlayer> players;
    private MemoryPlayer currentPlayer;

    void Awake()
    {
        view = GetComponent<PhotonView>();

        cardsParent = transform.GetChild(0);

        players = new List<MemoryPlayer>();
    }

    public void InstantiateCards()
    {
        for (int i = 0; i < (int)MemoryDifficulty.Hard; i++)
        {
            GameObject currentCard = PhotonNetwork.Instantiate("MemoryCard", Vector3.zero, Quaternion.identity);
            currentCard.transform.SetParent(cardsParent);
            currentCard.SetActive(false);
        }
    }

    public void PlaceCards()
    {
        for (int i = 0; i < cardsParent.childCount; i++)
        {
            GameObject currentCard = cardsParent.GetChild(i).gameObject;
            currentCard.GetComponent<MemoryCard>().ResetCard();
            currentCard.SetActive(false);
        }

        int numberOfCards = (int)difficulty;
        int currentColumn;
        int currentRow = -1;

        float cardWidth = 0.4f;
        float cardHeight = 0.6f;

        float tableHeight = 0.5f;

        int rows = 2;
        if (difficulty == MemoryDifficulty.Hard)
            rows = 3;

        float x = numberOfCards / rows * cardWidth;
        x += (numberOfCards / rows - 1.0f) * ColumnsOffset;
        x /= -2.0f;
        float z = rows * cardHeight;
        z += (rows - 1.0f) * RowsOffset;
        z /= -2.0f;

        cardsParent.position = new Vector3(x, tableHeight, z);

        for (int i = 0; i < numberOfCards; i++)
        {
            GameObject currentCard = cardsParent.GetChild(i).gameObject;
            currentCard.SetActive(true);

            currentColumn = i % (numberOfCards / rows);
            if (currentColumn == 0) currentRow++;

            currentCard.transform.localPosition = new Vector3(currentColumn * (cardWidth + ColumnsOffset), 0.0f, currentRow * (cardHeight + RowsOffset));
        }
    }

    public void SetDifficulty(MemoryDifficulty newDifficulty)
    {
        difficulty = newDifficulty;
    }

    public void SetDifficulty(int newDifficulty)
    {
        difficulty = (MemoryDifficulty)newDifficulty;
    }

    public void SetDifficultyEasy()
    {
        difficulty = MemoryDifficulty.Easy;
    }

    public void SetDifficultyMedium()
    {
        difficulty = MemoryDifficulty.Medium;
    }

    public void SetDifficultyHard()
    {
        difficulty = MemoryDifficulty.Hard;
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
}
