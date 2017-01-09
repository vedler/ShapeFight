﻿using UnityEngine;
using System.Collections;
using System;

public class BulletMover : AbsWeaponMover
{
    public override void FireMe(Vector2 direction)
    { 
        this.direction = direction;
        direction.Normalize();
        // TODO: Initial velocity based on player speed instead of percentage of max speed (player speed + some margin)
        rigidBody.velocity = direction * (maxSpeed * 0.5f);
        isFired = true;

        photonView.RPC("TriggerFireMe", PhotonTargets.All, direction);
    }

    public override void move()
    {
        if (isFired)
        {
            //GetComponent<Rigidbody2D>().AddForce(direction * speed, ForceMode2D.Impulse);
            //print("mag: " + GetComponent<Rigidbody2D>().velocity.magnitude);
        }
    }

    [PunRPC]
    public override void TriggerMove(PhotonMessageInfo info)
    {
        if (isFired)
        {
            //GetComponent<Rigidbody2D>().AddForce(direction * speed, ForceMode2D.Impulse);
            //print("mag: " + GetComponent<Rigidbody2D>().velocity.magnitude);
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
        rigidBody.velocity = direction * (maxSpeed * 0.5f);
        isFired = true;
    }

    public override void init()
    {
        //this.activeConfig = GameManager.getInstance().getWeaponManager().bulletConfig;
    }
}
