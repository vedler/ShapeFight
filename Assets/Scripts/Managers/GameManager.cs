using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // TODO: Object-pool based on an array and switch dead objects with the last active one instead of re-queueing


    private static GameManager instance;

    public static GameManager getInstance()
    {
        if (instance == null)
        {
            // Instead of having the GameManager in the scene, load it in as an object on getInstance()
            GameObject manager = new GameObject("[GameManager]");
            instance = manager.AddComponent<GameManager>();
            DontDestroyOnLoad(manager);
        }

        return instance;
    }

    private InputManager inputManager;

    public InputManager getInputManager()
    {
        if (inputManager == null)
        {
            throw new UnityException("GameManager's InputManager instance was accessed before Unity started it!");
        }

        return inputManager;
    }

    private WeaponManager weaponManager;

    public WeaponManager getWeaponManager()
    {
        if (weaponManager == null)
        {
            throw new UnityException("GameManager's WeaponManager instance was accessed before Unity started it!");
        }
            return weaponManager;
    }

    // Use this for initialization
    void Awake()
    {
        // Set the singleton instance
        GameManager.instance = this;

        // Load other managers
        // PS: We don't make them as a game components, because we want them to be only accessible through the GameManager instance


        // Load and initialize InputManager
        this.inputManager = new InputManager();

        //Load and initialize WeaponManager
        this.weaponManager = new WeaponManager();
    }

    // Update is called once per frame
    void Update()
    {
        inputManager.Update();
    }
    
}
