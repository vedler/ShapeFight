using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static float currentFixedTime { get; private set; }

    public static GameManager getInstance()
    {
        if (instance == null)
        {
            // Instead of having the GameManager in the scene, load it in as an object on getInstance()
            GameObject manager = new GameObject("[GameManager]");
            instance = manager.AddComponent<GameManager>();
            DontDestroyOnLoad(manager);
        }

        return instance;
    }

    private InputManager inputManager;

    public InputManager getInputManager()
    {
        if (inputManager == null)
        {
            throw new UnityException("GameManager's InputManager instance was accessed before Unity started it!");
        }

        return inputManager;
    }

    private WeaponManager weaponManager;

    public WeaponManager getWeaponManager()
    {
        if (weaponManager == null)
        {
            throw new UnityException("GameManager's WeaponManager instance was accessed before Unity started it!");
        }

        return weaponManager;
    }

    private WSelectionManager weaponSelectionManager;

    public WSelectionManager getWSelectionManager()
    {
        if (weaponSelectionManager == null)
        {
            throw new UnityException("GameManager's WSelectionManager instance was accessed before Unity started it!");
        }
        return weaponSelectionManager;
    }

    private NetworkManager networkManager;

    public NetworkManager getNetworkManager()
    {
        if (networkManager == null)
        {
            networkManager = FindObjectOfType<NetworkManager>();
        }

        return networkManager;
    }

    // Use this for initialization
    void Awake()
    {
        // Set the singleton instance
        GameManager.instance = this;

        // Load other managers
        // PS: We don't make them as a game components, because we want them to be only accessible through the GameManager instance


        // Load and initialize InputManager
        this.inputManager = new InputManager(this);

        //Load and initialize WeaponManager
        this.weaponManager = new WeaponManager(this);

        this.weaponSelectionManager = new WSelectionManager(this);

        // Get the network manager object from the scene
        this.networkManager = FindObjectOfType<NetworkManager>();

        // PhotonPeer.RegisterType(Type customType, byte code, SerializeMethod serializeMethod, DeserializeMethod constructor)

        // Doesn't matter that this time is not synced with the server, it is used more as an identifier
        currentFixedTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        inputManager.Update();
    }

    void FixedUpdate()
    {
        currentFixedTime += Time.fixedDeltaTime;
    }

    // Enum serialization
    public static byte[] SerializeEnum(object customobject)
    {
        return ExitGames.Client.Photon.Protocol.Serialize((byte)customobject);
    }

    public static object DeserializeEnum(byte[] bytes)
    {
        return ExitGames.Client.Photon.Protocol.Deserialize(bytes);
    }

    // Enum serialization
    public static byte[] SerializeEnum2(object customobject)
    {
        return ExitGames.Client.Photon.Protocol.Serialize((byte)customobject);
    }

    public static object DeserializeEnum2(byte[] bytes)
    {
        return ExitGames.Client.Photon.Protocol.Deserialize(bytes);
    }
}
