using UnityEngine;
using System.Collections;
using System;

public class BulletMover : AbsWeaponMover
{
    public override void FireMe(Vector2 direction)
    {
        damage = 20;
        radius = 5;
        this.direction = direction;
        direction.Normalize();
        // TODO: Initial velocity based on player speed instead of percentage of max speed (player speed + some margin)
        GetComponent<Rigidbody2D>().velocity = direction * (maxSpeed);
        isFired = true;
    }

    public override void move()
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
}
