using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public abstract class AbstractTSPMovementState : ITSPlayerState
{
    public TSPMovementStateHandler handler { get; protected set; }
    private ITSPlayerState nextState;

    public AbstractTSPMovementState(TSPMovementStateHandler handler)
    {
        this.handler = handler;
    }

    public abstract void enter();
    public abstract void exit();

    // If the command forces the player off the ground, make the check code be run
    public abstract bool FixedUpdate(Dictionary<TSPMovementStateHandler.ECommandType, Queue<ICommand>> commandCache);

    // Check if the state wants to make a switch
    public abstract bool check();

    // Get the next state to be switched to
    public ITSPlayerState getNextState()
    {
        return nextState;
    }

    protected void setNextState(ITSPlayerState nextState)
    {
        this.nextState = nextState;
    }
}