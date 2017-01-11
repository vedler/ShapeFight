using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerFlyingState : AbstractPMovementState
{
    private bool isInJump;
    private static bool jump = true;
    private bool skipCheckingJumpFirstUpdate;

    public PlayerFlyingState(PMovementStateHandler handler, bool isInJump) : base(handler)
    {
        this.isInJump = isInJump;
        skipCheckingJumpFirstUpdate = false;
    }

    public override bool check()
    {
        // If we are not on ground, we want to switch states to flying
        if (handler.isOnGround())
        {
            setNextState(new PlayerGroundState(handler));
            return false;
        }
        
        return true;
    }

    public override void enter()
    {
        /*if (isInJump)
        {
            // Make sure we dont lose the Jump key hold when we first run the queue after switching states
            skipCheckingJumpFirstUpdate = true;
            handler.setIsInJump();
        }*/
    }

    public override void exit()
    {
        handler.forceNotInJump();
    }

    public override bool FixedUpdate(ref Dictionary<byte, Queue<ICommand>> commandCache)
    {
        // We can keep flying left or right or manipulate the up-down gravity or use the jetpack

        // Check if player is still holding the jump button (keyUp might have been eaten already)
        // Also make sure this isn't the first frame, where we probably have already eaten the jump press
        
        while (commandCache[(byte)PMovementStateHandler.ECommandType.Down].Count > 0)
        {
            ICommand baseCommand = commandCache[(byte)PMovementStateHandler.ECommandType.Down].Dequeue();
            if (baseCommand is MoveCommand)
            {
                switch (((MoveCommand)baseCommand).control)
                {
                    case EInputControls.JetPack:

                        if (handler.jetpackUsageThisFrame == PMovementStateHandler.EJetpackUsage.EnchanceNotUsed)
                        {
                            handler.setJetpackUsageThisFrame(PMovementStateHandler.EJetpackUsage.Enhanced);
                        }
                        else
                        {
                            handler.setJetpackUsageThisFrame(PMovementStateHandler.EJetpackUsage.Normal);
                        }

                        break;

                    case EInputControls.Jump:
                        jump = true;
                        break;
                }
            }
        }

        // Key hold
        while (commandCache[(byte)PMovementStateHandler.ECommandType.Hold].Count > 0)
        {

            ICommand baseCommand = commandCache[(byte)PMovementStateHandler.ECommandType.Hold].Dequeue();

            if (baseCommand is MoveCommand)
            {
                switch (((MoveCommand)baseCommand).control)
                {
                    case EInputControls.MoveUp:

                        handler.manipulateGravity(EInputControls.MoveUp, -handler.playerCharacter.verticalDeltaGravity);

                        if (handler.jetpackUsageThisFrame == PMovementStateHandler.EJetpackUsage.Normal)
                        {
                            // We already applied the power, lets show it was the enchanced usage of jetpack
                            handler.setJetpackUsageThisFrame(PMovementStateHandler.EJetpackUsage.Enhanced);
                        }
                        else
                        {
                            // We have not yet fired the jetpack, but let it know that it's going to be enhanced if it is fired
                            handler.setJetpackUsageThisFrame(PMovementStateHandler.EJetpackUsage.EnchanceNotUsed);
                        }

                        break;

                    case EInputControls.MoveDown:

                        handler.manipulateGravity(EInputControls.MoveDown, handler.playerCharacter.verticalDeltaGravity);
                        break;

                    case EInputControls.MoveRight:

                        if (handler.isOnWall(PMovementStateHandler.EWallDirection.Right))
                        {
                            setNextState(new PlayerWallState(handler, PMovementStateHandler.EWallDirection.Right));
                            return false;
                        }

                        handler.playerCharacter.rigidBody.AddForce(new Vector2(handler.playerCharacter.leftAndRightPower, 0), ForceMode2D.Impulse);
                        handler.playerCharacter.rotateJetpack(90);

                        break;

                    case EInputControls.MoveLeft:

                        if (handler.isOnWall(PMovementStateHandler.EWallDirection.Left))
                        {
                            setNextState(new PlayerWallState(handler, PMovementStateHandler.EWallDirection.Left));
                            return false;
                        }

                        handler.playerCharacter.rigidBody.AddForce(new Vector2(-handler.playerCharacter.leftAndRightPower, 0), ForceMode2D.Impulse);
                        handler.playerCharacter.rotateJetpack(-90);

                        break;

                    case EInputControls.JetPack:
                        
                        // See above about these usages
                        if (handler.jetpackUsageThisFrame == PMovementStateHandler.EJetpackUsage.EnchanceNotUsed)
                        {
                            handler.setJetpackUsageThisFrame(PMovementStateHandler.EJetpackUsage.Enhanced);
                        }
                        else
                        {
                            handler.setJetpackUsageThisFrame(PMovementStateHandler.EJetpackUsage.Normal);
                        }

                        break;
                }
            }
        }

        // Done with this fixedupdate cycle
        handler.setCacheUsed();
        return true;
    }
}
