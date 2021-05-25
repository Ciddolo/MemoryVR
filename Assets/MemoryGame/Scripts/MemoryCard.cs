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
    private int visibility = 1;
    private int oldIndexMaterial;
    
    private void Start()
    {
        view = GetComponent<PhotonView>();
        gameManager = transform.parent.GetComponent<MemoryManager>();
    }

    private void Update()
    {
        if (indexMaterial != oldIndexMaterial)
            CurrentMeshRenderer.material = Materials[indexMaterial];

        if (visibility == 0)
            gameObject.SetActive(false);
        else if (visibility == 1)
            gameObject.SetActive(true);
    }

    public void ShowCard()
    {
        if (isShowing) return;
        if (wasFound) return;

        isShowing = true;
        indexMaterial = 1;
        CurrentMeshRenderer.material = Materials[indexMaterial];

        gameManager.UsePlayerMove();
    }

    public void HideCard()
    {
        if (!isShowing) return;
        if (wasFound) return;

        isShowing = false;
        indexMaterial = 0;
        CurrentMeshRenderer.material = Materials[indexMaterial];
    }

    public void SelectCard()
    {
        if (isShowing) return;
        if (wasFound) return;

        indexMaterial = 2;
        CurrentMeshRenderer.material = Materials[indexMaterial];
    }

    public void DeselectCard()
    {
        if (isShowing) return;
        if (wasFound) return;

        indexMaterial = 0;
        CurrentMeshRenderer.material = Materials[indexMaterial];
    }

    public void SetColor(Material newColor)
    {
        Materials[(int)ColorMaterial.Visible] = newColor;
    }

    public void ResetCard()
    {
        wasFound = false;
        isShowing = false;

        indexMaterial = 0;
        CurrentMeshRenderer.material = Materials[indexMaterial];
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && view.IsMine)
        {
            stream.SendNext(visibility);
            stream.SendNext(indexMaterial);
        }
        else
        {
            visibility = (int)stream.ReceiveNext();

            oldIndexMaterial = indexMaterial;
            indexMaterial = (int)stream.ReceiveNext();
        }
    }
}
