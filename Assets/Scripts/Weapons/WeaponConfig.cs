using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[Serializable]
public class WeaponConfig
{
    public float cooldownPeriod;
    public GameObject particleSysPrefab;
    public AudioSource[] sounds;
    
    public float damage;
    public float radius;
    public float explosionFalloffModifier;
}
