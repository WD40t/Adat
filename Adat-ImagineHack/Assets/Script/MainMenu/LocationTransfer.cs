using System.Collections.Generic;
using UnityEngine;

public class LocationTransfer : MonoBehaviour
{
    public static LocationTransfer Instance { get; private set; }

    public List<Location> locationStored = new List<Location>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep across scenes
    }

    public void CopyFrom(List<Location> sourceList)
    {
        locationStored.Clear();

        foreach (var loc in sourceList)
        {
            if (loc == null) continue;

            Location copy = ScriptableObject.CreateInstance<Location>();
            copy.name = loc.name;
            copy.shortDescription = loc.shortDescription;
            copy.ticketPrice = loc.ticketPrice;
            copy.latitude = loc.latitude;
            copy.longitude = loc.longitude;
            copy.mainImage = loc.mainImage;
            copy.image1 = loc.image1;
            copy.image2 = loc.image2;
            copy.description = loc.description;
            copy.cultureTag = loc.cultureTag;
            copy.locationType = loc.locationType;

            locationStored.Add(copy);
        }

        Debug.Log($"[LocationTransfer] Copied {locationStored.Count} locations.");
    }
}
