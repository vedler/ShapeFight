using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;

[Serializable]
class AbstractCommand : ICommand, ISerializable
{
    public EInputControls control { get; set; }

    public bool keyHold { get; set; }
    public bool keyDown { get; set; }

    protected AbstractCommand(EInputControls control) : this(control, false, false)
    {
    }

    protected AbstractCommand(EInputControls control, bool keyHold, bool keyDown)
    {
        this.control = control;
        this.keyHold = keyHold;
        this.keyDown = keyDown;
    }

    protected AbstractCommand(AbstractCommand other) : this(other.control, other.keyHold, other.keyDown)
    {
    }


    public virtual ICommand clone()
    {
        return new AbstractCommand(control, keyHold, keyDown);
    }

    // Clone with correct type
    public virtual T clone<T>() where T : AbstractCommand
    {
        //AbstractCommand clone = new AbstractCommand(control, keyHold, keyDown);

        return (T)this.clone();
    }

    public virtual void execute(bool[] inputFlags)
    {
        if (inputFlags == null || inputFlags.Length != 2)
        {
            throw new UnityException(this.GetType() + ": Calling command's execute (AbstractCommand's subclass) did not have the correct inputFlags as arguments (null or wrong length).");
        }

        keyHold = inputFlags[0];
        keyDown = inputFlags[1];
    }

    public virtual void execute(bool[] inputFlags, object data)
    {
        execute(inputFlags);
    }

    public override bool Equals(object obj)
    {
        if (!(obj is AbstractCommand))
        {
            return false;
        }

        return control == ((AbstractCommand)obj).control;
    }

    // SERIALIZATION

    //Deserialization constructor.
    public AbstractCommand(SerializationInfo info, StreamingContext ctxt)
    {
        //Get the values from info and assign them to the appropriate properties
        control = (EInputControls)info.GetValue("control", typeof(int));
        keyHold = (bool)info.GetValue("keyHold", typeof(bool));
        keyDown = (bool)info.GetValue("keyDown", typeof(bool));
    }

    //Serialization function.
    public virtual void GetObjectData(SerializationInfo info, StreamingContext ctxt)
    {
        info.AddValue("control", (byte)control);
        info.AddValue("keyHold", keyHold);
        info.AddValue("keyDown", keyDown);
    }

    // Pun stuff
    public static byte[] Serialize(object customobject)
    {
        BinaryFormatter bin = new BinaryFormatter();

        MemoryStream byteStream = new MemoryStream();

        bin.Serialize(byteStream, (AbstractCommand)customobject);

        return byteStream.ToArray();
    }

    public static AbstractCommand Deserialize(byte[] bytes)
    {
        BinaryFormatter bin = new BinaryFormatter();
        
        MemoryStream ms = new MemoryStream(bytes);
        //ms.Write(bytes, 0, bytes.Length);

        return (AbstractCommand)bin.Deserialize(ms);
    }

    public new static byte[] SerializeQueue(object customobject)
    {
        BinaryFormatter bin = new BinaryFormatter();

        MemoryStream ms = new MemoryStream();

        Queue<AbstractCommand> commands = (Queue<AbstractCommand>)customobject;

        /*foreach (var obj in commands)
        {
            bin.Serialize(ms, obj);
        }*/

        bin.Serialize(ms, commands);

        return ms.ToArray();
    }

    public new static Queue<AbstractCommand> DeserializeQueue(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return new Queue<AbstractCommand>();
        }

        BinaryFormatter bin = new BinaryFormatter();

        MemoryStream ms = new MemoryStream(bytes);
        //ms.Write(bytes, 0, bytes.Length);

        try
        {
            object obj = bin.Deserialize(ms);

            var queue = obj as Queue<AbstractCommand>;

            if (queue == null)
            {
                Photon.MonoBehaviour.print("AbstractCommand[] null");
                return new Queue<AbstractCommand>();
            }

            Queue<AbstractCommand> q = new Queue<AbstractCommand>(queue);

            return q;
        }
        catch (SerializationException e)
        {
            Photon.MonoBehaviour.print(e);
        }

        return new Queue<AbstractCommand>();
        
    }
}