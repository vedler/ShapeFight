using UnityEngine;
using System.Collections;
using System;

public class PelletMover : AbsWeaponMover {

    public override void FireMe(Vector2 direction)
    {
        //damage = 15;
        //radius = 3;
        this.direction = direction;
        direction.Normalize();
        // TODO: Initial velocity based on player speed instead of percentage of max speed (player speed + some margin)
        rigidBody.velocity = direction * (maxSpeed - UnityEngine.Random.Range(0f, maxSpeed * 0.2f));
        isFired = true;

        photonView.RPC("TriggerFireMe", PhotonTargets.All, direction);
    }

    public override void move()
    {
        if (isFired)
        {
            rigidBody.drag = (float)Math.Sqrt((rigidBody.velocity.magnitude)*0.05f);
            //GetComponent<Rigidbody2D>().AddForce(direction * -1f, ForceMode2D.Impulse);
        }
        if(rigidBody.drag < 1f)
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
        // TODO: Initial velocity based on player speed instead of percentage of max speed (player speed + some margin)
        rigidBody.velocity = direction * (maxSpeed - UnityEngine.Random.Range(0f, 5f));
        isFired = true;
    }

    [PunRPC]
    public override void TriggerMove(PhotonMessageInfo info)
    {
        if (isFired)
        {
            rigidBody.drag = (float)Math.Sqrt((rigidBody.velocity.magnitude) * 0.05f);
            //GetComponent<Rigidbody2D>().AddForce(direction * -1f, ForceMode2D.Impulse);
        }
        if (rigidBody.drag < 1f)
        {
            hasEnded = true;
        }
    }

    public override void init()
    {
        //this.activeConfig = GameManager.getInstance().getWeaponManager().pelletConfig;
    }
}
