using System.Collections.Generic;
using UnityEngine;

public class WayPointManager : MonoBehaviour
{

    public static WayPointManager Instance;
    public Transform[] waypoints;

    void Awake()
    {
        Instance = this;
    }
}