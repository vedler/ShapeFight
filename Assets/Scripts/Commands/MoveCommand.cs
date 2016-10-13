using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class MoveCommand : ICommand
{
    public EInputControls control { get; private set; }

    public bool keyHold { get; private set; }
    public bool keyDown { get; private set; }

    public MoveCommand(EInputControls control) : this(control, false, false)
    { 
    }

    private MoveCommand(EInputControls control, bool keyHold, bool keyDown)
    {
        this.control = control;
        this.keyHold = keyHold;
        this.keyDown = keyDown;
    }

    public void execute()
    {
        // TODO: Add a clone of this command to the game's history data with a proper timestamp in milliseconds

        GameManager.getInstance().getInputManager().invokeInputGroupEvent(EInputGroup.MovementInput, this);
    }

    public void execute(object data)
    {
        // TODO: Consider abstractizing this part of code

        // Check if it is correct length and trype
        if (!(data is bool[]) || ((bool[])data).Length != 2)
        {
            // TODO: Throw exception.
            return;
        }

        keyHold = ((bool[])data)[0];
        keyDown = ((bool[])data)[1];

        execute();
    }

    public ICommand clone()
    {
        return new MoveCommand(control, keyHold, keyDown);
    }
}