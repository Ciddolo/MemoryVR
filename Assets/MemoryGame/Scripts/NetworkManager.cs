using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        Debug.Log("START");
        OnConnectedToServer();
    }

    void OnConnectedToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("TRY TO CONNECT TO SERVER");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("CONNECTED TO SERVER");
        base.OnConnectedToMaster();

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        PhotonNetwork.JoinOrCreateRoom("Room0", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("JOINED ROOM");
        base.OnJoinedRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("NEW PLAYER JOINED THE ROOM");
        base.OnPlayerEnteredRoom(newPlayer);
    }
}
