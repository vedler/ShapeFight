using UnityEngine;
using System.Collections;
using System;

public class BulletMover : AbsWeaponMover
{
    public override void FireMe(Vector2 direction)
    { 
        this.direction = direction;
        direction.Normalize();

        rigidBody.velocity = direction * (maxSpeed * 0.5f);
        isFired = true;

        photonView.RPC("TriggerFireMe", PhotonTargets.All, direction);
    }

    public override void move()
    {

    }

    [PunRPC]
    public override void TriggerMove(PhotonMessageInfo info)
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

        rigidBody.velocity = direction * (maxSpeed * 0.5f);
        isFired = true;
    }

    public override void init()
    {

    }
}
