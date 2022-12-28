using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    GameObject PlayerPrefab;

    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (PlayerPrefab!=null)
            {
                int randomPointZ = Random.Range(-14, 0);
                float randomPointX = Random.Range(-3.5f, 0f);
                PhotonNetwork.Instantiate(PlayerPrefab.name, new Vector3(randomPointX, 0, randomPointZ), Quaternion.identity);

            }
            else
            {
                Debug.Log("Place Player Prefab!");
            }
        } 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
