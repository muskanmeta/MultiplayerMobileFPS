using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Login UI Panel")]
    public InputField playerNameInput;
    public GameObject LoginUIPanel;

    [Header("GameOptions UI Panel")]
    public GameObject GameOptionsUIPanel;

    [Header("CreateRoom UI Panel")]
    public GameObject CreateRoomUIPanel;
    public InputField RoomName;

    public InputField maxPlayerInputField;

    [Header("RoomList UI Panel")]
    public GameObject RoomListUIPanel;
    public GameObject RoomListEntryPrefab;
    public GameObject RoomListParentGameObject;

    [Header("JoinRandomRoom UI Panel")]
    public GameObject JoinRandomRoomUIPanel;

    [Header("InsideRoom UI Panel")]
    public GameObject InsideRoomUIPanel;
    public GameObject PlayerListEntryPrefab;
    public GameObject PlayerListParent;
    public Text roomInfoText;
    public GameObject StartGameButton;

    [Header("Connection Status Panel")]
    public Text connectionStatus;

    private Dictionary<string, RoomInfo> cachedRoomList;
    private Dictionary<string, GameObject> RoomListGameObjects;
    private Dictionary<int, GameObject> PlayerListGameObjects;

    public TMPro.TextMeshProUGUI emptyField;

    #region Unity Methods


    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        ActivatePanel(LoginUIPanel.name);
        emptyField.text = "";
        cachedRoomList = new Dictionary<string, RoomInfo>();
        RoomListGameObjects = new Dictionary<string, GameObject>();
    }


    #endregion

  

    #region UI Callbacks


    public void OnLoginButtonClicked()
    {
        string playerName = playerNameInput.text;
        if (!string.IsNullOrEmpty(playerName) && !string.IsNullOrWhiteSpace(playerName))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
            connectionStatus.text = "   Connection Status: Connecting...";
            emptyField.text = "";
        }
        else if (string.IsNullOrEmpty(playerName))
        {
            emptyField.text = "*Player Name invalid.";
        }
    }
    public void OnCreateRoomButtonClicked()
    {
        string roomName = RoomName.text;
        //if its empty, we will give default roomname with number;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Room " + Random.Range(1000, 10000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte) int.Parse(maxPlayerInputField.text);

        PhotonNetwork.CreateRoom(roomName,roomOptions);
    }
    public void OnCancelButtonClicked()
    {
        ActivatePanel(GameOptionsUIPanel.name);
    }
    
    public void OnRoomListButtonClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        ActivatePanel(RoomListUIPanel.name);
    }

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivatePanel(GameOptionsUIPanel.name);
    }

    public void OnLeaveRoomButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnJoinRandomRoomButtonClicked()
    {
        ActivatePanel(JoinRandomRoomUIPanel.name);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnStartButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    #endregion

    #region Photon Callback Methods


    public override void OnConnected()
    {
        Debug.Log("Connected to internet.");
    }
    public override void OnConnectedToMaster()
    {
        connectionStatus.text = "   Connected";
        connectionStatus.color = new Color32(28, 255, 40, 255);
        ActivatePanel(GameOptionsUIPanel.name);
    }
    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + " -Room is created.");
    }
    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.NickName+" has joined "+ PhotonNetwork.CurrentRoom.Name + " -Room.");
        ActivatePanel(InsideRoomUIPanel.name);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            StartGameButton.SetActive(true);
        }
        else
        {
            StartGameButton.SetActive(false);
        }

        roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + "    " +
                            PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                            PhotonNetwork.CurrentRoom.MaxPlayers;

        if (PlayerListGameObjects==null)
        {
            PlayerListGameObjects = new Dictionary<int, GameObject>();
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerListGameObject = Instantiate(PlayerListEntryPrefab);
            playerListGameObject.transform.SetParent(PlayerListParent.transform);
            playerListGameObject.transform.localScale = Vector3.one;

            playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = player.NickName;
            if (player.IsLocal)
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
            }
            else
            {
                playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
            }

            PlayerListGameObjects.Add(player.ActorNumber, playerListGameObject);
        }
    }
    //clearing the roomlist if the player leave room/back button is clicked/cancel button is clicked etc.
    public override void OnLeftLobby()
    {
        //CLearing the list and delete the Roomlist GameObjects if the localplayer leave the room
        ClearRoomList(); //user-defined private function
        cachedRoomList.Clear();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //updating Room Info in the room
        roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + "    " +
                           PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                           PhotonNetwork.CurrentRoom.MaxPlayers;

        GameObject playerListGameObject = Instantiate(PlayerListEntryPrefab);
        playerListGameObject.transform.SetParent(PlayerListParent.transform);
        playerListGameObject.transform.localScale = Vector3.one;

        playerListGameObject.transform.Find("PlayerNameText").GetComponent<Text>().text = newPlayer.NickName;
        if (newPlayer.IsLocal)
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(true);
        }
        else
        {
            playerListGameObject.transform.Find("PlayerIndicator").gameObject.SetActive(false);
        }

        PlayerListGameObjects.Add(newPlayer.ActorNumber, playerListGameObject);

    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //updating Room Info in the room
        roomInfoText.text = "Room Name: " + PhotonNetwork.CurrentRoom.Name + "    " +
                              PhotonNetwork.CurrentRoom.PlayerCount + "/" +
                           PhotonNetwork.CurrentRoom.MaxPlayers;

        Destroy(obj: PlayerListGameObjects[otherPlayer.ActorNumber].gameObject);
        PlayerListGameObjects.Remove(otherPlayer.ActorNumber);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            StartGameButton.SetActive(true);
        }
    }
    public override void OnLeftRoom()
    {
        ActivatePanel(GameOptionsUIPanel.name);
        //CLearing the list and delete the playerlist GameObjects if the localplayer leave the room
        foreach(GameObject go in PlayerListGameObjects.Values)
        {
            Destroy(go);
        }
        PlayerListGameObjects.Clear();
        PlayerListGameObjects = null;
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        ClearRoomList(); //private function

        foreach(RoomInfo rf in roomList)
        {
            Debug.Log(rf.Name);
            if (!rf.IsOpen|| !rf.IsVisible|| rf.RemovedFromList)
            {
                if (cachedRoomList.ContainsKey(rf.Name))
                {
                    cachedRoomList.Remove(rf.Name);
                }
            }
            else
            {
                if (cachedRoomList.ContainsKey(rf.Name))
                {
                    cachedRoomList[rf.Name] = rf;
                }
                else
                {
                     cachedRoomList.Add(rf.Name, rf);    
                }
            }
        }

        foreach (RoomInfo room in cachedRoomList.Values)
        {
            GameObject roomListEntryGameObject = Instantiate(RoomListEntryPrefab);
            roomListEntryGameObject.transform.SetParent(RoomListParentGameObject.transform);

            roomListEntryGameObject.transform.Find("RoomNameText").GetComponent<Text>().text = room.Name;
            roomListEntryGameObject.transform.Find("RoomPlayersText").GetComponent<Text>().text = room.PlayerCount + " / " + room.MaxPlayers;
            roomListEntryGameObject.transform.Find("JoinRoomButton").GetComponent<Button>().onClick.AddListener(() => OnJoinRoomButtonClicked(room.Name));

            RoomListGameObjects.Add(room.Name, roomListEntryGameObject);
        }

    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log(message);
        string roomName = "Room: "+Random.Range(100, 10000);

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 10;
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }
    #endregion

    #region Private Methods

    void OnJoinRoomButtonClicked(string roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(roomName);

    }
    void ClearRoomList()
    {
        foreach (var roomGO in RoomListGameObjects.Values)
        {
            Destroy(roomGO);
        }
        RoomListGameObjects.Clear();
    }
    
    #endregion

    #region Public Methods


    public void ActivatePanel(string panel)
    {
        LoginUIPanel.SetActive(panel.Equals(LoginUIPanel.name));
        GameOptionsUIPanel.SetActive(panel.Equals(GameOptionsUIPanel.name));
        JoinRandomRoomUIPanel.SetActive(panel.Equals(JoinRandomRoomUIPanel.name));
        CreateRoomUIPanel.SetActive(panel.Equals(CreateRoomUIPanel.name));
        RoomListUIPanel.SetActive(panel.Equals(RoomListUIPanel.name));
        InsideRoomUIPanel.SetActive(panel.Equals(InsideRoomUIPanel.name));
    }


    #endregion
}
