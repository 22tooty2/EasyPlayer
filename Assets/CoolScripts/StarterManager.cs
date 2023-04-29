using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarterManager : MonoBehaviour
{
    // Crown
    public delegate void RaiseButton();

    // Servants
    public bool StartChasing;
    public static RaiseButton startChasing;

    public bool ActivatePlayer;
    public static RaiseButton activatePlayer;

    public bool KnockbackPlayer;
    public delegate void knockbackPlayerDelegate(Vector3 dir, float force);
    public static knockbackPlayerDelegate knockbackPlayer;

    public Vector3 dir;
    public float force;

    // Update is called once per frame
    void Update()
    {
        if (StartChasing)
        {
            StartChasing = false;
            startChasing?.Invoke();
        }
        if (ActivatePlayer)
        {
            ActivatePlayer = false;
            activatePlayer?.Invoke();
        }
        if (KnockbackPlayer || Input.GetKeyDown(KeyCode.C))
        {
            KnockbackPlayer = false;
            knockbackPlayer?.Invoke(dir, force);
        }
    }
}
