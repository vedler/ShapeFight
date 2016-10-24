using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerFlyingState : AbstractPMovementState
{
    public PlayerFlyingState(PMovementStateHandler handler) : base(handler)
    {
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
    }

    public override void exit()
    {
    }

    public override bool FixedUpdate(Dictionary<PMovementStateHandler.ECommandType, Queue<ICommand>> commandCache)
    {
        // We can keep flying left or right or manipulate the up-down gravity or use the jetpack

        // Key hold
        while (commandCache[PMovementStateHandler.ECommandType.Hold].Count > 0)
        {

            ICommand baseCommand = commandCache[PMovementStateHandler.ECommandType.Hold].Dequeue();

            if (baseCommand is MoveCommand)
            {
                switch (((MoveCommand)baseCommand).control)
                {
                    case EInputControls.MoveUp:

                        handler.manipulateGravity(EInputControls.MoveUp, -handler.playerCharacter.verticalDeltaGravity);
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
                        break;

                    case EInputControls.MoveLeft:

                        if (handler.isOnWall(PMovementStateHandler.EWallDirection.Left))
                        {
                            setNextState(new PlayerWallState(handler, PMovementStateHandler.EWallDirection.Left));
                            return false;
                        }

                        handler.playerCharacter.rigidBody.AddForce(new Vector2(-handler.playerCharacter.leftAndRightPower, 0), ForceMode2D.Impulse);
                        break;

                    case EInputControls.Jump:

                        // TODO: Jetpack
                        break;
                }
            }
        }

        // Done with this fixedupdate cycle
        handler.setCacheUsed();
        return true;
    }
}
