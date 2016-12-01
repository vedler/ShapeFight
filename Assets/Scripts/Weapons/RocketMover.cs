﻿using UnityEngine;
using System.Collections;
using System;

public class RocketMover : AbsWeaponMover {

    public override void FireMe(Vector2 direction)
    {
        this.direction = direction;
        direction.Normalize();
        // TODO: Initial velocity based on player speed instead of percentage of max speed (player speed + some margin)
        GetComponent<Rigidbody2D>().velocity = direction * (maxSpeed * 0.5f);
        isFired = true;
    }

    public override void move()
    {
        if (isFired)
        { 
            GetComponent<Rigidbody2D>().AddForce(direction * speed, ForceMode2D.Impulse);
            //print("mag: " + GetComponent<Rigidbody2D>().velocity.magnitude);
        }
    }

    public override void OnObjectReuse()
    {
        speed = 0.7f;
        maxSpeed = 50.0f;
    }
}