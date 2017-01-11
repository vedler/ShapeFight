using UnityEngine;
using System.Collections;
using System;

public class GrenadeMover : AbsWeaponMover {

    public override void FireMe(Vector2 direction)
    {
        this.direction = direction;
        direction.Normalize();

        rigidBody.velocity = direction * (maxSpeed);
        isFired = true;

        photonView.RPC("TriggerFireMe", PhotonTargets.All, direction);
    }

    public override void move()
    {

    }

    public override void OnObjectReuse()
    {
        isFired = false;
        hasEnded = false;
    }

    [PunRPC]
    public override void TriggerFireMe(Vector2 direction, PhotonMessageInfo info)
    {
        this.direction = direction;
        direction.Normalize();
        // TODO: Initial velocity based on player speed instead of percentage of max speed (player speed + some margin)
        rigidBody.velocity = direction * (maxSpeed);
        isFired = true;
    }

    public override void TriggerMove(PhotonMessageInfo info)
    {
        
    }

    public override void init()
    {

    }
}

