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

public class MemoryCard : MonoBehaviourPun, IPunObservable
{
    public MeshRenderer CurrentMeshRenderer;

    public int Code;

    public Material[] Materials = new Material[3];

    private int isShowing;
    private int syncIsShowing;

    private int wasFound;
    private int syncWasFound;

    public MemoryManager GameManager;

    private PhotonView view;

    private Vector3 syncLocalPosition;

    private int indexMaterial;
    private int oldIndexMaterial;

    private void Awake()
    {
        view = GetComponent<PhotonView>();

        gameObject.SetActive(false);
    }

    private void Update()
    {
        Sync();
    }

    private void Sync()
    {
        if (indexMaterial != oldIndexMaterial)
        {
            oldIndexMaterial = indexMaterial;
            CurrentMeshRenderer.material = Materials[indexMaterial];
        }

        if (isShowing == 1 || wasFound == 1)
        {
            indexMaterial = (int)ColorMaterial.Visible;
            oldIndexMaterial = indexMaterial;
            CurrentMeshRenderer.material = Materials[indexMaterial];
        }

        if (!PhotonNetwork.IsMasterClient)
            transform.localPosition = syncLocalPosition;
    }

    public MemoryCard ShowCard()
    {
        if (!view.IsMine) return null;

        indexMaterial = (int)ColorMaterial.Visible;

        if (wasFound == 1 || isShowing == 1) return null;

        isShowing = 1;
        indexMaterial = 1;

        return this;
    }

    public void HideCard()
    {
        if (!view.IsMine) return;

        if (wasFound == 1)
            indexMaterial = (int)ColorMaterial.Visible;
        else
        {
            isShowing = 0;

            indexMaterial = (int)ColorMaterial.Default;
        }
    }

    public void SelectCard()
    {
        if (!view.IsMine) return;

        if (wasFound == 1 || isShowing == 1)
            indexMaterial = (int)ColorMaterial.Visible;
        else
            indexMaterial = (int)ColorMaterial.Selected;
    }

    public void DeselectCard()
    {
        if (!view.IsMine) return;

        if (wasFound == 1 || isShowing == 1)
            indexMaterial = (int)ColorMaterial.Visible;
        else
            indexMaterial = (int)ColorMaterial.Default;
    }

    public void ResetCard()
    {
        if (!view.IsMine) return;

        wasFound = 0;
        isShowing = 0;

        indexMaterial = (int)ColorMaterial.Default;
    }

    public void SetFound()
    {
        if (!view.IsMine) return;

        isShowing = 1;
        wasFound = 1;

        indexMaterial = (int)ColorMaterial.Visible;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && view.IsMine)
        {
            stream.SendNext(indexMaterial);
            stream.SendNext(isShowing);
            stream.SendNext(wasFound);

            if (PhotonNetwork.IsMasterClient)
                stream.SendNext(transform.localPosition);
        }
        else
        {
            oldIndexMaterial = indexMaterial;
            indexMaterial = (int)stream.ReceiveNext();

            isShowing = (int)stream.ReceiveNext();
            wasFound = (int)stream.ReceiveNext();

            if (!PhotonNetwork.IsMasterClient)
                syncLocalPosition = (Vector3)stream.ReceiveNext();
        }
    }
}
