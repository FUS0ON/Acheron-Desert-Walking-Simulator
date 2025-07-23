// PlayerSpawner.cs
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            Vector3 spawnPos = new Vector3(5, 1, 12);
            PhotonNetwork.Instantiate("Player", spawnPos, Quaternion.identity);
        }
    }
}
