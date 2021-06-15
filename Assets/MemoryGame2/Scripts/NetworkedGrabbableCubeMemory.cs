using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;


public class MemoryNetworkedGrabbable : Grabbable, IPunObservable
{
    PhotonView view;
    Rigidbody rb;

    // Used to Lerp our position when we are not the owner
    private Vector3 _syncStartPosition = Vector3.zero;
    private Vector3 _syncEndPosition = Vector3.zero;
    private Quaternion _syncStartRotation = Quaternion.identity;
    private Quaternion _syncEndRotation = Quaternion.identity;
    private bool _syncBeingHeld = false;

    // Interpolation values
    private float _lastSynchronizationTime = 0f;
    private float _syncDelay = 0f;
    private float _syncTime = 0f;

    void Awake()
    {
        view = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
    }

    public override void Update()
    {
        base.Update();

        Sync();
    }

    private void Sync()
    {
        if (!view.IsMine && view.Owner != null && _syncEndPosition != null && _syncEndPosition != Vector3.zero)
        {
            rb.isKinematic = true;

            _syncTime += Time.deltaTime;
            float syncValue = _syncTime / _syncDelay;
            float dist = Vector3.Distance(_syncStartPosition, _syncEndPosition);

            if (dist > 3f)
            {
                transform.position = _syncEndPosition;
                transform.rotation = _syncEndRotation;
            }
            else
            {
                transform.position = Vector3.Lerp(_syncStartPosition, _syncEndPosition, syncValue);
                transform.rotation = Quaternion.Lerp(_syncStartRotation, _syncEndRotation, syncValue);
            }

            BeingHeld = _syncBeingHeld;
        }
        else if (view.IsMine)
        {
            if (rb)
            {
                rb.isKinematic = wasKinematic;
            }

            BeingHeld = heldByGrabbers != null && heldByGrabbers.Count > 0;
        }
    }

    public override bool IsGrabbable()
    {
        if (!base.IsGrabbable()) return false;

        if (!view) return true;

        if (view.IsMine) return true;

        if (!PhotonNetwork.IsConnected) return true;

        return false;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && view.IsMine)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
            stream.SendNext(BeingHeld);
        }
        else
        {
            // Position
            _syncStartPosition = transform.position;
            _syncEndPosition = (Vector3)stream.ReceiveNext();

            // Rotation
            _syncStartRotation = transform.rotation;
            _syncEndRotation = (Quaternion)stream.ReceiveNext();

            // Status
            _syncBeingHeld = (bool)stream.ReceiveNext();

            _syncTime = 0f;
            _syncDelay = Time.time - _lastSynchronizationTime;
            _lastSynchronizationTime = Time.time;
        }
    }
}
