using UnityEngine;
using System.Collections;
using System;

public class PelletMover : AbsWeaponMover {

    public override void FireMe(Vector2 direction)
    {
        damage = 15;
        radius = 3;
        this.direction = direction;
        direction.Normalize();
        // TODO: Initial velocity based on player speed instead of percentage of max speed (player speed + some margin)
        GetComponent<Rigidbody2D>().velocity = direction * (maxSpeed - UnityEngine.Random.Range(0f, 5f));
        isFired = true;
    }

    public override void move()
    {
        if (isFired)
        {
            GetComponent<Rigidbody2D>().drag = (float)Math.Sqrt((GetComponent<Rigidbody2D>().velocity.magnitude)*0.05f);
            //GetComponent<Rigidbody2D>().AddForce(direction * -1f, ForceMode2D.Impulse);
        }
        if(GetComponent<Rigidbody2D>().drag < 1f)
        {
            hasEnded = true;
        }
    }

    public override void OnObjectReuse()
    {
        GetComponent<Rigidbody2D>().drag = 0;
        hasEnded = false;
        isFired = false;
    }
}
