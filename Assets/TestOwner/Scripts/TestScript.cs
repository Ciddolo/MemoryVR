using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TestScript : MonoBehaviour, IPunObservable
{
    public int Changer = -1;

    public Material MaterialDefault;
    public Material MaterialPressed;

    public PhotonView View;

    void Update()
    {
        if (Input.GetKey(KeyCode.Space) && View.IsMine)
            Changer = 1;
        else
            Changer = 0;

        if (Changer == 0)
            GetComponent<MeshRenderer>().material = MaterialDefault;
        else
            GetComponent<MeshRenderer>().material = MaterialPressed;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting && View.IsMine)
        {
            stream.SendNext(Changer);
        }
        else
        {
            Changer = (int)stream.ReceiveNext();
        }
    }
}
