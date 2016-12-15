using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

[Serializable]
class ShootingCommand : AbstractCommand, ISerializable
{
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

    public ShootingCommand(ShootingCommand other) : base(other.control, other.keyHold, other.keyDown)
    {
        this.targetPos = other.targetPos;
    }

    public override void execute(bool[] inputFlags)
    {
        // Call AbstractCommand first to save the flags
        base.execute(inputFlags);

        targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        GameManager.getInstance().getInputManager().invokeInputGroupEvent(EInputGroup.ShootingInput, this.clone());
    }

    public new ShootingCommand clone() 
    {
        ShootingCommand command = new ShootingCommand(this);
        command.targetPos = targetPos;

        return command;
    }

    //Deserialization constructor.
    public ShootingCommand(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {

        //Get the values from info and assign them to the appropriate properties
        //EmpId = (int)info.GetValue("EmployeeId", typeof(int));
        //EmpName = (String)info.GetValue("EmployeeName", typeof(string));
        float tx = (float)info.GetValue("targetPosX", typeof(float));
        float ty = (float)info.GetValue("targetPosY", typeof(float));
        targetPos = new Vector2(tx, ty);
    }

    //Serialization function.
    public override void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        base.GetObjectData(info, ctxt);
        //You can use any custom name for your name-value pair. But make sure you
        // read the values with the same name. For ex:- If you write EmpId as "EmployeeId"
        // then you should read the same with "EmployeeId"

        /*public EInputControls control { get; set; }

    public bool keyHold { get; set; }
    public bool keyDown { get; set; }*/
        info.AddValue("targetPosX", targetPos.x);
        info.AddValue("targetPosY", targetPos.y);
    }

    // Pun stuff
    public new static byte[] Serialize(object customobject)
    {
        BinaryFormatter bin = new BinaryFormatter();

        MemoryStream ms = new MemoryStream();

        bin.Serialize(ms, (ShootingCommand)customobject);

        return ms.ToArray();
    }

    public new static ShootingCommand Deserialize(byte[] bytes)
    {
        BinaryFormatter bin = new BinaryFormatter();

        MemoryStream ms = new MemoryStream(bytes);
        //ms.Write(bytes, 0, bytes.Length);

        return (ShootingCommand)bin.Deserialize(ms);
    }

    public new static byte[] SerializeQueue(object customobject)
    {
        BinaryFormatter bin = new BinaryFormatter();

        MemoryStream ms = new MemoryStream();

        Queue<ShootingCommand> commands = (Queue<ShootingCommand>)customobject;

        /*foreach (var obj in commands)
        {
            bin.Serialize(ms, obj);
        }*/
        bin.Serialize(ms, commands);

        return ms.ToArray();
    }

    public new static Queue<ShootingCommand> DeserializeQueue(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return new Queue<ShootingCommand>();
        }

        BinaryFormatter bin = new BinaryFormatter();

        MemoryStream ms = new MemoryStream(bytes);
        //ms.Write(bytes, 0, bytes.Length);

        try
        {
            object obj = bin.Deserialize(ms);

            var queue = obj as Queue<ShootingCommand>;

            if (queue == null)
            {
                Photon.MonoBehaviour.print("ShootingCommand[] null");
                return new Queue<ShootingCommand>();
            }

            Queue<ShootingCommand> q = new Queue<ShootingCommand>(queue);

            return q;
        }
        catch (SerializationException e)
        {
            Photon.MonoBehaviour.print(e);
        }

        return new Queue<ShootingCommand>();
    }
}
 