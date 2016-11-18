using UnityEngine;
using System.Collections;
using System;

public class WeaponSpawner : Photon.MonoBehaviour, IUserInputListener {

    public GameObject newRocket;
    public Rigidbody2D rigidBody;
    // -----------------------

    private InputManager inputManager;

    private WeaponManager weaponManager;

    public void OnUserInputKeyDown(EInputGroup group, ICommand command)
    {
        if (command is ShootingCommand)
        {
            ShootingCommand shootingCommand = (ShootingCommand)command;
            switch (shootingCommand.control)
            {
                case EInputControls.ShootMain:

                    //Parse target data
                    Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 myPos = new Vector3(transform.position.x, transform.position.y+0.5f);
                    Vector2 direction = targetPos - myPos;

                    //Normalize the vector for the adding of force
                    direction.Normalize();
                    Quaternion rotation = Quaternion.Euler(0, 0, -90 + (Mathf.Atan2(direction.y, direction.x)*Mathf.Rad2Deg));
                    weaponManager.ReuseObject(newRocket, myPos, rotation, direction);
                    break;
                    /*case EInputControls.ShootAlt:
                        siia vajalik kood
                        break;*/
            }
        }
    }

    public void OnUserInputKeyHold(EInputGroup group, ICommand command)
    {
        /*  Tulistamiseks vajalik
        if (command is ShootingCommand)
        {

            switch (((ShootingCommand)command).control)
            {

                case EInputControls.ShootMain:
                    vajalik kood siia
                    break;
                case EInputControls.ShootAlt:
                    vajalik kood siia
                    break;
            }
        } */
    }

    public void OnUserInputKeyUp(EInputGroup group, ICommand command)
    {
    }

    void Awake()
    {
        rigidBody = newRocket.GetComponent<Rigidbody2D>();
    }

    // Use this for initialization
    void Start()
    {
        // Init weapon manager
        weaponManager = GameManager.getInstance().getWeaponManager();

        //Creates a rocket pool with a size of rockets
        weaponManager.CreatePool(newRocket, 20);

        // If view is ours, attach us to the input manager
        if (photonView.isMine)
        {
            inputManager = GameManager.getInstance().getInputManager();
            // Subscribe to all shooting input events
            inputManager.subscribeToInputGroup(EInputGroup.ShootingInput, this);
        }
    }

    // Update is called once per frame
    void Update () {
	
	}
}
