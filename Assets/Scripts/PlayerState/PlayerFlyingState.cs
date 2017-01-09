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

        /*if (isInJump)
        {
            foreach (MoveCommand comm in commandCache[PMovementStateHandler.ECommandType.Up])
            {
                bool isStillHoldingJump = false;
                if (comm.control == EInputControls.Jump)
                {
                    isStillHoldingJump = true;
                    break;
                }

                if (!isStillHoldingJump)
                {
                    handler.forceNotInJump();
                    isInJump = false;
                }
            }
        }

        if (isInJump && !skipCheckingJumpFirstUpdate)
        {
            bool isStillHoldingJump = false;

            foreach (MoveCommand comm in commandCache[PMovementStateHandler.ECommandType.Hold])
            {
                if (comm.control == EInputControls.Jump)
                {
                    isStillHoldingJump = true;
                    handler.setIsInJump();
                    break;
                }
            }

            if (!isStillHoldingJump)
            {
                isInJump = false;
                MonoBehaviour.print("Release jump1");
            }
        }
        else if (isInJump && skipCheckingJumpFirstUpdate)
        {
            // We are past the first update
            skipCheckingJumpFirstUpdate = false;

            bool isStillHoldingJump = false;

            // Also check key up just to make sure (only the first frame)
            foreach (MoveCommand comm in commandCache[PMovementStateHandler.ECommandType.Up])
            {
                if (comm.control == EInputControls.Jump)
                {
                    isStillHoldingJump = true;
                    handler.setIsInJump();
                    break;
                }
            }

            if (!isStillHoldingJump)
            {
                isInJump = false;
                MonoBehaviour.print("Release jump2");
            }
        }*/

        while (commandCache[(byte)PMovementStateHandler.ECommandType.Down].Count > 0)
        {
            ICommand baseCommand = commandCache[(byte)PMovementStateHandler.ECommandType.Down].Dequeue();
            if (baseCommand is MoveCommand)
            {
                switch (((MoveCommand)baseCommand).control)
                {
                    case EInputControls.JetPack:
                        if (handler.playerCharacter.getJetpackFuel() >= 3.5)
                        {
                            jump = false;
                            handler.playerCharacter.stopJets();
                            handler.playerCharacter.fireJets();
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

                        //handler.manipulateGravity(EInputControls.MoveUp, -handler.playerCharacter.verticalDeltaGravity);
                        if (!jump && handler.playerCharacter.getJetpackFuel() >= 20)
                        {
                            handler.manipulateGravity(EInputControls.MoveUp, -handler.playerCharacter.verticalDeltaGravity);
                            handler.playerCharacter.burst();
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

                        //handler.playerCharacter.rigidBody.AddForce(new Vector2(handler.playerCharacter.leftAndRightPower, 0), ForceMode2D.Impulse);
                        if (!jump && handler.playerCharacter.getJetpackFuel() >= 3.5f)
                        {
                            handler.playerCharacter.rigidBody.AddForce(new Vector2(handler.playerCharacter.leftAndRightPower, 0), ForceMode2D.Impulse);
                            handler.playerCharacter.rotateJetpack(90);
                        }

                        break;

                    case EInputControls.MoveLeft:

                        if (handler.isOnWall(PMovementStateHandler.EWallDirection.Left))
                        {
                            setNextState(new PlayerWallState(handler, PMovementStateHandler.EWallDirection.Left));
                            return false;
                        }

                        //handler.playerCharacter.rigidBody.AddForce(new Vector2(-handler.playerCharacter.leftAndRightPower, 0), ForceMode2D.Impulse);
                        if (!jump && handler.playerCharacter.getJetpackFuel() >= 3.5f)
                        {
                            handler.playerCharacter.rigidBody.AddForce(new Vector2(-handler.playerCharacter.leftAndRightPower, 0), ForceMode2D.Impulse);
                            handler.playerCharacter.rotateJetpack(-90);
                        }
                        break;

                    case EInputControls.JetPack:

                        //handler.playerCharacter.rigidBody.AddForce(new Vector2(0, handler.playerCharacter.jetPackPower), ForceMode2D.Impulse);
                        if (handler.playerCharacter.getJetpackFuel() >= 3.5f)
                        {
                            handler.playerCharacter.reduceFuel();
                            handler.playerCharacter.rotateJetpack();
                            handler.playerCharacter.rigidBody.AddForce(new Vector2(0, handler.playerCharacter.jetPackPower), ForceMode2D.Impulse);
                        }

                        /*if (!isInJump)
                        {
                            
                        } */

                        break;
                }
            }
        }

        // Done with this fixedupdate cycle
        handler.setCacheUsed();
        return true;
    }
}
