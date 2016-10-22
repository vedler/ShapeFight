using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class ShootingCommand : AbstractCommand
{
    public float mouseX { get; private set; }
    public float mouseY { get; private set; }
    public Vector2 targetPos { get; private set; }


    public ShootingCommand(EInputControls control) : base(control)
    {
        /* private Vector3 mouse_pos;
        public Transform target;
        private Vector3 object_pos;
        private float angle; */
        //mouseX = 0;
        //mouseY = 0;
    }

    public override void execute(bool[] inputFlags)
    {
        // Call AbstractCommand first to save the flags
        base.execute(inputFlags);

        MonoBehaviour.print("Sai commandi kätte");
        Vector2 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        GameManager.getInstance().getInputManager().invokeInputGroupEvent(EInputGroup.ShootingInput, this);
    }

}