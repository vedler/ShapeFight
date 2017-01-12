using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponManager {

    private GameManager gameManager;

    public WeaponSpawner spawner;

    Dictionary<int, Queue<ObjectInstance>> poolDictionary = new Dictionary<int, Queue<ObjectInstance>> ();

    // Use this for initialization
    public WeaponManager (GameManager gameManager)
    {
        // Init
        this.gameManager = gameManager;
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
                ObjectInstance newObject = new ObjectInstance(Object.Instantiate(prefab) as GameObject);
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
                ObjectInstance newObject = new ObjectInstance(PhotonNetwork.Instantiate(weaponName, new Vector3(1000, 1000, 0), new Quaternion(), 0));
                newObject.passSpawner(spawner);
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

        public void passSpawner(WeaponSpawner spawner)
        {
            gameObject.GetComponent<AbsWeaponMover>().spawner = spawner;
        }

        public ObjectInstance(GameObject objectInstance)
        {
            gameObject = objectInstance;

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
                mover.Remove();
                mover.Disable();
            }
            else
            {
                MonoBehaviour.print("No mover found on pool object creation.");
                //gameObject.SetActive(false);
                gameObject.transform.position = new Vector3(1000, 1000, 0);
            }
        }

        public void Reuse(Vector3 position, Quaternion rotation, Vector2 direction)
        {
            ResetObjectPhysics(); //resets the object's physics

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
