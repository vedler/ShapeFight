using UnityEngine;
using System.Collections.Generic;

public class PlayerSynchronizer : Photon.MonoBehaviour {

    public struct NetworkInfo
    {
        public Vector2 positon;
        public Vector2 velocity;
        public float timestamp;
    }

    public struct ReconciliationInfo
    {
        public NetworkInfo? localHistoryPosition;
        public NetworkInfo serverPosition;
        public float lerpStartTime;
        public float lerpDuration;
    }

    public struct CommandInformation
    {
        public float timestamp;
        public long tickNum;
    }

    // GetComponent<Rigidbody2D>()

    private float lastSynchronizationTime = 0f;
    private float syncDelay = 0f;
    private float syncTime = 0f;
    private Vector2 syncStartPosition = Vector2.zero;
    private Vector2 syncEndPosition = Vector2.zero;
    private Vector2 syncStartVelocity = Vector2.zero;
    private Vector2 syncEndVelocity = Vector2.zero;

    private Rigidbody2D rigidBody;
    private Collider2D collider;

    // Client
    private float lastCommandsSentTime;
    private Queue<NetworkInfo> clientSnapshots;

    // Server
    private Queue<CommandInformation> receivedCommandInfo;

    private PlayerCharacter parentCharacter;

    //public bool wasMine { get; private set; }

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();

        parentCharacter = GetComponent<PlayerCharacter>();
        receivedCommandInfo = new Queue<CommandInformation>();

        clientSnapshots = new Queue<NetworkInfo>();
        lastCommandsSentTime = Time.realtimeSinceStartup;

    }

    void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //print("(PV) This: " + Time.realtimeSinceStartup + ", Last: " + lastCommandsSentTime);

        // If the object doesnt have the correct components, set them up
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        if (collider == null)
        {
            collider = GetComponent<Collider2D>();
        }

        // We are the host
        if (PhotonNetwork.isMasterClient)
        {
            //print("We are the host");
            
            if (!photonView.isMine)
            {
                print("Attempting to request ownership");
                photonView.RequestOwnership();
                return;
            }

            if (stream.isWriting)
            {
                // We attempt to send the last received and parsed time from RPC command call
                // We used the time from that and the current position and velocity to send the info back
                // Another possibility is to also cache all of the forces given this frame and send them too (pre-position, pre-velocity and applied forces)
                //      This could make the packets too large, however

                // Another thing we have to make sure, is that we always stick to fixedupdate timesteps when sending and receiving these changes
                // We can definitely make sure we do the RPC calls from fixeduptade
                // But we also have to send the results within fixedupdate

                // Nullable
                CommandInformation? cmdInfo = null;

                while (receivedCommandInfo.Count > 0)
                {
                    if (receivedCommandInfo.Peek().tickNum > parentCharacter.numberOfTicks)
                    {
                        CommandInformation newInfo = receivedCommandInfo.Dequeue();

                        // Make sure we wait for the physics to solve before we send the positional information
                        if (receivedCommandInfo.Count > 0 && receivedCommandInfo.Peek().tickNum > parentCharacter.numberOfTicks)
                        {
                            // If we saved a bunch in the preceding frames, send the latest we can
                            continue;
                        }
                        else
                        {
                            cmdInfo = newInfo;
                        }
                    }
                }


                if (cmdInfo != null)
                {
                    print("(SEND) Time: " + cmdInfo.Value.timestamp);
                    stream.SendNext(rigidBody.position);
                    stream.SendNext(rigidBody.velocity);
                    stream.SendNext(cmdInfo.Value.timestamp);
                }
                else
                {
                    print("(XX) Time: " + -1);
                    stream.SendNext(rigidBody.position);
                    stream.SendNext(rigidBody.velocity);
                    stream.SendNext(-1.0f);
                }
                
            }

            /*// Receive info
            else
            {
                Vector2 syncPosition = (Vector2)stream.ReceiveNext();
                Vector2 syncVelocity = (Vector2)stream.ReceiveNext();

                syncTime = 0f;
                syncDelay = Time.time - lastSynchronizationTime;
                lastSynchronizationTime = Time.time;

                syncEndPosition = syncPosition + syncVelocity * syncDelay;
                syncEndVelocity = syncVelocity;
                syncStartPosition = rigidBody.position;
                syncStartVelocity = rigidBody.velocity;
            }*/
        }

        // We are not the host
        else
        {
            print("We are not the host");

            if (photonView.isMine)
            {
                print("Attempting to give ownership");
                photonView.TransferOwnership(PhotonNetwork.masterClient);
                return;
            }

            /*if (stream.isWriting)
            {
                stream.SendNext(rigidBody.position);
                stream.SendNext(rigidBody.velocity);
            }

            // Receive info
            else
            {*/
            // Only trust the information from the server
            if (!info.sender.isMasterClient)
            {
                print("ignoring some info");
                return;
            }

            // Now instead we are sent a timestamp and two vectors to be used
            // Instead of applying them here, we go back to the history of our positions/velocities
            // calculate the delta and apply Lerp with the time of 0.5x the ping

            // Also when applying this Lerp, we check the raycast if the delta of our positions would hit anything
            // If it would, we instantly snap to the delta position (this could bring in some really huge bugs - getting stuck in walls or something)
            //      but to alleviate this problem, we can also make our ping cap at like 500ms

            Vector2 syncPosition = (Vector2)stream.ReceiveNext();
            Vector2 syncVelocity = (Vector2)stream.ReceiveNext();
            float timestamp = (float)stream.ReceiveNext();

            syncTime = 0f;
            syncDelay = Time.realtimeSinceStartup - lastSynchronizationTime;
            lastSynchronizationTime = Time.realtimeSinceStartup;

            syncEndPosition = syncPosition + syncVelocity * syncDelay;
            syncEndVelocity = syncVelocity;
            syncStartPosition = rigidBody.position;
            syncStartVelocity = rigidBody.velocity;

            // We control this character, lets get some reconciliation information
            if (parentCharacter.wasMine)
            {
                // Save the info the server sent
                NetworkInfo serverInfo = new NetworkInfo();
                serverInfo.positon = syncPosition;
                serverInfo.velocity = syncVelocity;
                serverInfo.timestamp = timestamp;

                // Create the reconciliation data
                ReconciliationInfo recInfo = new ReconciliationInfo();
                recInfo.serverPosition = serverInfo;

                // Save current time and ping (with a 5% margin)
                recInfo.lerpStartTime = Time.realtimeSinceStartup;
                recInfo.lerpDuration = syncDelay * 0.95f;

                // Nullable, because we might not find the client info
                NetworkInfo? clientInfo = null;

                while (clientSnapshots.Count > 0)
                {
                    if (clientSnapshots.Peek().timestamp > timestamp)
                    {
                        break;
                    }

                    clientInfo = clientSnapshots.Dequeue();
                }

                // Save the position from the local history, can be null if nothing was found
                recInfo.localHistoryPosition = clientInfo;
            }
            //}
        }
        
    }

    void FixedUpdate()
    {
        if (rigidBody == null)
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        if (collider == null)
        {
            collider = GetComponent<Collider2D>();
        }

       /* if (parentCharacter.wasMine)
        {
            print("(FU) This: " + Time.realtimeSinceStartup + ", Last: " + lastCommandsSentTime);
            // Save state
            SaveCurrentSnapshot();

            // We need to reconciliate, because we are not the host
            SyncedExternalMovement();

            sendInputData();
        }
        else*/ 
        
        if (!parentCharacter.wasMine && !photonView.isMine)
        {
            SyncedExternalMovement();
        }
        else if (PhotonNetwork.isMasterClient)
        {

        }
    }

    private void SaveCurrentSnapshot()
    {
        NetworkInfo info = new NetworkInfo();
        info.positon = rigidBody.position;
        info.velocity = rigidBody.velocity;
        info.timestamp = lastCommandsSentTime;
        clientSnapshots.Enqueue(info);
    }

    /**
     * This method takes care of the movement of all the other players in the game.
     * 
     */
    private void SyncedExternalMovement()
    {
        syncTime += Time.fixedDeltaTime;
        //GetComponent<Rigidbody2D>().position = Vector2.Lerp(syncStartPosition, syncEndPosition, syncTime / syncDelay);
        //GetComponent<Rigidbody2D>().velocity = Vector2.Lerp(syncStartVelocity, syncEndVelocity, syncTime / syncDelay);

        RaycastHit hit;

        // Get movement direction vector
        Vector2 rayDir = rigidBody.velocity;
        rayDir.Normalize();

        // Get diagonal size of the square
        Vector2 diagVec = collider.bounds.size;

        // Layer mask to only detect hits with the map layer
        // TODO: Player layer
        var layerMask = (1 << LayerMask.NameToLayer("MapLayer"));

        bool collidingSoon = false;
        Vector2 collisionTransform = Vector2.zero;

        if (Physics.Raycast(transform.position, rayDir, out hit, diagVec.magnitude * 3, layerMask))
        {
            Vector3 colliderEdgePoint = collider.bounds.ClosestPoint(hit.point);
            print("closest to coll: " + colliderEdgePoint);
            collidingSoon = Vector2.Distance(colliderEdgePoint, transform.position) * 2 >= Vector2.Distance(hit.point, transform.position);

            // Calculate the center of the transform
            if (collidingSoon)
            {
                print("Colliding soon");
                collisionTransform = hit.point - colliderEdgePoint + transform.position;
            }
        }

        if (collidingSoon)
        {
            rigidBody.position = Vector2.Lerp(syncStartPosition, collisionTransform, Mathf.Pow(syncTime / syncDelay, 2));
        } 
        else
        {
            rigidBody.position = Vector2.Lerp(syncStartPosition, syncEndPosition, Mathf.Pow(syncTime / syncDelay, 2));
        }

        
        rigidBody.velocity = Vector2.Lerp(syncStartVelocity, syncEndVelocity, Mathf.Pow(syncTime / syncDelay, 2));
    }

    void ReconciliatedMovement()
    {

    }

    // Clientside
    public void sendInputData()
    {
        // TODO: Get the input cache from the state handler and send the input lists to the master client with RPC
        if (!PhotonNetwork.isMasterClient)
        {
            // Send it
            print("(SI) Time: " + Time.realtimeSinceStartup);
            lastCommandsSentTime = Time.realtimeSinceStartup;

            Dictionary<byte, Queue<ICommand>> commands = parentCharacter.movementStateHandler.finalizedCache;
            //photonView.RPC("ReceiveClientInputData", PhotonTargets.MasterClient, lastCommandsSentTime, commands);

            // Begin
            photonView.RPC("StartClientInputSending", PhotonTargets.MasterClient, lastCommandsSentTime);

            Queue<AbstractCommand> abs;
            Queue<MoveCommand> move;
            Queue<ShootingCommand> shoot;

            foreach (var type in commands.Keys)
            {
                Queue<AbstractCommand> cAbs = new Queue<AbstractCommand>();
                Queue<MoveCommand> cMove = new Queue<MoveCommand>();
                Queue<ShootingCommand> cShoot = new Queue<ShootingCommand>();

                Queue<ICommand> cloneCommands = new Queue<ICommand>(commands[type]);

                while(cloneCommands.Count > 0)
                {
                    ICommand comm = cloneCommands.Dequeue();

                    if (comm is MoveCommand)
                    {
                        cMove.Enqueue((MoveCommand)comm);
                    }
                    else if (comm is ShootingCommand)
                    {
                        cShoot.Enqueue((ShootingCommand)comm);
                    }
                    else
                    {
                        cAbs.Enqueue((AbstractCommand)comm);
                    }
                }

                byte[] moveBytes = MoveCommand.SerializeQueue(cMove);
                byte[] shootBytes = ShootingCommand.SerializeQueue(cShoot);
                byte[] absBytes = AbstractCommand.SerializeQueue(cAbs);

                photonView.RPC("SendClientInput", PhotonTargets.MasterClient, type, CommandClass.Move, moveBytes);
                photonView.RPC("SendClientInput", PhotonTargets.MasterClient, type, CommandClass.Shoot, shootBytes);
                photonView.RPC("SendClientInput", PhotonTargets.MasterClient, type, CommandClass.Abs, absBytes);
            }


            photonView.RPC("FinishClientInputSending", PhotonTargets.MasterClient);

            // Actual reconciliation code would save the input data here, but we use deltas to calculate the actual position
        }
    }





    // ------------------------ SERVER LOGIC ------------------------------------------

    public class NetworkInputData
    {
        public Dictionary<byte, Queue<ICommand>> commands;
        public float timestamp;

        public NetworkInputData(float timestamp)
        {
            commands = new Dictionary<byte, Queue<ICommand>>();
            commands.Add((byte)PMovementStateHandler.ECommandType.Up, new Queue<ICommand>());
            commands.Add((byte)PMovementStateHandler.ECommandType.Down, new Queue<ICommand>());
            commands.Add((byte)PMovementStateHandler.ECommandType.Hold, new Queue<ICommand>());
        }
    }

    NetworkInputData networkInputData;

    private enum CommandClass : byte
    {
        Abs = 0,
        Move,
        Shoot
    }

    [PunRPC] 
    public void StartClientInputSending(float timestamp, PhotonMessageInfo info)
    {
        networkInputData = new NetworkInputData(timestamp);
    }

    [PunRPC]
    public void SendClientInput(byte cmdType, byte cmdClass, byte[] commandData, PhotonMessageInfo info)
    {
        Queue<ICommand> commands = new Queue<ICommand>();

        if (cmdClass == (byte)CommandClass.Move)
        {
            //commands = MoveCommand.DeserializeQueue(commandData).Cast<
            //Queue<ICommand> commands = MoveCommand.DeserializeQueue(commandData).Co

            foreach (var comm in MoveCommand.DeserializeQueue(commandData))
            {
                networkInputData.commands[cmdType].Enqueue(comm);
            }
        }
        else if (cmdClass == (byte)CommandClass.Shoot)
        {
            //commands = MoveCommand.DeserializeQueue(commandData).Cast<
            //Queue<ICommand> commands = MoveCommand.DeserializeQueue(commandData).Co

            foreach (var comm in ShootingCommand.DeserializeQueue(commandData))
            {
                networkInputData.commands[cmdType].Enqueue(comm);
            }
        }
        else
        {
            foreach (var comm in AbstractCommand.DeserializeQueue(commandData))
            {
                networkInputData.commands[cmdType].Enqueue(comm);
            }
        }
    }

    [PunRPC]
    public void FinishClientInputSending(PhotonMessageInfo info)
    {
        if (info.photonView.viewID != this.photonView.viewID)
        {
            print("yo mismatch with views");
        }
        print("(RI) TimeS: " + networkInputData.timestamp + ", Size: " + (networkInputData.commands[(byte)PMovementStateHandler.ECommandType.Down].Count + networkInputData.commands[(byte)PMovementStateHandler.ECommandType.Hold].Count + networkInputData.commands[(byte)PMovementStateHandler.ECommandType.Up].Count));


        // Mimic InputManager logic
        foreach (byte type in networkInputData.commands.Keys)
        {
            Queue<ICommand> queue = networkInputData.commands[type];

            while (queue.Count > 0)
            {
                ICommand command = queue.Dequeue();

                if (command is MoveCommand)
                {
                    parentCharacter.movementStateHandler.addCommand((PMovementStateHandler.ECommandType)type, command);
                }
            }
        }

        CommandInformation commandInfo = new CommandInformation();
        commandInfo.tickNum = 
            parentCharacter.numberOfTicks;
        commandInfo.timestamp = networkInputData.timestamp;

        receivedCommandInfo.Enqueue(commandInfo);
    }

    [PunRPC]
    public void ReceiveClientInputData(float timestamp, Dictionary<byte, Queue<ICommand>> commands, PhotonMessageInfo info)
    {
        if (info.photonView.viewID != this.photonView.viewID)
        {
            print("yo mismatch with views");
        }
        print("(RI) TimeS: " + timestamp + ", Size: " + (commands[(byte)PMovementStateHandler.ECommandType.Down].Count + commands[(byte)PMovementStateHandler.ECommandType.Hold].Count + commands[(byte)PMovementStateHandler.ECommandType.Up].Count));


        // Mimic InputManager logic
        foreach (byte type in commands.Keys) {
            Queue<ICommand> queue = commands[type];

            while (queue.Count > 0)
            {
                ICommand command = queue.Dequeue();

                if (command is MoveCommand)
                {
                    parentCharacter.movementStateHandler.addCommand((PMovementStateHandler.ECommandType)type, command);
                }
            }
        }

        CommandInformation commandInfo = new CommandInformation();
        commandInfo.tickNum = parentCharacter.numberOfTicks;
        commandInfo.timestamp = timestamp;

        receivedCommandInfo.Enqueue(commandInfo);
    }
}
