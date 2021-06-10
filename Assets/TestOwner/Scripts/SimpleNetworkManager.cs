using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SimpleNetworkManager : MonoBehaviourPunCallbacks
{
    [Tooltip("Maximum number of players per room. If the room is full, a new random one will be created. 0 = No Max.")]
    [SerializeField]
    private byte maxPlayersPerRoom = 0;

    [Tooltip("If true, the JoinRoomName will try to be Joined On Start. If false, need to call JoinRoom yourself.")]
    public bool JoinRoomOnStart = true;

    [Tooltip("If true, do not destroy this object when moving to another scene")]
    public bool dontDestroyOnLoad = true;

    public string JoinRoomName = "RandomRoom";

    [Tooltip("Game Version can be used to separate rooms.")]
    public string GameVersion = "1";

    void Awake()
    {
        // Required if you want to call PhotonNetwork.LoadLevel() 
        PhotonNetwork.AutomaticallySyncScene = true;

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void Start()
    {
        // Connect to Random Room if Connected to Photon Server
        if (PhotonNetwork.IsConnected)
        {
            if (JoinRoomOnStart)
            {
                LogText("<color=white>Joining Room : " + JoinRoomName + "</color>");
                PhotonNetwork.JoinRoom(JoinRoomName);
            }
        }
        // Otherwise establish a new connection. We can then connect via OnConnectedToMaster
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = GameVersion;
        }
    }

    void Update()
    {
        // Show Loading Progress
        if (PhotonNetwork.LevelLoadingProgress > 0 && PhotonNetwork.LevelLoadingProgress < 1)
        {
            Debug.Log(PhotonNetwork.LevelLoadingProgress);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        LogText("<color=white>Room does not exist. Creating </color><color=yellow>" + JoinRoomName + "</color>");
        PhotonNetwork.CreateRoom(JoinRoomName, new RoomOptions { MaxPlayers = maxPlayersPerRoom }, TypedLobby.Default);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("<color=red>OnJoinRandomFailed Failed, Error : " + message + "</color>");
    }

    public override void OnConnectedToMaster()
    {

        LogText("<color=white>Connected to Master Server. \n</color>");

        if (JoinRoomOnStart)
        {
            LogText("<color=white>Joining Room : </color><color=aqua>" + JoinRoomName + "</color>");
            PhotonNetwork.JoinRoom(JoinRoomName);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

        float playerCount = PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom != null ? PhotonNetwork.CurrentRoom.PlayerCount : 0;

        LogText("<color=white>Connected players : " + playerCount + "</color>");
    }

    public override void OnJoinedRoom()
    {
        LogText("<color=white>Joined Room. Creating Remote Player Representation.</color>");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        LogText("<color=red>Disconnected from PUN due to cause : " + cause + "</color>");

        if (!PhotonNetwork.ReconnectAndRejoin())
        {
            LogText("<color=white>Reconnect and Joined.</color>");
        }

        base.OnDisconnected(cause);
    }

    public void LoadScene(string sceneName)
    {
        // Fade Screen out
        StartCoroutine(doLoadLevelWithFade(sceneName));
    }

    IEnumerator doLoadLevelWithFade(string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);

        yield return null;
    }

    void LogText(string message)
    {
        Debug.Log(message);
    }
}
