using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class ShootingCommand : AbstractCommand
{
    public float mouseX { get; private set;  }
    public float mouseY { get; private set; }


    public ShootingCommand(EInputControls control) : base(control)
    {
        mouseX = 0;
        mouseY = 0;
    }

    public override void execute(bool[] inputFlags)
    {
        // Call AbstractCommand first to save the flags
        base.execute(inputFlags);
        
        // get pos

        GameManager.getInstance().getInputManager().invokeInputGroupEvent(EInputGroup.MovementInput, this);
    }
}