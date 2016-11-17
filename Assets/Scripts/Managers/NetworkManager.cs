using UnityEngine;
using System.Collections;

public class NetworkManager : Photon.MonoBehaviour {

    [SerializeField]
    public GameObject playerPrefab;

	// Use this for initialization
	void Start () {
        PhotonNetwork.ConnectUsingSettings("0.1a");
    }

    void Awake()
    {
        PhotonNetwork.autoJoinLobby = true;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    private const string roomName = "RoomName";
    private RoomInfo[] roomsList;

    void OnGUI()
    {
        if (!PhotonNetwork.connected)
        {
            GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());
        }
        else if (PhotonNetwork.room == null)
        {
            // Create Room
            if (GUI.Button(new Rect(100, 100, 250, 100), "Start Server"))
            {
                //PhotonNetwork.CreateRoom(roomName + System.Guid.NewGuid().ToString("N"), true, true, 5);
                PhotonNetwork.CreateRoom(roomName + System.Guid.NewGuid().ToString("N"), new RoomOptions() { MaxPlayers = 5 }, null);
            }
            // Join Room
            RoomInfo[] list = PhotonNetwork.GetRoomList();
            print("lmao - " + list.Length);
            if (roomsList != null)
            {
                for (int i = 0; i < roomsList.Length; i++)
                {
                    if (GUI.Button(new Rect(100, 250 + (110 * i), 250, 100), "Join " + roomsList[i].name))
                    {
                        PhotonNetwork.JoinRoom(roomsList[i].name);
                    }
                }
            }
        }
    }

    void OnReceivedRoomListUpdate()
    {
        print("Received list");
        roomsList = PhotonNetwork.GetRoomList();
    }

    void OnJoinedRoom()
    {
        Debug.Log("Connected to Room");
        PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 0, 0), Quaternion.identity, 0);
    }
}
