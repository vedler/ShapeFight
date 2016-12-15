using UnityEngine;
using System.Collections;
using System;

public class RocketMover : AbsWeaponMover {

    [SerializeField]
    public float acceleration;

    public override void FireMe(Vector2 direction)
    {
        this.direction = direction;
        radius = 10;
        damage = 30;
        direction.Normalize();
        // TODO: Initial velocity based on player speed instead of percentage of max speed (player speed + some margin)
        GetComponent<Rigidbody2D>().velocity = direction * (maxSpeed * 0.5f);
        isFired = true;
    }

    public override void move()
    {
        if (isFired)
        { 
            GetComponent<Rigidbody2D>().AddForce(direction * acceleration, ForceMode2D.Impulse);
            //print("mag: " + GetComponent<Rigidbody2D>().velocity.magnitude);
        }
    }

    public override void OnObjectReuse()
    {
        //acceleration = 0.7f;
        //maxSpeed = 50.0f;
    }
}
