using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerGroundState : AbstractPMovementState
{
    public PlayerGroundState(PMovementStateHandler handler) : base(handler)
    {
    }

    public override bool check()
    {
        // If we are not on ground, we want to switch states to flying
        if (!handler.isOnGround())
        {
            setNextState(new PlayerFlyingState(handler));
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

    public override bool FixedUpdate(Dictionary<PMovementStateHandler.ECommandType, Queue<ICommand>> commandCache)
    {
        // We can jump, move left and move right, also we can manipulate gravity (just to be sure)

        // Key Down
        while (commandCache[PMovementStateHandler.ECommandType.Down].Count > 0)
        {

            ICommand baseCommand = commandCache[PMovementStateHandler.ECommandType.Down].Dequeue();

            if (baseCommand is MoveCommand)
            {
                switch (((MoveCommand)baseCommand).control)
                {
                    case EInputControls.Jump:
                        handler.playerCharacter.rigidBody.AddForce(new Vector2(0, handler.playerCharacter.jumpPower), ForceMode2D.Impulse);

                        // Tell the state handler we want to switch states now
                        setNextState(new PlayerFlyingState(handler));
                        handler.forceOffGround();
                        return false;
                }
            }
        }

        // Key hold
        while (commandCache[PMovementStateHandler.ECommandType.Hold].Count > 0)
        {

            ICommand baseCommand = commandCache[PMovementStateHandler.ECommandType.Hold].Dequeue();

            if (baseCommand is MoveCommand)
            {
                switch (((MoveCommand)baseCommand).control)
                {
                    case EInputControls.MoveUp:

                        handler.manipulateGravity(EInputControls.MoveUp, - handler.playerCharacter.verticalDeltaGravity);
                        break;

                    case EInputControls.MoveDown:

                        handler.manipulateGravity(EInputControls.MoveDown, handler.playerCharacter.verticalDeltaGravity);
                        break;

                    case EInputControls.MoveRight:

                        handler.playerCharacter.rigidBody.AddForce(new Vector2(handler.playerCharacter.leftAndRightPower, 0), ForceMode2D.Impulse);
                        break;

                    case EInputControls.MoveLeft:
                        
                        handler.playerCharacter.rigidBody.AddForce(new Vector2(-handler.playerCharacter.leftAndRightPower, 0), ForceMode2D.Impulse);
                        break;
                }
            }
        }

        // We are done with this cycle and dont need to switch states
        handler.setCacheUsed();
        return true;
    }
}