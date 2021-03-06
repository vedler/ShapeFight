﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InputManager {

    private GameManager gameManager;

    private Dictionary<EInputGroup, List<IUserInputListener>> inputGroupListeners;
    private Dictionary<EInputControls, ICommand> inputControlCommands;
    private Dictionary<EInputControls, EInputGroup> inputGroups;
    private Dictionary<KeyCode, EInputControls> inputControlBinds;

    private List<KeyCode> pressedKeys;

    // Use this for initialization
    public InputManager(GameManager gameManager)
    {
        // Init
        this.gameManager = gameManager;

        // Create key to input group map
        inputGroups = new Dictionary<EInputControls, EInputGroup>();

        // Create the input group listener map
        inputGroupListeners = new Dictionary<EInputGroup, List<IUserInputListener>>();

        // Initialize input groups and listener maps
        initInputGroups();

        // Create the controls to commands map
        inputControlCommands = new Dictionary<EInputControls, ICommand>();

        // Initialize input controls to command mappings
        initUserControlBinds();

        // Create user input control map
        inputControlBinds = new Dictionary<KeyCode, EInputControls>();

        // Build default user control mappings
        // TODO: Custom user key mapping settings
        initUserControlMap();

        // Create list for holding pressed keys
        pressedKeys = new List<KeyCode>();
    }

    // Update is called once per frame
    public void Update()
    {
        // Check for any input
        parseInput();

        // TODO: Consider saving mouse movements as commands too to have crosshair/aiming movement in replays as well
    }

    private void parseInput()
    {
        // Check for released keys
        foreach (KeyCode key in pressedKeys)
        {
            if (Input.GetKeyUp(key))
            {
                // Get KeyCode's control and then the control's command
                ICommand command = inputControlCommands[inputControlBinds[key]];

                // Indicate the button is not held down or pressed for the first time, meaning it was released
                bool[] flags = new bool[2];
                flags[0] = false;
                flags[1] = false;

                // Execute this command
                command.execute(flags);
            }
        }

        pressedKeys = new List<KeyCode>();

        // Check if there is any input
        if (!Input.anyKey && !Input.anyKeyDown)
        {
            return;
        }

        foreach (KeyCode key in inputControlBinds.Keys)
        {
            bool hold = Input.GetKey(key);
            bool down = Input.GetKeyDown(key);

            if (hold || down)
            {
                // Add to list so we can later check if it was released
                pressedKeys.Add(key);

                // Get KeyCode's control and then the control's command
                ICommand command = inputControlCommands[inputControlBinds[key]];
                
                // Bool for command data
                bool[] flags = new bool[2];
                flags[0] = hold;
                flags[1] = down;

                // Execute this command
                command.execute(flags);
            }
        }
    }

    private void initInputGroups()
    {
        if (inputGroups == null || inputGroupListeners == null) return;

        // Movement controls
        inputGroups.Add(EInputControls.MoveUp, EInputGroup.MovementInput);
        inputGroups.Add(EInputControls.MoveDown, EInputGroup.MovementInput);
        inputGroups.Add(EInputControls.MoveLeft, EInputGroup.MovementInput);
        inputGroups.Add(EInputControls.MoveRight, EInputGroup.MovementInput);
        inputGroups.Add(EInputControls.Jump, EInputGroup.MovementInput);
        inputGroups.Add(EInputControls.JetPack, EInputGroup.MovementInput);

        inputGroupListeners.Add(EInputGroup.MovementInput, new List<IUserInputListener>());


        // Weapon controls
        inputGroups.Add(EInputControls.ShootMain, EInputGroup.ShootingInput);
        inputGroups.Add(EInputControls.ShootAlt, EInputGroup.ShootingInput);

        inputGroupListeners.Add(EInputGroup.ShootingInput, new List<IUserInputListener>());

    }

    private void initUserControlMap()
    {
        // Main movement controls
        inputControlBinds.Add(KeyCode.W, EInputControls.MoveUp);
        inputControlBinds.Add(KeyCode.S, EInputControls.MoveDown);
        inputControlBinds.Add(KeyCode.A, EInputControls.MoveLeft);
        inputControlBinds.Add(KeyCode.D, EInputControls.MoveRight);
        inputControlBinds.Add(KeyCode.Space, EInputControls.Jump);
        inputControlBinds.Add(KeyCode.LeftShift, EInputControls.JetPack);

        // Alternate movement controls
        inputControlBinds.Add(KeyCode.UpArrow, EInputControls.MoveUp);
        inputControlBinds.Add(KeyCode.DownArrow, EInputControls.MoveDown);
        inputControlBinds.Add(KeyCode.LeftArrow, EInputControls.MoveLeft);
        inputControlBinds.Add(KeyCode.RightArrow, EInputControls.MoveRight);
        inputControlBinds.Add(KeyCode.RightControl, EInputControls.Jump);
        inputControlBinds.Add(KeyCode.RightShift, EInputControls.JetPack);

        // Shooting controls
        inputControlBinds.Add(KeyCode.Mouse0, EInputControls.ShootMain);
        inputControlBinds.Add(KeyCode.Mouse1, EInputControls.ShootAlt);
    }

    private void initUserControlBinds()
    {
        // Move commands
        inputControlCommands.Add(EInputControls.MoveUp, new MoveCommand(EInputControls.MoveUp));
        inputControlCommands.Add(EInputControls.MoveDown, new MoveCommand(EInputControls.MoveDown));
        inputControlCommands.Add(EInputControls.MoveLeft, new MoveCommand(EInputControls.MoveLeft));
        inputControlCommands.Add(EInputControls.MoveRight, new MoveCommand(EInputControls.MoveRight));
        inputControlCommands.Add(EInputControls.Jump, new MoveCommand(EInputControls.Jump));
        inputControlCommands.Add(EInputControls.JetPack, new MoveCommand(EInputControls.JetPack));

        // Shooting commands
        inputControlCommands.Add(EInputControls.ShootMain, new ShootingCommand(EInputControls.ShootMain));
        inputControlCommands.Add(EInputControls.ShootAlt, new ShootingCommand(EInputControls.ShootAlt));
    }

    // --- Input event subscription ---

    public bool subscribeToInputGroup(EInputGroup group, IUserInputListener listener)
    {
        // Check if that input group is currently supported
        if (!inputGroupListeners.ContainsKey(group)) return false;

        // Make sure we don't add a duplicate listener
        if (!inputGroupListeners[group].Contains(listener))
        {
            inputGroupListeners[group].Add(listener);
        }

        return true;
    }

    public bool unsubscribeFromInputGroup(EInputGroup group, IUserInputListener listener)
    {
        // Check if that input group is currently supported
        if (!inputGroupListeners.ContainsKey(group)) return false;

        if (inputGroupListeners[group].Contains(listener))
        {
            inputGroupListeners[group].Remove(listener);
        }

        return true;
    }

    public void unsubscribeFromAllInputGroups(IUserInputListener listener)
    {
        // Remove from all user group listeners
        foreach (EInputGroup group in inputGroupListeners.Keys)
        {
            if (inputGroupListeners[group].Contains(listener))
            {
                inputGroupListeners[group].Remove(listener);
            }
        }
    }

    // -------------------------------------------------------------------

    public void invokeInputGroupEvent(EInputGroup group, ICommand command)
    {
        AbstractCommand absCommand = (AbstractCommand)command;

        foreach (IUserInputListener listener in inputGroupListeners[group])
        {
            // Pressed key
            if (absCommand.keyDown)
            {
                if (listener == null) continue;
                listener.OnUserInputKeyDown(group, command);
            }
            // Holding key
            if (absCommand.keyHold)
            {
                if (listener == null) continue;
                listener.OnUserInputKeyHold(group, command);
            }
            // Released key
            if (!absCommand.keyDown && !absCommand.keyHold)
            {
                if (listener == null) continue;
                listener.OnUserInputKeyUp(group, command);
            }
        }
    }
}
