﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PlayerDeadState : AbstractPMovementState
{
    public enum EDeadReason : byte
    {
        Joined = 0,
        Died
    }

    private EDeadReason deadReason;
    private float timeStartedDead;

    private readonly float RESPAWN_TIME = 5.0f;
    private readonly float SPAWN_TIME = 5.0f;
    private readonly float SPAWN_CLEAR_RADIUS = 8.0f;

    

    public PlayerDeadState(PMovementStateHandler handler, EDeadReason reason) : base(handler)
    {
        deadReason = reason;
        setNextState(new PlayerFlyingState(handler, false));
    }

    public override bool check()
    {
        return true;
    }

    public override void enter()
    {
        if (!handler.playerCharacter.wasMine)
        {
            return;
        }

        timeStartedDead = Time.realtimeSinceStartup;
        handler.playerCharacter.Disable();

        if (handler.playerCharacter.spawnPoints.Length == 0)
        {
            handler.playerCharacter.resetForSpawn(Vector2.zero);
            return;
        }

        // Find random spawnpoint ID
        int randId = UnityEngine.Random.Range(0, handler.playerCharacter.spawnPoints.Length - 1);
        Vector2 pos = handler.playerCharacter.spawnPoints[randId];
        
        int layerMask = LayerMask.GetMask("LocalPlayerTag");

        Collider2D[] collisions = Physics2D.OverlapCircleAll(pos, SPAWN_CLEAR_RADIUS, layerMask, 0);

        if (collisions.Length > 0)
        {
            Vector2[] spawns = (Vector2[])handler.playerCharacter.spawnPoints.Clone();

            while (collisions.Length > 0 && spawns.Length > 1)
            {
                // Remove the last selected element from the array
                if (randId == 0)
                {
                    spawns = Slice(spawns, 1, spawns.Length - randId);
                }
                else if (randId == handler.playerCharacter.spawnPoints.Length - 1)
                {
                    spawns = Slice(spawns, 0, randId);
                }
                else
                {
                    // Remove one element from the array
                    Vector2[] lower = Slice(spawns, 0, randId);
                    Vector2[] upper = Slice(spawns, randId + 1, spawns.Length - randId);
                    spawns = new Vector2[lower.Length + upper.Length];
                    lower.CopyTo(spawns, 0);
                    upper.CopyTo(spawns, lower.Length);
                }

                randId = UnityEngine.Random.Range(0, handler.playerCharacter.spawnPoints.Length - 1);
                collisions = Physics2D.OverlapCircleAll(pos, SPAWN_CLEAR_RADIUS, layerMask, 0);
            }

            // We found our spawn, we good
            if (spawns.Length == 1)
            {
                pos = spawns[0];
            }
            else
            {
                pos = spawns[randId];
            }
        }
        
        handler.playerCharacter.resetForSpawn(pos);
    }

    public override void exit()
    {
        if (!handler.playerCharacter.wasMine)
        {
            return;
        }

        handler.playerCharacter.Enable();
        handler.playerCharacter.updateFuelText();
        handler.playerCharacter.updateHealthText();
        handler.playerCharacter.playerRespawn();
    }

    public override bool FixedUpdate(ref Dictionary<byte, Queue<ICommand>> commandCache)
    {
        if (!handler.playerCharacter.wasMine)
        {
            if (handler.playerCharacter.isEnabled())
            {
                commandCache = new Dictionary<byte, Queue<ICommand>>();
                handler.setCacheUsed();
                return false;
            }

            return true;
        }

        switch (deadReason)
        {
            case EDeadReason.Joined:
                if (Time.realtimeSinceStartup - timeStartedDead > SPAWN_TIME)
                {
                    commandCache = new Dictionary<byte, Queue<ICommand>>();
                    handler.setCacheUsed();
                    return false;
                }
                break;
            case EDeadReason.Died:
                if (Time.realtimeSinceStartup - timeStartedDead > RESPAWN_TIME)
                {
                    commandCache = new Dictionary<byte, Queue<ICommand>>();
                    handler.setCacheUsed();
                    return false;
                }
                break;
            default:
                break;
        }

        return true;
    }

    public static T[] Slice<T>(T[] source, int start, int end)
    {
        // Handles negative ends.
        if (end < 0)
        {
            end = source.Length + end;
        }
        int len = end - start;

        // Return new array.
        T[] res = new T[len];
        for (int i = 0; i < len; i++)
        {
            res[i] = source[i + start];
        }
        return res;
    }

    
}