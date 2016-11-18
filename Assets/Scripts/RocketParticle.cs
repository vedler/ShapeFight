using UnityEngine;
using System.Collections;

public class RocketParticle : PoolObject
{

    private GameManager gm;

    [SerializeField]
    public float speed;

    [SerializeField]
    public float maxSpeed;

    private bool isFired;
    private Vector2 direction;
    private Vector2 startPosition;

    // Use this for initialization
    void Start()
    {
        gm = GameManager.getInstance();
    }

    void FixedUpdate()
    {
        if (isFired)
        {
            direction.Normalize();
            GetComponent<Rigidbody2D>().AddForce(direction * maxSpeed, ForceMode2D.Impulse);
        }

    }

    public override void FireMe(Vector2 direction)
    {
        //GetComponent<Rigidbody2D>().AddForce(direction * speed, ForceMode2D.Impulse);
        this.direction = direction;
        isFired = true;
        gameObject.SetActive(true);
        StartCoroutine(ParticleLife());

    }

    public void SetStartPosition(Vector2 sp)
    {
        startPosition = sp;
    }

    private IEnumerator ParticleLife()
    {
        for (int i = 0; i < 7; i++)
        {
            gameObject.transform.position = gameObject.transform.position + (Vector3)direction * speed * Time.deltaTime;
            yield return null;
        }
        isFired = false;
        gameObject.SetActive(false);
    }
}