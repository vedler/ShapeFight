using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerWallState : AbstractPMovementState
{
    private PMovementStateHandler.EWallDirection direction;

    public PlayerWallState(PMovementStateHandler handler, PMovementStateHandler.EWallDirection direction) : base(handler)
    {
        this.direction = direction;
    }

    public override bool check()
    {
        // If is on other wall now
        if (handler.isOnWall(direction == PMovementStateHandler.EWallDirection.Left ? PMovementStateHandler.EWallDirection.Right : PMovementStateHandler.EWallDirection.Left))
        {
            setNextState(new PlayerWallState(handler, direction == PMovementStateHandler.EWallDirection.Left ? PMovementStateHandler.EWallDirection.Right : PMovementStateHandler.EWallDirection.Left));
            return false;
        }

        if (!handler.isOnWall(direction))
        {
            setNextState(new PlayerFlyingState(handler, false));
            return false;
        }

        return true;
    }

    public override void enter()
    {
    }

    public override void exit()
    {
        handler.counterGravityManipulation(direction == PMovementStateHandler.EWallDirection.Left ? EInputControls.MoveLeft : EInputControls.MoveRight);
    }

    public override bool FixedUpdate(Dictionary<PMovementStateHandler.ECommandType, Queue<ICommand>> commandCache)
    {
        // We can walljump, slide slower, slide faster or not stick to the wall at all (no autostick, hold key towards wall to stay on it)

        if (handler.isOnGround())
        {
            setNextState(new PlayerGroundState(handler));
            handler.forceOffWall(direction);
            return false;
        }

        // We are currently sticking to a wall
        handler.manipulateGravity(direction == PMovementStateHandler.EWallDirection.Left ? EInputControls.MoveLeft : EInputControls.MoveRight, -handler.playerCharacter.wallDeltaGravity);


        // Key up
        while (commandCache[PMovementStateHandler.ECommandType.Up].Count > 0)
        {

            ICommand baseCommand = commandCache[PMovementStateHandler.ECommandType.Up].Dequeue();

            if (baseCommand is MoveCommand)
            {
                switch (((MoveCommand)baseCommand).control)
                {
                    case EInputControls.MoveRight:

                        if (direction == PMovementStateHandler.EWallDirection.Right)
                        {
                            // We let go of the key that is holding us onto the wall
                            setNextState(new PlayerFlyingState(handler, true));
                            return false;
                        }
                        break;

                    case EInputControls.MoveLeft:

                        if (direction == PMovementStateHandler.EWallDirection.Left)
                        {
                            // We let go of the key that is holding us onto the wall
                            setNextState(new PlayerFlyingState(handler, true));
                            return false;
                        }
                        break;
                }
            }
        }

        // Key down
        while (commandCache[PMovementStateHandler.ECommandType.Down].Count > 0)
        {

            ICommand baseCommand = commandCache[PMovementStateHandler.ECommandType.Down].Dequeue();

            if (baseCommand is MoveCommand)
            {
                switch (((MoveCommand)baseCommand).control)
                {
                    case EInputControls.Jump:

                        if (direction == PMovementStateHandler.EWallDirection.Left)
                        {
                            // Half the jump power facing away from the wall and vertical jump also hindered
                            handler.counterGravityManipulation(EInputControls.MoveLeft);
                            handler.playerCharacter.rigidBody.AddForce(new Vector2(0.6f * handler.playerCharacter.jumpPower, 1.6f * handler.playerCharacter.jumpPower), ForceMode2D.Impulse);
                        }
                        else if (direction == PMovementStateHandler.EWallDirection.Right)
                        {
                            // Half the jump power facing away from the wall and vertical jump also hindered
                            handler.counterGravityManipulation(EInputControls.MoveRight);
                            handler.playerCharacter.rigidBody.AddForce(new Vector2(-0.6f * handler.playerCharacter.jumpPower, 1.6f * handler.playerCharacter.jumpPower), ForceMode2D.Impulse);
                        }

                        setNextState(new PlayerFlyingState(handler, true));
                        handler.forceOffWall(direction);
                        return false;

                    case EInputControls.JetPack:
                        if (handler.playerCharacter.getJetpackFuel() >= 3.5)
                        {
                            handler.playerCharacter.stopJets();
                            handler.playerCharacter.fireJets();
                        }
                        break;
                }
            }
        }

        bool wasKeyHeld = false;

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

                        if (direction == PMovementStateHandler.EWallDirection.Left)
                        { 
                            // Counteract the wall sticking force means player will fall
                            setNextState(new PlayerFlyingState(handler, false));
                            handler.forceOffWall(direction);
                            return false;
                        }
                        else if (direction == PMovementStateHandler.EWallDirection.Right)
                        {
                            wasKeyHeld = true;
                        }
                        break;

                    case EInputControls.MoveLeft:

                        if (direction == PMovementStateHandler.EWallDirection.Right)
                        {
                            // Counteract the wall sticking force means player will fall
                            setNextState(new PlayerFlyingState(handler, false));
                            handler.forceOffWall(direction);
                            return false;
                        }
                        else if (direction == PMovementStateHandler.EWallDirection.Left)
                        {
                            wasKeyHeld = true;
                        }
                        break;

                    case EInputControls.JetPack:

                        if (handler.playerCharacter.getJetpackFuel() >= 3.5)
                        {
                            handler.playerCharacter.reduceFuel();
                            handler.playerCharacter.rotateJetpack();
                            handler.playerCharacter.rigidBody.AddForce(new Vector2(0, handler.playerCharacter.jetPackPower), ForceMode2D.Impulse);
                        }
                        break;
                }
            }
        }

        if (!wasKeyHeld)
        {
            if (handler.isOnGround())
            {
                setNextState(new PlayerGroundState(handler));
            }
            else
            {
                setNextState(new PlayerFlyingState(handler, false));
            }
           
            handler.forceOffWall(direction);
            return false;
        }

        // Done with this fixedupdate cycle
        handler.setCacheUsed();
        return true;
    }
}
