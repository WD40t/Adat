using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    // Singleton instance
    public static MapManager Instance { get; private set; }

    [Header("Map Settings")]
    public string apiKey;
    public float lat = -33.856606f;
    public float lon = 151.215007f;
    private int zoom = 18;

    public enum Resolution { Low = 1, High = 2 };
    public Resolution mapResolution = Resolution.Low;

    public enum MapType { Roadmap, Satellite, Hybrid, Terrain };
    public MapType mapType = MapType.Roadmap;

    private string url = "";
    private int mapWidth = 640;
    private int mapHeight = 640;
    private bool mapIsLoading = false;
    private Rect rect;

    // Last loaded state
    private string apiKeyLast;
    private float latLast = -33.856606f;
    private float lonLast = 151.215007f;
    private int zoomLast = 18;
    private Resolution mapResolutionLast = Resolution.Low;
    private MapType mapTypeLast = MapType.Roadmap;

    private RawImage mapImage;

    void Awake()
    {
        // Enforce singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        mapImage = GetComponent<RawImage>();
        rect = mapImage.rectTransform.rect;
        mapWidth = (int)Math.Round(rect.width);
        mapHeight = (int)Math.Round(rect.height);
    }

    void Start()
    {
        StartCoroutine(GetGoogleMap());
    }

    /// <summary>
    /// Call this from other scripts to update the map location.
    /// </summary>
    public void UpdateMap(float newLat, float newLon)
    {
        lat = newLat;
        lon = newLon;

        rect = mapImage.rectTransform.rect;
        mapWidth = (int)Math.Round(rect.width);
        mapHeight = (int)Math.Round(rect.height);

        StartCoroutine(GetGoogleMap());
    }

    IEnumerator GetGoogleMap()
    {
        // Marker format: &markers=color:red|label:C|lat,lon
        string markerParam = $"&markers=color:blue|{lat},{lon}";

        url = $"https://maps.googleapis.com/maps/api/staticmap?center={lat},{lon}&zoom={zoom}&size={mapWidth}x{mapHeight}&scale={(int)mapResolution}&maptype={mapType.ToString().ToLower()}{markerParam}&key={apiKey}";

        mapIsLoading = true;

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Google Map Error: " + www.error);
            }
            else
            {
                mapImage.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                mapIsLoading = false;

                // Save last loaded state
                apiKeyLast = apiKey;
                latLast = lat;
                lonLast = lon;
                zoomLast = zoom;
                mapResolutionLast = mapResolution;
                mapTypeLast = mapType;
            }
        }
    }
}
