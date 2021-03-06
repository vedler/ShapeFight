﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
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

    private WSelectionManager weaponSelectionManager;

    public WSelectionManager getWSelectionManager()
    {
        if (weaponSelectionManager == null)
        {
            throw new UnityException("GameManager's WSelectionManager instance was accessed before Unity started it!");
        }
        return weaponSelectionManager;
    }

    // Use this for initialization
    void Awake()
    {
        // Set the singleton instance
        GameManager.instance = this;

        // Load other managers
        // PS: We don't make them as a game components, because we want them to be only accessible through the GameManager instance


        // Load and initialize InputManager
        this.inputManager = new InputManager(this);

        //Load and initialize WeaponManager
        this.weaponManager = new WeaponManager(this);

        this.weaponSelectionManager = new WSelectionManager(this);
    }

    // Update is called once per frame
    void Update()
    {
        inputManager.Update();
    }
    
}
