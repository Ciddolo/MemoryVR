using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum ColorMaterial
{
    Default,
    Visible,
    Selected
}

public class MemoryCard : MonoBehaviour, IPunObservable
{
    public bool IsShowing { get { return isShowing; } }
    public bool WasFound { get { return wasFound; } }

    public MeshRenderer CurrentMeshRenderer;

    public List<Material> Materials;

    private bool isShowing;
    private bool wasFound;

    private MemoryManager gameManager;

    private PhotonView view;
    private int indexMaterial;
    private int oldIndexMaterial;

    private void Start()
    {
        view = GetComponent<PhotonView>();
        gameManager = transform.parent.GetComponent<MemoryManager>();
    }

    private void Update()
    {
        if (indexMaterial != oldIndexMaterial)
        {
            oldIndexMaterial = indexMaterial;
            CurrentMeshRenderer.material = Materials[indexMaterial];
        }
    }

    public void ShowCard()
    {
        if (isShowing) return;
        if (wasFound) return;

        isShowing = true;
        indexMaterial = 1;

        gameManager.UsePlayerMove();
    }

    public void HideCard()
    {
        if (!isShowing) return;
        if (wasFound) return;

        isShowing = false;
        indexMaterial = (int)ColorMaterial.Default;
    }

    public void SelectCard()
    {
        if (isShowing) return;
        if (wasFound) return;

        indexMaterial = (int)ColorMaterial.Selected;
    }

    public void DeselectCard()
    {
        if (isShowing) return;
        if (wasFound) return;

        indexMaterial = (int)ColorMaterial.Default;
    }

    public void SetColor(Material newColor)
    {
        Materials[(int)ColorMaterial.Visible] = newColor;
    }

    public void ResetCard()
    {
        wasFound = false;
        isShowing = false;

        indexMaterial = (int)ColorMaterial.Default;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(indexMaterial);
        }
        else
        {
            oldIndexMaterial = indexMaterial;
            indexMaterial = (int)stream.ReceiveNext();
        }
    }
}
