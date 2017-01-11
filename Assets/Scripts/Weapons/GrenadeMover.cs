using UnityEngine;
using System.Collections;
using System;

public class GrenadeMover : AbsWeaponMover {

    public override void FireMe(Vector2 direction)
    {
        //damage = 20;
        //radius = 15;

        this.direction = direction;
        direction.Normalize();
        // TODO: Initial velocity based on player speed instead of percentage of max speed (player speed + some margin)
        rigidBody.velocity = direction * (maxSpeed);
        isFired = true;

        photonView.RPC("TriggerFireMe", PhotonTargets.All, direction);
    }

    public override void move()
    {
        if (isFired)
        {
        }
    }

    public override void OnObjectReuse()
    {

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
        throw new NotImplementedException();
    }

    public override void init()
    {
        //this.activeConfig = GameManager.getInstance().getWeaponManager().grenadeConfig;
    }
}

