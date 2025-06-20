using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class SmoothMapMercator : MonoBehaviour
{
    [Header("Map Settings")]
    public string apiKey = "AIzaSyAY7GR_7YOGYSZCMHm0EAHkEkG1wjKsEIY";
    public double lat = -33.856606, lon = 151.215007;
    public int zoom = 12;

    public enum Resolution { low = 1, high = 2 }
    public Resolution resolution = Resolution.low;

    public enum MapType { roadmap, satellite, hybrid, terrain }
    public MapType mapType = MapType.roadmap;

    [Header("Timing")]
    [Tooltip("How often (sec) to refresh the map _while_ dragging.")]
    public float mapUpdateDelay = 0f;

    private RawImage rawImage;
    private RectTransform rectT;
    private int mapWidth, mapHeight;

    // Drag state
    private Vector2 dragStartPx;
    private Vector2 dragStartCenterPx;
    private float dragUpdateTimer;

    // Throttle coroutine
    private Coroutine updateCoroutine;

    const int TILE_SIZE = 400;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rectT = rawImage.rectTransform;

        mapWidth = Mathf.RoundToInt(rectT.rect.width);
        mapHeight = Mathf.RoundToInt(rectT.rect.height);

        // initial grab
        StartCoroutine(GetGoogleMap());
    }

    void Update()
    {
        HandleZoom();
        HandleDrag();
    }

    void HandleDrag()
    {
        // On drag start, capture both pointer and map-center
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPx = Input.mousePosition;
            dragStartCenterPx = LatLonToPixel(lat, lon, zoom);
            dragUpdateTimer = mapUpdateDelay;
        }

        // While dragging: live UV shift
        if (Input.GetMouseButton(0))
        {
            Vector2 delta = (Vector2)Input.mousePosition - dragStartPx;
            rawImage.uvRect = new Rect(
                -delta.x / mapWidth,
                -delta.y / mapHeight,
                1, 1
            );

            // countdown and refresh map mid-drag
            dragUpdateTimer -= Time.deltaTime;
            if (dragUpdateTimer <= 0f)
            {
                // Compute new center from this drag
                Vector2 newCenterPx = new Vector2(
                    dragStartCenterPx.x - delta.x,
                    dragStartCenterPx.y + delta.y
                );
                var ll = PixelToLatLon(newCenterPx, zoom);
                lat = ll.x; lon = ll.y;

                // immediately fetch the new map
                if (updateCoroutine != null) StopCoroutine(updateCoroutine);
                updateCoroutine = StartCoroutine(GetGoogleMap());

                // reset drag origin so next delta is fresh
                dragStartPx = Input.mousePosition;
                dragStartCenterPx = LatLonToPixel(lat, lon, zoom);
                dragUpdateTimer = mapUpdateDelay;
            }
        }

        // On release: do one last commit if needed
        if (Input.GetMouseButtonUp(0))
        {
            // you may already be fresh, but ensure final center is applied
            Vector2 delta = (Vector2)Input.mousePosition - dragStartPx;
            Vector2 newCenterPx = new Vector2(
                dragStartCenterPx.x - delta.x,
                dragStartCenterPx.y + delta.y
            );
            var ll = PixelToLatLon(newCenterPx, zoom);
            lat = ll.x; lon = ll.y;

            // final fetch
            if (updateCoroutine != null) StopCoroutine(updateCoroutine);
            updateCoroutine = StartCoroutine(GetGoogleMap());
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0f)
        {
            zoom = Mathf.Clamp(zoom + (scroll > 0 ? 1 : -1), 1, 20);
            if (updateCoroutine != null) StopCoroutine(updateCoroutine);
            updateCoroutine = StartCoroutine(GetGoogleMap());
        }
    }

    IEnumerator GetGoogleMap()
    {
        string url = $"https://maps.googleapis.com/maps/api/staticmap" +
                     $"?center={lat},{lon}" +
                     $"&zoom={zoom}" +
                     $"&size={mapWidth}x{mapHeight}" +
                     $"&scale={(int)resolution}" +
                     $"&maptype={mapType}" +
                     $"&key={apiKey}";

        using (var uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success)
            {
                rawImage.texture = ((DownloadHandlerTexture)uwr.downloadHandler).texture;
                // snap UV back so your texture aligns exactly
                rawImage.uvRect = new Rect(0, 0, 1, 1);
            }
            else Debug.LogError($"Map Load Error: {uwr.error}");
        }
    }

    // —— Web-Mercator conversions ——
    Vector2 LatLonToPixel(double lat, double lon, int zoom)
    {
        double sinLat = Math.Sin(lat * Math.PI / 180.0);
        double mapSize = TILE_SIZE * Math.Pow(2, zoom);

        double x = (lon + 180.0) / 360.0 * mapSize;
        double y = (0.5 - Math.Log((1 + sinLat) / (1 - sinLat)) / (4 * Math.PI)) * mapSize;
        return new Vector2((float)x, (float)y);
    }

    Vector2 PixelToLatLon(Vector2 px, int zoom)
    {
        double mapSize = TILE_SIZE * Math.Pow(2, zoom);

        double lon = px.x / mapSize * 360.0 - 180.0;
        double latRad = Math.Atan(Math.Sinh(Math.PI * (1 - 2 * px.y / mapSize)));
        double lat = latRad * 180.0 / Math.PI;
        return new Vector2((float)lat, (float)lon);
    }
}
