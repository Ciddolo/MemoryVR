using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryCard : MonoBehaviour
{
    public Material DefaultMaterial;
    public Material ColorMaterial;
    public Material SelectionMaterial;

    public bool IsShowing { get { return isShowing; } }
    public bool WasFound { get { return wasFound; } }

    private MeshRenderer meshRenderer;
    private bool isShowing;
    private bool wasFound;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void ShowCard()
    {
        if (isShowing) return;
        if (wasFound) return;

        isShowing = true;
        meshRenderer.material = ColorMaterial;
    }

    public void HideCard()
    {
        if (!isShowing) return;
        if (wasFound) return;

        isShowing = false;
        meshRenderer.material = DefaultMaterial;
    }

    public void SelectCard()
    {
        if (isShowing) return;
        if (wasFound) return;

        meshRenderer.material = SelectionMaterial;
    }

    public void DeselectCard()
    {
        if (isShowing) return;
        if (wasFound) return;

        meshRenderer.material = DefaultMaterial;
    }

    public void SetColor(Material newColor)
    {
        ColorMaterial = newColor;
    }

    public void ResetCard()
    {
        wasFound = false;
        isShowing = false;
        HideCard();
    }
}
