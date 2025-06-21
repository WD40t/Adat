using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class CharterManager : MonoBehaviour
{
    public GameObject daysMenu;
    public GameObject mapsMenu;

    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;
    public TextMeshProUGUI text4;

    public GameObject mapPrefab;       // Assign in inspector
    private GameObject currentMap;     // Will hold the instantiated map

    void Start()
    {
        daysMenu.SetActive(true);
        mapsMenu.SetActive(false);
    }

    public void swapMenu()
    {
        daysMenu.SetActive(!daysMenu.activeSelf);
        mapsMenu.SetActive(!mapsMenu.activeSelf);
    }

    public void LoadDay1()
    {
        var locations = SaveDestination.Instance.day1Location;

        text1.text = locations.Count > 0 ? "[S] " + locations[0].name : "";
        text2.text = locations.Count > 1 ? "[1] " + locations[1].name : "";
        text3.text = locations.Count > 2 ? "[2] " + locations[2].name : "";
        text4.text = locations.Count > 3 ? "[3] " + locations[3].name : "";
    }

    public void LoadDay2()
    {
        var locations = SaveDestination.Instance.day2Location;

        text1.text = locations.Count > 0 ? "[S] " + locations[0].name : "";
        text2.text = locations.Count > 1 ? "[1] " + locations[1].name : "";
        text3.text = locations.Count > 2 ? "[2] " + locations[2].name : "";
        text4.text = locations.Count > 3 ? "[3] " + locations[3].name : "";
    }

    public void ShowDay1Map()
    {
        LoadAndShowMap(SaveDestination.Instance.day1Location);
    }

    public void ShowDay2Map()
    {
        LoadAndShowMap(SaveDestination.Instance.day2Location);
    }

    private void LoadAndShowMap(List<Location> locations)
    {
        // Destroy existing map if present
        if (currentMap != null)
        {
            Destroy(currentMap);
        }

        // Instantiate new map
        currentMap = Instantiate(mapPrefab, transform); // optional: set a better parent like a UI container

        // Get MapPath component and populate coordinates
        MapPath path = currentMap.GetComponent<MapPath>();
        if (path == null)
        {
            Debug.LogError("MapPrefab is missing the MapPath component.");
            return;
        }

        path.locations.Clear();
        foreach (var loc in locations)
        {
            path.locations.Add(new LatLng
            {
                lat = loc.latitude,
                lng = loc.longitude
            });
        }

        Debug.Log($"[CharterManager] Loaded {locations.Count} coordinates into the map.");
    }
    public void DestroyCurrentMap()
    {
        if (currentMap != null)
        {
            Destroy(currentMap);
            currentMap = null;
            Debug.Log("[CharterManager] Current map destroyed externally.");
        }
    }
}