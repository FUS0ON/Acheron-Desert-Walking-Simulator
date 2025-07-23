using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement; 

public class ConnectToServer : MonoBehaviourPunCallbacks
{

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon server");
        SceneManager.LoadScene("Menu");
    }
    void Update()
    {
        
    }
}
