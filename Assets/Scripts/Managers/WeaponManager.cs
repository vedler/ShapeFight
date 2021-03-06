﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponManager {

    private GameManager gameManager;

    Dictionary<int, Queue<ObjectInstance>> poolDictionary = new Dictionary<int, Queue<ObjectInstance>> ();

    // Use this for initialization
    public WeaponManager (GameManager gameManager)
    {
        // Init
        this.gameManager = gameManager;

    }

    // Creates a pool with a certain size
	public void CreatePool(GameObject prefab, int poolSize)
    {
        int poolKey = prefab.GetInstanceID();

        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<ObjectInstance>());

            for (int i = 0; i < poolSize; ++i)
            {
                ObjectInstance newObject = new ObjectInstance (Object.Instantiate(prefab) as GameObject);
                poolDictionary[poolKey].Enqueue(newObject);
            }
        }
    }


    //Use this to reuse an object from the pool
    public void ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector2 direction)
    {
        int poolKey = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey))
        {
            ObjectInstance objectToReuse = poolDictionary[poolKey].Dequeue();
            poolDictionary[poolKey].Enqueue(objectToReuse);

            objectToReuse.Reuse(position, rotation, direction);
        }
    }

    public class ObjectInstance{

        GameObject gameObject;
        bool hasPoolObjectComponent;
        PoolObject poolObjectScript;

        public ObjectInstance(GameObject objectInstance)
        {
            gameObject = objectInstance;
            gameObject.SetActive(false);

            if (gameObject.GetComponent<PoolObject>() != null)
            {
                hasPoolObjectComponent = true;
                poolObjectScript = gameObject.GetComponent<PoolObject>();
            }
        }

        public void Reuse(Vector3 position, Quaternion rotation, Vector2 direction)
        {
            ResetObjectPhysics(); //resets the object's physics
            gameObject.SetActive(true); 
            gameObject.transform.position = position; // gives the object correct spawn parameters
            gameObject.transform.rotation = rotation; //
            
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
