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
    public GameObject CardPrefab;

    public MemoryDifficulty Difficulty = MemoryDifficulty.Medium;

    public float ColumnsOffset = 0.3f;
    public float RowsOffset = 0.3f;

    private Transform cardsParent;

    void Start()
    {
        cardsParent = transform.GetChild(0);

        InstantiateCards();
        PlaceCards();
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

    public void PlaceCards(float tableHeight = 0.5f, float cardWidth = 0.4f, float cardHeight = 0.6f)
    {
        int numberOfCards = (int)Difficulty;
        int currentColumn;
        int currentRow = -1;

        int rows = 2;
        if (Difficulty == MemoryDifficulty.Hard)
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
            currentCard.transform.GetChild(0).GetComponent<MemoryCard>().ResetCard();

            currentColumn = i % (numberOfCards / rows);
            if (currentColumn == 0) currentRow++;

            currentCard.transform.localPosition = new Vector3(currentColumn * (cardWidth + ColumnsOffset), 0.0f, currentRow * (cardHeight + RowsOffset));
        }
    }
}
