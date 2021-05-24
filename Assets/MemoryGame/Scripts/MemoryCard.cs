using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryCard : MonoBehaviour
{
    public bool IsShowing { get { return isShowing; } }
    public bool WasFound { get { return wasFound; } }

    public MeshRenderer CurrentMeshRenderer;

    public Material DefaultMaterial;
    public Material ColorMaterial;
    public Material SelectionMaterial;

    private bool isShowing;
    private bool wasFound;

    public void ShowCard()
    {
        if (isShowing) return;
        if (wasFound) return;

        isShowing = true;
        CurrentMeshRenderer.material = ColorMaterial;
    }

    public void HideCard()
    {
        if (!isShowing) return;
        if (wasFound) return;

        isShowing = false;
        CurrentMeshRenderer.material = DefaultMaterial;
    }

    public void SelectCard()
    {
        if (isShowing) return;
        if (wasFound) return;

        CurrentMeshRenderer.material = SelectionMaterial;
    }

    public void DeselectCard()
    {
        if (isShowing) return;
        if (wasFound) return;

        CurrentMeshRenderer.material = DefaultMaterial;
    }

    public void SetColor(Material newColor)
    {
        ColorMaterial = newColor;
    }

    public void ResetCard()
    {
        wasFound = false;
        isShowing = false;

        CurrentMeshRenderer.material = DefaultMaterial;
    }
}
