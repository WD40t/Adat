using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public struct LatLng { public double lat, lng; }

[Serializable]
class DistanceElement { public Distance distance; public Duration duration; public string status; }
[Serializable]
class Distance { public string text; public int value; }
[Serializable]
class Duration { public string text; public int value; }
[Serializable]
class DistanceMatrixRow { public DistanceElement[] elements; }
[Serializable]
class DistanceMatrixResponse
{
    public string[] destination_addresses;
    public string[] origin_addresses;
    public DistanceMatrixRow[] rows;
    public string status;
}

[Serializable]
class DirectionsResponse
{
    public Route[] routes;
    public string status;
}

[Serializable]
class Route
{
    public OverviewPolyline overview_polyline;
}

[Serializable]
class OverviewPolyline
{
    public string points;
}

public class MapPath : MonoBehaviour
{
    [Header("API Settings")]
    public string apiKey = "AIzaSyAY7GR_7YOGYSZCMHm0EAHkEkG1wjKsEIY";

    [Header("Waypoints (including start/end)")]
    public List<LatLng> locations = new List<LatLng>();

    [Header("Static Map Display")]
    public RawImage staticMapDisplay;
    public Vector2Int mapImageSize = new Vector2Int(640, 640);

    double[,] distMatrix;
    List<int> bestRoute;      // e.g. [0,2,1,0]
    string roadPolyline;      // encoded

    void Start()
    {
        if (string.IsNullOrWhiteSpace(apiKey) ||
            locations.Count < 2 ||
            staticMapDisplay == null)
        {
            Debug.LogError("Set apiKey, ≥2 locations, and staticMapDisplay.");
            enabled = false;
            return;
        }
        StartCoroutine(FetchAndShowRoute());
    }

    IEnumerator FetchAndShowRoute()
    {
        yield return StartCoroutine(FetchDistanceMatrix());
        SolveTSP();
        yield return StartCoroutine(FetchDirections());
        yield return StartCoroutine(LoadStaticMapImage());
    }

    IEnumerator FetchDistanceMatrix()
    {
        string coordList = string.Join("|",
            locations.ConvertAll(p => $"{p.lat},{p.lng}"));

        string url = $"https://maps.googleapis.com/maps/api/distancematrix/json" +
            $"?origins={UnityWebRequest.EscapeURL(coordList)}" +
            $"&destinations={UnityWebRequest.EscapeURL(coordList)}" +
            $"&key={apiKey}";

        using var www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("DistanceMatrix: " + www.error);
            yield break;
        }

        var dm = JsonUtility.FromJson<DistanceMatrixResponse>(
            www.downloadHandler.text);
        if (dm.status != "OK")
        {
            Debug.LogError("DistanceMatrix status: " + dm.status);
            yield break;
        }

        int n = locations.Count;
        distMatrix = new double[n, n];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < n; j++)
                distMatrix[i, j] =
                    dm.rows[i].elements[j].status == "OK"
                        ? dm.rows[i].elements[j].distance.value
                        : double.PositiveInfinity;
    }

    void SolveTSP()
    {
        int n = locations.Count;
        var stops = new List<int>();
        for (int i = 1; i < n; i++) stops.Add(i);

        double bestCost = double.MaxValue;
        List<int> bestPerm = null;

        foreach (var perm in Permute(stops))
        {
            double cost = 0;
            int prev = 0;
            foreach (int idx in perm)
            {
                cost += distMatrix[prev, idx];
                prev = idx;
            }
            cost += distMatrix[prev, 0]; // loop back

            if (cost < bestCost)
            {
                bestCost = cost;
                bestPerm = new List<int>(perm);
            }
        }

        bestRoute = new List<int> { 0 };
        bestRoute.AddRange(bestPerm);

        Debug.Log($"TSP order: {string.Join("→", bestRoute)}");
        Debug.Log($"Distance: {bestCost / 1000.0:F2} km");
    }

    IEnumerator FetchDirections()
    {
        // origin = first, dest = last
        var origin = locations[bestRoute[0]];
        var dest = locations[bestRoute[^1]];
        // intermediate waypoints = bestRoute[1..^1]
        var wpts = bestRoute.GetRange(1, bestRoute.Count - 2)
                            .ConvertAll(i => locations[i]);
        string wpString = string.Join("|",
            wpts.ConvertAll(p => $"{p.lat},{p.lng}"));

        string url = $"https://maps.googleapis.com/maps/api/directions/json" +
            $"?origin={origin.lat},{origin.lng}" +
            $"&destination={dest.lat},{dest.lng}" +
            (wpts.Count > 0
              ? $"&waypoints={UnityWebRequest.EscapeURL(wpString)}"
              : "") +
            $"&key={apiKey}";

        using var www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Directions: " + www.error);
            yield break;
        }

        var dr = JsonUtility.FromJson<DirectionsResponse>(
            www.downloadHandler.text);
        if (dr.status != "OK" || dr.routes.Length == 0)
        {
            Debug.LogError("Directions status: " + dr.status);
            yield break;
        }

        roadPolyline = dr.routes[0].overview_polyline.points;
    }

    IEnumerator LoadStaticMapImage()
    {
        // build markers & visible exactly as before
        string visible = "&visible=" +
            string.Join("|",
              bestRoute.ConvertAll(i =>
                $"{locations[i].lat},{locations[i].lng}"));

        string markers = "";
        for (int i = 0; i < bestRoute.Count; i++)
        {
            var p = locations[bestRoute[i]];
            string label = (i == 0)
                           ? "S" : i.ToString();
            markers += $"&markers=label:{label}|{p.lat},{p.lng}";
        }

        // *use the encoded polyline* here**
        string path = $"&path=weight:5|color:0x0000ff|enc:{roadPolyline}";

        string url = $"https://maps.googleapis.com/maps/api/staticmap" +
            $"?size={mapImageSize.x}x{mapImageSize.y}" +
            visible + markers + path +
            $"&key={apiKey}";

        using var www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("StaticMap: " + www.error);
            yield break;
        }

        staticMapDisplay.texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        staticMapDisplay.uvRect = new Rect(0, 0, 1, 1);
    }

    // permutation helper...
    IEnumerable<List<int>> Permute(List<int> list)
    {
        if (list.Count == 1)
            yield return new List<int>(list);
        else
        {
            for (int i = 0; i < list.Count; i++)
            {
                int v = list[i];
                var rem = new List<int>(list);
                rem.RemoveAt(i);
                foreach (var perm in Permute(rem))
                {
                    perm.Insert(0, v);
                    yield return perm;
                }
            }
        }
    }
}
