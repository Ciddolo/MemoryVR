using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Start()
    {
        cardsParent = transform.GetChild(0);

        InstantiateCards();
    }

    private void InstantiateCards()
    {
        for (int i = 0; i < (int)MemoryDifficulty.Hard; i++)
        {
            GameObject currentCard = Instantiate(CardPrefab);
            currentCard.transform.SetParent(cardsParent);
            currentCard.SetActive(false);
        }
    }

    public void SetDifficulty(int newDifficulty)
    {
        difficulty = (MemoryDifficulty)newDifficulty;
    }

    public void SetDifficulty(MemoryDifficulty newDifficulty)
    {
        difficulty = newDifficulty;
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
}
