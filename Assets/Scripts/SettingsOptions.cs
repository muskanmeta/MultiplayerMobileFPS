using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class SettingsOptions : MonoBehaviourPunCallbacks
{
    public GameObject SettingsPanel;
    public GameObject MainPanel;

    private void Start()
    {
        ActivatePanel("MainPanel");
    }


    public void ActivatePanel(string panel)
    {
        SettingsPanel.SetActive(panel.Equals(SettingsPanel.name));
    }

   public void LeaveGame()
    {
        
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.IsConnectedAndReady)
            {
                PhotonNetwork.LeaveRoom();
                PhotonNetwork.Disconnect();
                PhotonNetwork.ConnectUsingSettings();
                PhotonNetwork.LoadLevel("LobbyScene");
            }
        }
 
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
    }

  

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connect to MasterServer");
        base.OnConnectedToMaster();
    }
}
