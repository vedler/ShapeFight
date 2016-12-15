using UnityEngine;
using System.Collections;

public class GrenadeMover : AbsWeaponMover {

    public override void FireMe(Vector2 direction)
    {
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
        }
    }

    public override void OnObjectReuse()
    {

    }
}
