using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticlePool {


    private GameManager gameManager;

    Dictionary<int, Queue<ObjectInstance>> poolDictionary = new Dictionary<int, Queue<ObjectInstance>>();

    public ParticlePool (GameManager gameManager)
    {
        this.gameManager = gameManager;
    }

    public void CreatePool(GameObject prefab, int poolSize)
    {
        int poolKey = prefab.GetInstanceID();

        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<ObjectInstance>());

            for (int i = 0; i < poolSize; ++i)
            {
                ObjectInstance newObject = new ObjectInstance(Object.Instantiate(prefab) as GameObject);
                poolDictionary[poolKey].Enqueue(newObject);
            }
        }
    }

    public void ReuseObject(GameObject prefab, Vector3 position)
    {
        int poolKey = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey))
        {
            ObjectInstance objectToReuse = poolDictionary[poolKey].Dequeue();
            poolDictionary[poolKey].Enqueue(objectToReuse);
            int angle = Random.Range(0, 360);
            Vector3 direction = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.right;
            objectToReuse.Reuse(position, direction);
        }
        else
        {
            throw new UnityException("NO POOL KEY");
        }
    }

    public class ObjectInstance
    {
        GameObject gameObject;
        bool hasPoolObjectComponent;
        PoolObject poolObjectScript;

        public ObjectInstance(GameObject objectInstance)
        {
            gameObject = objectInstance;
            gameObject.SetActive(false);

            if (gameObject.GetComponent<PoolObject>())
            {
                hasPoolObjectComponent = true;
                poolObjectScript = gameObject.GetComponent<PoolObject>();
            }
        }

        public void Reuse(Vector3 position, Vector2 direction)
        {
            ResetObjectPhysics(); //resets the object's physics
            gameObject.SetActive(true);
            gameObject.transform.position = position; // gives the object correct spawn 

            if (hasPoolObjectComponent)
            {
                poolObjectScript.OnObjectReuse();
                poolObjectScript.FireMe(direction);
            }
        }

        public void ResetObjectPhysics()
        {
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
            gameObject.GetComponent<Rigidbody2D>().angularVelocity = 0.0f;
        }

    }
}
