using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class AbstractPMovementState : IPlayerState
{
    public PMovementStateHandler handler { get; protected set; }
    private IPlayerState nextState;

    public AbstractPMovementState(PMovementStateHandler handler)
    {
        this.handler = handler;
    }

    public abstract void enter();
    public abstract void exit();

    // If the command forces the player off the ground, make the check code be run
    public abstract bool FixedUpdate(Dictionary<PMovementStateHandler.ECommandType, Queue<ICommand>> commandCache);

    // Check if the state wants to make a switch
    public abstract bool check();

    // Get the next state to be switched to
    public IPlayerState getNextState()
    {
        return nextState;
    }

    protected void setNextState(IPlayerState nextState)
    {
        this.nextState = nextState;
    }
}