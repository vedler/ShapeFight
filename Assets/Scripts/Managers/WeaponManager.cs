﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponManager {

    public WeaponConfig bulletConfig;
    public WeaponConfig rocketConfig;
    public WeaponConfig pelletConfig;
    public WeaponConfig grenadeConfig;

    private GameManager gameManager;

    Dictionary<int, Queue<ObjectInstance>> poolDictionary = new Dictionary<int, Queue<ObjectInstance>> ();

    // Use this for initialization
    public WeaponManager (GameManager gameManager)
    {
        // Init
        this.gameManager = gameManager;

        bulletConfig = new WeaponConfig();
        //bulletConfig.sounds = GameObject.FindGameObjectWithTag("WeaponSoundsTag").GetComponents<AudioSource>();
        bulletConfig.damage = 20;
        bulletConfig.radius = 5;


        rocketConfig = new WeaponConfig();
        //rocketConfig.sounds = GameObject.FindGameObjectWithTag("WeaponSoundsTag").GetComponents<AudioSource>();
        rocketConfig.radius = 10;
        rocketConfig.damage = 30;


        pelletConfig = new WeaponConfig();
        //pelletConfig.sounds = GameObject.FindGameObjectWithTag("WeaponSoundsTag").GetComponents<AudioSource>();
        pelletConfig.damage = 15;
        pelletConfig.radius = 3;


        grenadeConfig = new WeaponConfig();
        //grenadeConfig.sounds = GameObject.FindGameObjectWithTag("WeaponSoundsTag").GetComponents<AudioSource>();
        grenadeConfig.damage = 20;
        grenadeConfig.radius = 15;


    }

    // Creates a pool with a certain size
    [System.Obsolete("CreatePool is deprecated, please use CreateNetworkPool instead.")]
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

    public void CreateNetworkPool(string weaponName, int poolSize)
    {
        int poolKey = weaponName.GetHashCode();

        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<ObjectInstance>());

            for (int i = 0; i < poolSize; ++i)
            {
                ObjectInstance newObject = new ObjectInstance(PhotonNetwork.Instantiate(weaponName, Vector3.zero, new Quaternion(), 0));
                poolDictionary[poolKey].Enqueue(newObject);
            }
        }
    }


    //Use this to reuse an object from the pool
    [System.Obsolete("ReuseObject is deprecated, please use ReuseNetworkObject instead.")]
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

    public void ReuseNetworkObject(string weaponName, Vector3 position, Quaternion rotation, Vector2 direction)
    {
        int poolKey = weaponName.GetHashCode();

        if (poolDictionary.ContainsKey(poolKey))
        {
            ObjectInstance objectToReuse = poolDictionary[poolKey].Dequeue();
            poolDictionary[poolKey].Enqueue(objectToReuse);

            objectToReuse.Reuse(position, rotation, direction);
        }
    }

    public class ObjectInstance {

        ProjectileSynchronizer sync;
        GameObject gameObject;
        bool hasPoolObjectComponent;
        PoolObject poolObjectScript;

        public ObjectInstance(GameObject objectInstance)
        {
            gameObject = objectInstance;
            gameObject.SetActive(false);

            sync = gameObject.GetComponent<ProjectileSynchronizer>();

            if (gameObject.GetComponent<PoolObject>() != null)
            {
                hasPoolObjectComponent = true;
                poolObjectScript = gameObject.GetComponent<PoolObject>();
            }

            AbsWeaponMover mover = gameObject.GetComponent<AbsWeaponMover>();
            if (mover != null)
            {
                mover.init();
            }
        }

        public void Reuse(Vector3 position, Quaternion rotation, Vector2 direction)
        {
            ResetObjectPhysics(); //resets the object's physics
            gameObject.SetActive(true); 
            gameObject.transform.position = position; // gives the object correct spawn parameters
            gameObject.transform.rotation = rotation; //
            sync.TriggerResetToPosition(position, Vector2.zero, rotation);

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
