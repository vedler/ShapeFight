using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TrueSync;
using UnityEngine;

public class TSPlayerGroundState : AbstractTSPMovementState
{
    public TSPlayerGroundState(TSPMovementStateHandler handler) : base(handler)
    {
    }

    public override bool check()
    {
        // If we are not on ground, we want to switch states to flying
        if (!handler.isOnGround())
        {
            setNextState(new TSPlayerFlyingState(handler, false));
            return false;
        }

        return true; 
    }

    public override void enter()
    {
        
    }

    public override void exit()
    {
        
    }

    public override bool FixedUpdate(Dictionary<TSPMovementStateHandler.ECommandType, Queue<ICommand>> commandCache)
    {
        // We can jump, move left and move right, also we can manipulate gravity (just to be sure)

        // Key Down
        while (commandCache[TSPMovementStateHandler.ECommandType.Down].Count > 0)
        {

            ICommand baseCommand = commandCache[TSPMovementStateHandler.ECommandType.Down].Dequeue();

            if (baseCommand is MoveCommand)
            {
                switch (((MoveCommand)baseCommand).control)
                {
                    case EInputControls.Jump:
                        handler.playerCharacter.rigidBody.AddForce(new TSVector2(0, handler.playerCharacter.jumpPower), ForceMode.Impulse);

                        // Tell the state handler we want to switch states now
                        setNextState(new TSPlayerFlyingState(handler, true));
                        handler.forceOffGround();
                        return false;
                }
            }
        }

        // Key hold
        while (commandCache[TSPMovementStateHandler.ECommandType.Hold].Count > 0)
        {

            ICommand baseCommand = commandCache[TSPMovementStateHandler.ECommandType.Hold].Dequeue();

            if (baseCommand is MoveCommand)
            {
                switch (((MoveCommand)baseCommand).control)
                {
                    case EInputControls.MoveUp:

                        handler.manipulateGravity(EInputControls.MoveUp, - handler.playerCharacter.verticalDeltaGravity.AsFloat());
                        break;

                    case EInputControls.MoveDown:

                        handler.manipulateGravity(EInputControls.MoveDown, handler.playerCharacter.verticalDeltaGravity.AsFloat());
                        break;

                    case EInputControls.MoveRight:

                        handler.playerCharacter.rigidBody.AddForce(new TSVector2(handler.playerCharacter.leftAndRightPower, 0), ForceMode.Impulse);
                        break;

                    case EInputControls.MoveLeft:
                        
                        handler.playerCharacter.rigidBody.AddForce(new TSVector2(-handler.playerCharacter.leftAndRightPower, 0), ForceMode.Impulse);
                        break;
                    case EInputControls.JetPack:

                        handler.playerCharacter.rigidBody.AddForce(new TSVector2(0, handler.playerCharacter.jetPackPower), ForceMode.Impulse);
                        /*if (!isInJump)
                        {
                            
                        } */

                        break;
                }
            }
        }

        // We are done with this cycle and dont need to switch states
        handler.setCacheUsed();
        return true;
    }
}