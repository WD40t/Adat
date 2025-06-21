using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class SaveDestination : MonoBehaviour
{
    public static SaveDestination Instance { get; private set; }

    [SerializeField] private int maxLocations = 6;
    public int MaxLocations => maxLocations;

    // your “liked” intermediate stops
    public List<Location> SavedLocations { get; } = new List<Location>();


    // these two weren’t there before:
    public Location StartLocation { get; private set; }
    public Location EndLocation { get; private set; }

    public int picked;

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
    public bool IsSlotFilled(int index)
    {
        return index >= 0 && index < SavedLocations.Count && SavedLocations[index] != null;
    }
    public int GetSlotCount()
    {
        return SavedLocations.Count;
    }

    public List<Location> day1Location = new List<Location>();
    public List<Location> day2Location = new List<Location>();

    public void DistributeLocations()
    {
        // Clear any previous assignments
        day1Location.Clear();
        day2Location.Clear();

        // Create a working copy and shuffle it
        List<Location> shuffled = new List<Location>(SavedLocations);
        for (int i = 0; i < shuffled.Count; i++)
        {
            int randIndex = UnityEngine.Random.Range(i, shuffled.Count);
            var temp = shuffled[i];
            shuffled[i] = shuffled[randIndex];
            shuffled[randIndex] = temp;
        }

        int total = shuffled.Count;

        int day1Count = 0;

        int day2Count = 0;

        int day3Count = 0;


        if (total > 4)
        {
            day1Count += 3;
            total -= 3;
            
            if (total > 3)
            {
                day2Count += 3;
                total -= 3;
            }
            else
            {
                day2Count = total;
                total = 0;
            }
            if (total > 3)
            {
                day3Count += 3;
                total = 0;
            }
            else
            {
                day3Count = total;
                total = 0;
            }

        }
        else
        {
            day1Count = total;
        }
            // Assign to days
            for (int i = 0; i < day1Count; i++)
            {
                day1Location.Add(shuffled[i]);
            }

        for (int i = day1Count; i < day1Count + day2Count; i++)
        {
            day2Location.Add(shuffled[i]);
        }

        Debug.Log($"[SaveDestination] Day 1: {day1Location.Count}, Day 2: {day2Location.Count}");
    }
    public void choosePicked(int picked2)
    {
        picked = picked2;
    }
}
