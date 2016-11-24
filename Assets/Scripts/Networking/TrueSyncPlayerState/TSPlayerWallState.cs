using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class TSPlayerWallState : AbstractTSPMovementState
{
    private TSPMovementStateHandler.EWallDirection direction;

    public TSPlayerWallState(TSPMovementStateHandler handler, TSPMovementStateHandler.EWallDirection direction) : base(handler)
    {
        this.direction = direction;
    }

    public override bool check()
    {
        // If is on other wall now
        if (handler.isOnWall(direction == TSPMovementStateHandler.EWallDirection.Left ? TSPMovementStateHandler.EWallDirection.Right : TSPMovementStateHandler.EWallDirection.Left))
        {
            setNextState(new TSPlayerWallState(handler, direction == TSPMovementStateHandler.EWallDirection.Left ? TSPMovementStateHandler.EWallDirection.Right : TSPMovementStateHandler.EWallDirection.Left));
            return false;
        }

        if (!handler.isOnWall(direction))
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
        handler.counterGravityManipulation(direction == TSPMovementStateHandler.EWallDirection.Left ? EInputControls.MoveLeft : EInputControls.MoveRight);
    }

    public override bool FixedUpdate(Dictionary<TSPMovementStateHandler.ECommandType, Queue<ICommand>> commandCache)
    {
        // We can walljump, slide slower, slide faster or not stick to the wall at all (no autostick, hold key towards wall to stay on it)

        if (handler.isOnGround())
        {
            setNextState(new TSPlayerGroundState(handler));
            handler.forceOffWall(direction);
            return false;
        }

        // We are currently sticking to a wall
        handler.manipulateGravity(direction == TSPMovementStateHandler.EWallDirection.Left ? EInputControls.MoveLeft : EInputControls.MoveRight, -handler.playerCharacter.wallDeltaGravity);


        // Key up
        while (commandCache[TSPMovementStateHandler.ECommandType.Up].Count > 0)
        {

            ICommand baseCommand = commandCache[TSPMovementStateHandler.ECommandType.Up].Dequeue();

            if (baseCommand is MoveCommand)
            {
                switch (((MoveCommand)baseCommand).control)
                {
                    case EInputControls.MoveRight:

                        if (direction == TSPMovementStateHandler.EWallDirection.Right)
                        {
                            // We let go of the key that is holding us onto the wall
                            setNextState(new TSPlayerFlyingState(handler, true));
                            return false;
                        }
                        break;

                    case EInputControls.MoveLeft:

                        if (direction == TSPMovementStateHandler.EWallDirection.Left)
                        {
                            // We let go of the key that is holding us onto the wall
                            setNextState(new TSPlayerFlyingState(handler, true));
                            return false;
                        }
                        break;
                }
            }
        }

        // Key down
        while (commandCache[TSPMovementStateHandler.ECommandType.Down].Count > 0)
        {

            ICommand baseCommand = commandCache[TSPMovementStateHandler.ECommandType.Down].Dequeue();

            if (baseCommand is MoveCommand)
            {
                switch (((MoveCommand)baseCommand).control)
                {
                    case EInputControls.Jump:

                        if (direction == TSPMovementStateHandler.EWallDirection.Left)
                        {
                            // Half the jump power facing away from the wall and vertical jump also hindered
                            handler.counterGravityManipulation(EInputControls.MoveLeft);
                            handler.playerCharacter.rigidBody.AddForce(new Vector2(0.6f * handler.playerCharacter.jumpPower, 1.6f * handler.playerCharacter.jumpPower), ForceMode2D.Impulse);
                        }
                        else if (direction == TSPMovementStateHandler.EWallDirection.Right)
                        {
                            // Half the jump power facing away from the wall and vertical jump also hindered
                            handler.counterGravityManipulation(EInputControls.MoveRight);
                            handler.playerCharacter.rigidBody.AddForce(new Vector2(-0.6f * handler.playerCharacter.jumpPower, 1.6f * handler.playerCharacter.jumpPower), ForceMode2D.Impulse);
                        }

                        setNextState(new TSPlayerFlyingState(handler, true));
                        handler.forceOffWall(direction);
                        return false;
                }
            }
        }

        bool wasKeyHeld = false;

        // Key hold
        while (commandCache[TSPMovementStateHandler.ECommandType.Hold].Count > 0)
        {

            ICommand baseCommand = commandCache[TSPMovementStateHandler.ECommandType.Hold].Dequeue();
            
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

                        if (direction == TSPMovementStateHandler.EWallDirection.Left)
                        { 
                            // Counteract the wall sticking force means player will fall
                            setNextState(new TSPlayerFlyingState(handler, false));
                            handler.forceOffWall(direction);
                            return false;
                        }
                        else if (direction == TSPMovementStateHandler.EWallDirection.Right)
                        {
                            wasKeyHeld = true;
                        }
                        break;

                    case EInputControls.MoveLeft:

                        if (direction == TSPMovementStateHandler.EWallDirection.Right)
                        {
                            // Counteract the wall sticking force means player will fall
                            setNextState(new TSPlayerFlyingState(handler, false));
                            handler.forceOffWall(direction);
                            return false;
                        }
                        else if (direction == TSPMovementStateHandler.EWallDirection.Left)
                        {
                            wasKeyHeld = true;
                        }
                        break;
                }
            }
        }

        if (!wasKeyHeld)
        {
            if (handler.isOnGround())
            {
                setNextState(new TSPlayerGroundState(handler));
            }
            else
            {
                setNextState(new TSPlayerFlyingState(handler, false));
            }
           
            handler.forceOffWall(direction);
            return false;
        }

        // Done with this fixedupdate cycle
        handler.setCacheUsed();
        return true;
    }
}
