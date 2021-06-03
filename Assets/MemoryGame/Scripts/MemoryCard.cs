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
    public MeshRenderer CurrentMeshRenderer;

    public int Code;

    public Material[] Materials = new Material[3];

    public bool IsShowing;
    public bool WasFound;

    public MemoryManager GameManager;

    public PhotonView View;

    private Vector3 syncLocalPosition;

    private int indexMaterial;
    private int oldIndexMaterial;

    private void Start()
    {
        View = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            transform.localPosition = syncLocalPosition;

        if (indexMaterial != oldIndexMaterial)
        {
            oldIndexMaterial = indexMaterial;
            CurrentMeshRenderer.material = Materials[indexMaterial];
        }
    }

    public MemoryCard ShowCard()
    {
        if (IsShowing) return null;
        if (WasFound) return null;

        IsShowing = true;
        indexMaterial = 1;

        return this;
    }

    public void HideCard()
    {
        if (!IsShowing) return;
        if (WasFound) return;

        IsShowing = false;
        indexMaterial = (int)ColorMaterial.Default;
    }

    public void SelectCard()
    {
        if (IsShowing) return;
        if (WasFound) return;

        indexMaterial = (int)ColorMaterial.Selected;
    }

    public void DeselectCard()
    {
        if (IsShowing) return;
        if (WasFound) return;

        indexMaterial = (int)ColorMaterial.Default;
    }

    public void ResetCard()
    {
        WasFound = false;
        IsShowing = false;

        indexMaterial = (int)ColorMaterial.Default;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            if (PhotonNetwork.IsMasterClient)
                stream.SendNext(transform.localPosition);

            if (GameManager.IsMyTurn())
                stream.SendNext(indexMaterial);
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient)
                syncLocalPosition = (Vector3)stream.ReceiveNext();

            if (!GameManager.IsMyTurn())
            {
                oldIndexMaterial = indexMaterial;
                indexMaterial = (int)stream.ReceiveNext();
            }
        }
    }
}
