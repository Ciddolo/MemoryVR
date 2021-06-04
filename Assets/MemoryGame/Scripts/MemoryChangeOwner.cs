using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MemoryChangeOwner : MonoBehaviourPun, IPunOwnershipCallbacks
{
    public bool ChangeOwner { get { return changeOwner; } }
    private bool changeOwner;

    private void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        changeOwner = false;
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
    }

    public void RequestOwnership()
    {
        changeOwner = true;

        base.photonView.RequestOwnership();
    }

    public void TransferOwnership(int newOwnerId)
    {
        changeOwner = true;
        
        base.photonView.TransferOwnership(newOwnerId);
    }
}
