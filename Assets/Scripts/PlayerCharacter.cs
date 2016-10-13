using UnityEngine;
using System.Collections;
using System;

public class PlayerCharacter : MonoBehaviour, IUserInputListener {

    private InputManager inputManager;

    public bool ready { get; private set; }

    // Coroutine to reiterate until finished with loading
    IEnumerator initPlayerCharacter()
    {
        if (GameManager.getInstance() == null || !GameManager.getInstance().areManagersReady())
        {
            yield return new WaitForSeconds(0.1f);    
        }

        inputManager = GameManager.getInstance().getInputManager();

        // Subscribe to all movement input events
        inputManager.subscribeToInputGroup(EInputGroup.MovementInput, this);

        ready = true;
        yield break;
    }

    // Use this for initialization
    void Start () {
        StartCoroutine(initPlayerCharacter());
	}
	
	// Update is called once per frame
	void Update () {
        if (!ready) return;
	}

    public void OnUserInput(EInputGroup group, ICommand command)
    {
        if (!ready) return;
        
        // TODO: Remake with physics + force, currently only to show using eventhandler and commands

        if (command is MoveCommand)
        {

            switch (((MoveCommand)command).control)
            {
                case EInputControls.MoveUp:
                    transform.Translate(0, Time.deltaTime * 3.0f, 0);
                    break;
                case EInputControls.MoveDown:
                    transform.Translate(0, Time.deltaTime * -3.0f, 0);
                    break;
                case EInputControls.MoveRight:
                    transform.Translate(Time.deltaTime * 3.0f, 0, 0);
                    break;
                case EInputControls.MoveLeft:
                    transform.Translate(Time.deltaTime * -3.0f, 0, 0);
                    break;
            }
        }
    }    


    public void OnUserInputKeyDown(EInputGroup group, ICommand command)
    {
        if (!ready) return;
    }
}
