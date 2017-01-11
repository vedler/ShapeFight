using UnityEngine;
using System.Collections;
using ExitGames.Client.Photon;

public class NetworkManager : Photon.MonoBehaviour {

    [SerializeField]
    public GameObject playerPrefab;

    public GameObject localPlayerObject { get; private set; }
    public PlayerCharacter localPlayerCharacter { get; private set; }

    [SerializeField]
    public GameObject bulletPrefab;
    public GameObject rocketPrefab;
    public GameObject grenadePrefab;
    public GameObject pelletPrefab;

    // Use this for initialization
    void Start () {
        PhotonNetwork.ConnectUsingSettings("0.1a");
        // PhotonPeer.RegisterType(Type customType, byte code, SerializeMethod serializeMethod, DeserializeMethod constructor)
        print("reg:" + PhotonPeer.RegisterType(typeof(AbstractCommand), (byte)'A', AbstractCommand.Serialize, AbstractCommand.Serialize));
        print("reg:" + PhotonPeer.RegisterType(typeof(MoveCommand), (byte)'M', MoveCommand.Serialize, MoveCommand.Serialize));
        print("reg:" + PhotonPeer.RegisterType(typeof(ShootingCommand), (byte)'S', ShootingCommand.Serialize, ShootingCommand.Serialize));

        //SerializeEnum
        //print("reg:" + PhotonPeer.RegisterType(typeof(PMovementStateHandler.ECommandType), (byte)'C', GameManager.SerializeEnum, GameManager.DeserializeEnum));
        //print("reg:" + PhotonPeer.RegisterType(typeof(PMovementStateHandler.EWallDirection), (byte)'W', GameManager.SerializeEnum2, GameManager.DeserializeEnum2));

        // Protocol
        /*Protocol.TryRegisterType(typeof(AbstractCommand), (byte)'A', AbstractCommand.Serialize, AbstractCommand.Serialize);
        Protocol.TryRegisterType(typeof(MoveCommand), (byte)'M', MoveCommand.Serialize, MoveCommand.Serialize);
        Protocol.TryRegisterType(typeof(ShootingCommand), (byte)'S', ShootingCommand.Serialize, ShootingCommand.Serialize);

        //SerializeEnum
        Protocol.TryRegisterType(typeof(PMovementStateHandler.ECommandType), (byte)'C', GameManager.SerializeEnum, GameManager.DeserializeEnum);
        Protocol.TryRegisterType(typeof(PMovementStateHandler.EWallDirection), (byte)'W', GameManager.SerializeEnum, GameManager.DeserializeEnum);*/
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
        localPlayerObject = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 0, 0), Quaternion.identity, 0);
        localPlayerCharacter = localPlayerObject.GetComponent<PlayerCharacter>();
    }
}
