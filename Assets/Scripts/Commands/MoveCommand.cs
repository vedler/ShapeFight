using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

[Serializable]
class MoveCommand : AbstractCommand, ISerializable
{
    // C# doesn't allow fully abstract constructors, constructor call to base class has to be made manually
    public MoveCommand(EInputControls control) : base(control)
    {
    }

    public MoveCommand(MoveCommand other) : base(other.control, other.keyHold, other.keyDown)
    {
    }

    public override void execute(bool[] inputFlags)
    {
        // Call AbstractCommand first to save the flags
        base.execute(inputFlags);

        // TODO: Add a clone of this command to the game's history data with a proper timestamp in milliseconds
        
        GameManager.getInstance().getInputManager().invokeInputGroupEvent(EInputGroup.MovementInput, this);
    }

    // SERIALIZATION

    public MoveCommand(SerializationInfo info, StreamingContext ctxt) : base(info, ctxt)
    {
    }

    public new static byte[] Serialize(object customobject)
    {
        BinaryFormatter bin = new BinaryFormatter();

        MemoryStream ms = new MemoryStream();

        bin.Serialize(ms, (MoveCommand)customobject);

        return ms.ToArray();
    }

    public new static MoveCommand Deserialize(byte[] bytes)
    {
        BinaryFormatter bin = new BinaryFormatter();

        MemoryStream ms = new MemoryStream(bytes);
        //ms.Write(bytes, 0, bytes.Length);

        return (MoveCommand)bin.Deserialize(ms);
    }

    public new static byte[] SerializeQueue(object customobject)
    {
        BinaryFormatter bin = new BinaryFormatter();

        MemoryStream ms = new MemoryStream();

        Queue<MoveCommand> commands = (Queue<MoveCommand>)customobject;

        /*foreach (var obj in commands)
        {
            bin.Serialize(ms, obj);
        }*/
        bin.Serialize(ms, commands);

        return ms.ToArray();
    }

    public new static Queue<MoveCommand> DeserializeQueue(byte[] bytes)
    {
        if (bytes.Length == 0)
        {
            return new Queue<MoveCommand>();
        }

        BinaryFormatter bin = new BinaryFormatter();

        MemoryStream ms = new MemoryStream(bytes);
        //ms.Write(bytes, 0, bytes.Length);

        try
        {
            object obj = bin.Deserialize(ms);

            var queue = obj as Queue<MoveCommand>;

            if (queue == null)
            {
                Photon.MonoBehaviour.print("MoveCommand[] null");
                return new Queue<MoveCommand>();
            }

            Queue<MoveCommand> q = new Queue<MoveCommand>(queue);

            return q;
        }
        catch (SerializationException e)
        {
            Photon.MonoBehaviour.print(e);
        }

        return new Queue<MoveCommand>();
    }

}