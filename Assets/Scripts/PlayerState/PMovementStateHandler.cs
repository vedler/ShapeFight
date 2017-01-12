using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PMovementStateHandler
{
    /*
        On update calls we can take all the player input and keep it here until we are ready for a fixedupdate. All physics calls should be done
        in fixedupdate, but all the input is received in normal update, so those two are not in sync.

        Every fixedupdate we let the current state first decide if it needs to switch to any other state and if not, handle the current cache of commands.

        Networked version of this:

            In normal mode, the commands are added by the PlayerCharacter script by receiving events from the InputManager. With the networked version, the
            server (or master client in this case, who acts as the server) will also run all of the commands received from RPC calls (client calls master client RPC's) 
            into the NetworkSynchroniser script that then uses the same PMovementStateHandler from the PlayerCharacter script to add the list of commands to all of
            the networked player objects.

            Master client will have the say what the actual positions of the characters should be and any non-master clients will only try to predict and then later
            reconcile any differences between the master client and the non-master characters. In other words all clients will use their inputs to move themselves, cache those
            movements or positional states and later with the response from the server, change their position to the one that was sent by the server. High-ping tests
            will be necessary to see if this concept will work out with larger pings. Photon should have some ping-testing scripts ready to go.

            Script execution order will be important here, since we want to first receive all the networked data and then run any updates from the PlayerCharacter script,
            which also delegates any update calls to the movement state handler. Movement changes will be interpolated and every new movement will also try to use very simple
            collision checking (just a raycast) to see if the changed movement will cause any collision problems. If they will, the interpolation is stopped and the final
            position is used instantly. Since re-simulating all of the movement (solving rigidbody physics multiple times in one physics update frame) with the input 
            is not possible, our concept of using movement and velocity delta interpolation would have to work.
    */

    public PlayerCharacter playerCharacter { get; private set; }
    public float currentGravityScale;
    public Dictionary<EInputControls, float> grvManipulatorsThisFrame;
    private bool deathTrigger;

    private float onGroundTimer;
    private float onLeftWallTimer;
    private float onRightWallTimer;

    private float forceOffLeftWallTimer;
    private float forceOffRightWallTimer;
    private float forceOffGroundTimer;

    private float pressedJumpTimer;
    private float forceDisablePressedJumpTimer;

    public EJetpackUsage jetpackUsageThisFrame { get; private set; }

    public enum EJetpackUsage : byte
    {
        None = 0,
        Normal,
        Enhanced,
        EnchanceNotUsed,
        Liftoff
    }

    public enum EWallDirection : byte
    {
        Left = 0,
        Right
    }

    public enum ECommandType : byte
    {
        Up = 0,
        Down,
        Hold
    }

    public Dictionary<byte, Queue<ICommand>> commandCache { get; private set; }
    public Dictionary<byte, Queue<ICommand>> finalizedCache { get; private set; }

    // Sometimes there are no FixedUpdates between two Update calls, so we don't want this input to be lost
    // Also to note: key up and key down commands will only be executed once between two update calls,
    //      but any key hold commands will be executed as many times as the physics engine seems fit,
    //      so all the key hold command executions should be using the fixed update deltaTime
    private bool hasFinalizedCacheBeenUsedOnce;

    private IPlayerState currentState;
    private bool cacheUsedThisUpdate;

    public void setCacheUsed()
    {
        cacheUsedThisUpdate = true;
    }

    public PMovementStateHandler(PlayerCharacter playerCharacter)
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
        //currentState = 
        currentState = new PlayerDeadState(this, PlayerDeadState.EDeadReason.Joined);
        currentState.enter();

        currentGravityScale = playerCharacter.defaultGravityScale;
        grvManipulatorsThisFrame = new Dictionary<EInputControls, float>();

        jetpackUsageThisFrame = EJetpackUsage.None;
        deathTrigger = false;
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
        Dictionary<byte, Queue<ICommand>> cloneCache = new Dictionary<byte, Queue<ICommand>>(finalizedCache);

        // Use a new gravity scale for this update
        currentGravityScale = playerCharacter.defaultGravityScale;
        playerCharacter.rigidBody.gravityScale = currentGravityScale;

        // Create a new list of gravity manipulators for one frame, so that duplicate input doesn't change the gravity twice
        grvManipulatorsThisFrame = new Dictionary<EInputControls, float>();

        HashSet<Type> usedStateClasses = new HashSet<Type>();

        if (deathTrigger)
        {
            deathTrigger = false;
            currentState.exit();
            currentState = new PlayerDeadState(this, PlayerDeadState.EDeadReason.Died);
            currentState.enter();
        }

        while (true)
        {
            //MonoBehaviour.print(currentState.ToString());

            // Check if the current state wants to switch or if the update wants to switch in the middle of the update
            if (currentState.check() && (cacheUsedThisUpdate || currentState.FixedUpdate(ref cloneCache)))
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

            // Do jetpack usage and reset if there were multiple jetpack usages this frame
            lateHandleJetpackUsage();
        }

        lateHandleJetpackUsage();

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
            playerCharacter.rigidBody.gravityScale = currentGravityScale;
        }
    }

    public void counterGravityManipulation(EInputControls manipulator)
    {
        if (grvManipulatorsThisFrame.Keys.Contains(manipulator))
        {
            // Counter the value
            currentGravityScale -= grvManipulatorsThisFrame[manipulator];
            playerCharacter.rigidBody.gravityScale = currentGravityScale;

            // Remove it from manipulators
            grvManipulatorsThisFrame.Remove(manipulator);
        }
    }
    
    private void resetCommandCache()
    {
        commandCache = new Dictionary<byte, Queue<ICommand>>();
        commandCache.Add((byte)ECommandType.Down, new Queue<ICommand>());
        commandCache.Add((byte)ECommandType.Up, new Queue<ICommand>());
        commandCache.Add((byte)ECommandType.Hold, new Queue<ICommand>());
    }

    private void checkForFinalizedCacheReuse()
    {
        if (!hasFinalizedCacheBeenUsedOnce)
        {
            commandCache = new Dictionary<byte, Queue<ICommand>>(finalizedCache);
            hasFinalizedCacheBeenUsedOnce = true;
        }
    }

    private void finalizeCommandCache()
    {
        finalizedCache = new Dictionary<byte, Queue<ICommand>>(commandCache);
        hasFinalizedCacheBeenUsedOnce = false;
    }

    public void addCommand(ECommandType type, ICommand command)
    {
        checkForFinalizedCacheReuse();

        commandCache[(byte)type].Enqueue(command);
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

    public void lateHandleJetpackUsage()
    {
        bool failed = false;

        if (jetpackUsageThisFrame == EJetpackUsage.EnchanceNotUsed || jetpackUsageThisFrame == EJetpackUsage.None)
        {
            failed = true;
        }
        else if (!playerCharacter.hasEnoughFuelFor(jetpackUsageThisFrame))
        {
            failed = true;
        }

        if (failed)
        {
            playerCharacter.stopJets();
            return;
        }
        
        playerCharacter.fireJets();

        playerCharacter.rigidBody.AddForce(new Vector2(0, playerCharacter.jetPackPower), ForceMode2D.Impulse);

        // Other specific changes
        switch (jetpackUsageThisFrame) {
            case EJetpackUsage.Normal:
                playerCharacter.rotateJetpack();
                break;
            case EJetpackUsage.Enhanced:
                playerCharacter.burst();
                break;
            case EJetpackUsage.Liftoff:
                playerCharacter.rotateJetpack();
                break;
            default:
                break;
        }
        
        playerCharacter.reduceFuel(jetpackUsageThisFrame);
        resetJetpackUsage();
    }

    public void setJetpackUsageThisFrame(EJetpackUsage usage)
    {
        jetpackUsageThisFrame = usage;
    }

    public void resetJetpackUsage()
    {
        jetpackUsageThisFrame = EJetpackUsage.None;
    }

    public void triggerDeath()
    {
        deathTrigger = true;
    }
}