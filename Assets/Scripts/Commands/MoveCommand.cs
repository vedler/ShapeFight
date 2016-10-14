using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class MoveCommand : AbstractCommand
{
    // C# doesn't allow fully abstract constructors, constructor call to base class has to be made manually
    public MoveCommand(EInputControls control) : base(control)
    {
    }

    public override void execute(bool[] inputFlags)
    {
        // Call AbstractCommand first to save the flags
        base.execute(inputFlags);

        // TODO: Add a clone of this command to the game's history data with a proper timestamp in milliseconds
        
        GameManager.getInstance().getInputManager().invokeInputGroupEvent(EInputGroup.MovementInput, this);
    }
    
}