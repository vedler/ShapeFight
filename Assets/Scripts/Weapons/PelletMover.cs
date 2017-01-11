using UnityEngine;
using System.Collections;
using System;

public class PelletMover : AbsWeaponMover {

    public override void FireMe(Vector2 direction)
    {
        this.direction = direction;
        direction.Normalize();

        rigidBody.velocity = direction * (maxSpeed - UnityEngine.Random.Range(0f, maxSpeed * 0.2f));

        isFired = true;

        photonView.RPC("TriggerFireMe", PhotonTargets.All, direction);
    }

    public override void move()
    {
        if (isFired)
        {
            rigidBody.drag = (float)Math.Sqrt((rigidBody.velocity.magnitude) * 0.05f);
        }

        if (rigidBody.drag < 1f)
        {
            hasEnded = true;
        }

        photonView.RPC("TriggerMove", PhotonTargets.All);
    }

    public override void OnObjectReuse()
    {
        rigidBody.drag = 0;
        hasEnded = false;
        isFired = false;
    }

    [PunRPC]
    public override void TriggerFireMe(Vector2 direction, PhotonMessageInfo info)
    {
        this.direction = direction;
        direction.Normalize();

        rigidBody.velocity = direction * (maxSpeed - UnityEngine.Random.Range(0f, 5f));
        isFired = true;
    }

    [PunRPC]
    public override void TriggerMove(PhotonMessageInfo info)
    {
        if (isFired)
        {
            rigidBody.drag = (float)Math.Sqrt((rigidBody.velocity.magnitude) * 0.05f);
        }
        if (rigidBody.drag < 1f)
        {
            hasEnded = true;
        }
    }

    public override void init()
    {

    }
}
