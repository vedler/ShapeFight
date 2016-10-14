using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class AbstractCommand : ICommand
{
    public EInputControls control { get; protected set; }

    public bool keyHold { get; protected set; }
    public bool keyDown { get; protected set; }

    protected AbstractCommand(EInputControls control) : this(control, false, false)
    {
    }

    private AbstractCommand(EInputControls control, bool keyHold, bool keyDown)
    {
        this.control = control;
        this.keyHold = keyHold;
        this.keyDown = keyDown;
    }


    public virtual ICommand clone()
    {
        return clone<AbstractCommand>();
    }

    // Clone with correct type
    public virtual T clone<T>() where T : AbstractCommand
    {
        AbstractCommand clone = new AbstractCommand(control, keyHold, keyDown);

        return (T)clone;
    }

    public virtual void execute(bool[] inputFlags)
    {
        if (inputFlags == null || inputFlags.Length != 2)
        {
            throw new UnityException("Calling command's execute (AbstractCommand's subclass) did not have the correct inputFlags as arguments (null or wrong length).");
        }

        keyHold = inputFlags[0];
        keyDown = inputFlags[1];
    }

    public virtual void execute(bool[] inputFlags, object data)
    {
        execute(inputFlags);
    }
}