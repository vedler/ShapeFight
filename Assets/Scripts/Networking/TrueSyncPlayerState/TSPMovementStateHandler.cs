using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TSPMovementStateHandler
{
    /*
        On update calls we can take all the player input and keep it here until we are ready for a fixedupdate. All physics calls should be done
        in fixedupdate, but all the input is received in normal update, so those two are not in sync.

        Every fixedupdate we let the current state first decide if it needs to switch to any other state and if not, handle the current cache of commands.
    */

    public TSPlayerCharacter playerCharacter { get; private set; }
    public float currentGravityScale;
    public Dictionary<EInputControls, float> grvManipulatorsThisFrame;

    private float onGroundTimer;
    private float onLeftWallTimer;
    private float onRightWallTimer;

    private float forceOffLeftWallTimer;
    private float forceOffRightWallTimer;
    private float forceOffGroundTimer;

    private float pressedJumpTimer;
    private float forceDisablePressedJumpTimer;

    public enum EWallDirection
    {
        Left,
        Right
    }

    public enum ECommandType
    {
        Up,
        Down,
        Hold
    }

    public Dictionary<ECommandType, Queue<ICommand>> commandCache { get; private set; }
    public Dictionary<ECommandType, Queue<ICommand>> finalizedCache { get; private set; }

    // Sometimes there are no FixedUpdates between two Update calls, so we don't want this input to be lost
    // Also to note: key up and key down commands will only be executed once between two update calls,
    //      but any key hold commands will be executed as many times as the physics engine seems fit,
    //      so all the key hold command executions should be using the fixed update deltaTime
    private bool hasFinalizedCacheBeenUsedOnce;

    private ITSPlayerState currentState;
    private bool cacheUsedThisUpdate;

    public void setCacheUsed()
    {
        cacheUsedThisUpdate = true;
    }

    public TSPMovementStateHandler(TSPlayerCharacter playerCharacter)
    {
        this.playerCharacter = playerCharacter;

        // True for initialization purposes
        hasFinalizedCacheBeenUsedOnce = true;

        // Initialize both caches
        resetCommandCache();
        finalizeCommandCache();

        onGroundTimer = float.MinValue;
        onLeftWallTimer = float.MinValue;
        onRightWallTimer = float.MinValue;


        forceOffLeftWallTimer = float.MinValue;
        forceOffRightWallTimer = float.MinValue;
        forceOffGroundTimer = float.MinValue;

        pressedJumpTimer = float.MinValue;
        forceDisablePressedJumpTimer = float.MinValue;

        cacheUsedThisUpdate = false;

        // Default state is flying
        currentState = new TSPlayerFlyingState(this, false);
        currentState.enter();

        currentGravityScale = TrueSync.FP.ToFloat(playerCharacter.defaultGravityScale);
        grvManipulatorsThisFrame = new Dictionary<EInputControls, float>();
    }

    public void FixedUpdate()
    {

        // Cache not used yet
        cacheUsedThisUpdate = false;

        // Fixedupdate might run several times per frame to "catch up" with the rest of input
        hasFinalizedCacheBeenUsedOnce = true;

        // Make a copy of the finalized cache
        // After using, only the "key hold" commands (to be executed multiple times between updates) 
        //      + any remaining commands from the clone should be in the finalized cache
        Dictionary<ECommandType, Queue<ICommand>> cloneCache = new Dictionary<ECommandType, Queue<ICommand>>(finalizedCache);

        // Use a new gravity scale for this update
        currentGravityScale = TrueSync.FP.ToFloat(playerCharacter.defaultGravityScale);
        playerCharacter.gravityScale = currentGravityScale;

        // Create a new list of gravity manipulators for one frame, so that a double'd input doesn't change the gravity twice
        grvManipulatorsThisFrame = new Dictionary<EInputControls, float>();

        HashSet<Type> usedStateClasses = new HashSet<Type>();

        while (true)
        {
            MonoBehaviour.print(currentState.ToString());

            // Check if the current state wants to switch or if the update wants to switch in the middle of the update
            if (currentState.check() && (cacheUsedThisUpdate || currentState.FixedUpdate(cloneCache)))
            {
                // If the check was good and the fixedupdate finished, we are done with this full FixedUpdate call
                break;
            }

            // Prevent infinite loop
            if (usedStateClasses.Contains<Type>(currentState.getNextState().GetType()))
            {
                break;
            }

            // Otherwise we switch states and keep executing
            currentState.exit();
            currentState = currentState.getNextState();
            currentState.enter();

            // Add to list of used states
            usedStateClasses.Add(currentState.GetType());
        }
        
        cacheUsedThisUpdate = true;
    }

    public void LateUpdate()
    {
        finalizeCommandCache();
        resetCommandCache();
    }

    public void manipulateGravity(EInputControls manipulator, float manipulation)
    {
        if (!grvManipulatorsThisFrame.Keys.Contains(manipulator))
        {
            grvManipulatorsThisFrame.Add(manipulator, manipulation);
            currentGravityScale += manipulation;
            playerCharacter.gravityScale = currentGravityScale;
        }
    }

    public void counterGravityManipulation(EInputControls manipulator)
    {
        if (grvManipulatorsThisFrame.Keys.Contains(manipulator))
        {
            // Counter the value
            currentGravityScale -= grvManipulatorsThisFrame[manipulator];
            playerCharacter.gravityScale = currentGravityScale;

            // Remove it from manipulators
            grvManipulatorsThisFrame.Remove(manipulator);
        }
    }
    
    private void resetCommandCache()
    {
        commandCache = new Dictionary<ECommandType, Queue<ICommand>>();
        commandCache.Add(ECommandType.Down, new Queue<ICommand>());
        commandCache.Add(ECommandType.Up, new Queue<ICommand>());
        commandCache.Add(ECommandType.Hold, new Queue<ICommand>());
    }

    private void checkForFinalizedCacheReuse()
    {
        if (!hasFinalizedCacheBeenUsedOnce)
        {
            commandCache = new Dictionary<ECommandType, Queue<ICommand>>(finalizedCache);
            hasFinalizedCacheBeenUsedOnce = true;
        }
    }

    private void finalizeCommandCache()
    {
        finalizedCache = new Dictionary<ECommandType, Queue<ICommand>>(commandCache);
        hasFinalizedCacheBeenUsedOnce = false;
    }

    public void addCommand(ECommandType type, ICommand command)
    {
        checkForFinalizedCacheReuse();

        commandCache[type].Enqueue(command);
    }
    
    public void setOnGround()
    {
        onGroundTimer = Time.fixedTime;
    }

    public void forceOffGround()
    {
        forceOffGroundTimer = Time.fixedTime;
        onGroundTimer = float.MinValue;
    }

    public bool isOnGround()
    {
        // If the last time we were on ground was roughly 2 frames ago, we are still on ground

        if (forceOffGroundTimer != float.MinValue)
        {
            if (forceOffGroundTimer + Time.fixedDeltaTime > Time.fixedTime)
            {
                return false;
            }
            else
            {
                forceOffGroundTimer = float.MinValue;
                return true;
            }
        }

        return onGroundTimer + 2 * Time.fixedDeltaTime > Time.fixedTime;
    }

    public void setOnWall(EWallDirection direction)
    {
        if (direction == EWallDirection.Left)
        {
            onLeftWallTimer = Time.fixedTime;
        }
        else
        {
            onRightWallTimer = Time.fixedTime;
        }
    }

    public bool isOnWall(EWallDirection direction)
    {
        // If the last time we were on a wall was roughly 2 frames ago, we are still on the wall
        if (direction == EWallDirection.Left)
        {
            if (forceOffLeftWallTimer != float.MinValue)
            {
                if (forceOffLeftWallTimer + Time.fixedDeltaTime > Time.fixedTime)
                {
                    return false;
                }
                else
                {
                    forceOffLeftWallTimer = float.MinValue;
                    return true;
                }
            }

            return onLeftWallTimer + 2 * Time.fixedDeltaTime > Time.fixedTime;
        }
        else
        {
            if (forceOffRightWallTimer != float.MinValue)
            {
                if (forceOffRightWallTimer + Time.fixedDeltaTime > Time.fixedTime)
                {
                    return false;
                }
                else
                {
                    forceOffRightWallTimer = float.MinValue;
                    return true;
                }
            }

            return onRightWallTimer + 2 * Time.fixedDeltaTime > Time.fixedTime;
        }
    }

    public void forceOffWall(EWallDirection direction)
    {
        if (direction == EWallDirection.Left)
        {
            onLeftWallTimer = float.MinValue;
            forceOffLeftWallTimer = Time.fixedTime;
        }
        else
        {
            onRightWallTimer = float.MinValue;
            forceOffRightWallTimer = Time.fixedTime;
        }
    }

    public void setIsInJump()
    {
        pressedJumpTimer = Time.fixedTime;
    }

    public void forceNotInJump()
    {
        forceDisablePressedJumpTimer = Time.fixedTime;
        pressedJumpTimer = float.MinValue;
    }

    public bool isInJump()
    {
        // If the last time we were on ground was roughly 2 frames ago, we are still on ground

        if (forceDisablePressedJumpTimer != float.MinValue)
        {
            if (forceDisablePressedJumpTimer + Time.fixedDeltaTime > Time.fixedTime)
            {
                return false;
            }
            else
            {
                forceDisablePressedJumpTimer = float.MinValue;
                return true;
            }
        }

        return pressedJumpTimer + Time.fixedDeltaTime > Time.fixedTime;
    }
}