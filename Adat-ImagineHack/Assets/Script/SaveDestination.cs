using System.Collections.Generic;
using UnityEngine;

public class SaveDestination : MonoBehaviour
{
    public static SaveDestination Instance { get; private set; }

    [SerializeField] private int maxLocations = 4;
    public int MaxLocations => maxLocations;

    // your “liked” intermediate stops
    public List<Location> SavedLocations { get; } = new List<Location>();

    // these two weren’t there before:
    public Location StartLocation { get; private set; }
    public Location EndLocation { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool AddLocation(Location loc)
    {
        if (loc == null) return false;
        if (SavedLocations.Contains(loc)) return false;
        if (SavedLocations.Count >= maxLocations) return false;

        SavedLocations.Add(loc);
        return true;
    }

    // call this from your UIManager
    public void SetStart(Location loc)
    {
        StartLocation = loc;
        Debug.Log($"[SaveDestination] Start set to {loc.name}");
    }

    // call this from your UIManager
    public void SetEnd(Location loc)
    {
        EndLocation = loc;
        Debug.Log($"[SaveDestination] End set to {loc.name}");
    }
}
