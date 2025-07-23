using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MenuManager : MonoBehaviourPunCallbacks
{
    public InputField room;
    public Text statusText;

    public void CreateRoom()
    {
        if (string.IsNullOrWhiteSpace(room.text))
        {
            statusText.text = "Enter the room name!";
            return;
        }

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(room.text, options, null);
    }

    public void JoinRoom()
    {
        if (string.IsNullOrWhiteSpace(room.text))
        {
            statusText.text = "Enter the room name!";
            return;
        }

        PhotonNetwork.JoinRoom(room.text);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel("SampleScene");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Couldn't join the room: " + message);
        statusText.text = "Join error: " + message;
    }
}
