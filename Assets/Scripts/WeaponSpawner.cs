using UnityEngine;
using System.Collections;
using System;

public class WeaponSpawner : MonoBehaviour, IUserInputListener {

    [SerializeField]
    public GameObject newRocket;

    public Rigidbody2D rigidBody;
    // -----------------------

    private InputManager inputManager;

    public void OnUserInputKeyDown(EInputGroup group, ICommand command)
    {
        if (command is ShootingCommand)
        {
            ShootingCommand shootingCommand = (ShootingCommand)command;
            switch (shootingCommand.control)
            {
                case EInputControls.ShootMain:
                    // Tuleks tekitada uus rakett iga kord, andes kaasa mängija koordinaadid.
                    Vector2 myPos = new Vector3(transform.position.x + 1, transform.position.y + 1);
                    Vector2 direction = shootingCommand.targetPos - myPos;
                    direction.Normalize();
                    Quaternion rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                    GameObject rocketGameObject = (GameObject)Instantiate(newRocket, myPos, rotation);
                    RocketMover missile = rocketGameObject.GetComponent<RocketMover>();
                    rocketGameObject.layer = gameObject.layer + 1;
                    missile.startMoving();
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
        rigidBody = GetComponent<Rigidbody2D>();
    }

    // Use this for initialization
    void Start()
    {
        inputManager = GameManager.getInstance().getInputManager();

        // Subscribe to all shooting input events
        inputManager.subscribeToInputGroup(EInputGroup.ShootingInput, this);
    }

    // Update is called once per frame
    void Update () {
	
	}
}
