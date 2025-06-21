using System;
using System.Collections.Generic;
using UnityEngine;

public class RouteOptimizer : MonoBehaviour
{
    [Header("Route Inputs")]
    [Tooltip("Your chosen start (e.g. hotel)")]
    public Location startLocation;

    [Tooltip("If true, the route will return back to the start at the end")]
    public bool endAtStart = false;

    [Tooltip("Intermediate stops (up to 4 liked Locations)")]
    public List<Location> visitLocations = new List<Location>();

    [Header("Results (read-only)")]
    [Tooltip("The optimized list of Locations in order")]
    public List<Location> optimizedRoute = new List<Location>();

    [Tooltip("Total distance (in km) of that route")]
    public double totalDistance = 0;

    [ContextMenu("Calculate Optimal Route")]
    public void CalculateRoute()
    {
        // Validation
        if (startLocation == null)
        {
            Debug.LogWarning("[RouteOptimizer] startLocation is required.");
            return;
        }
        if (visitLocations == null || visitLocations.Count == 0)
        {
            Debug.LogWarning("[RouteOptimizer] No visitLocations—route is just the start.");
            optimizedRoute = new List<Location> { startLocation };
            if (endAtStart) optimizedRoute.Add(startLocation);
            totalDistance = 0;
            return;
        }

        List<Location> bestPath = null;
        double shortest = double.MaxValue;

        // Try every permutation of the intermediate stops
        foreach (var perm in GetPermutations(visitLocations, visitLocations.Count))
        {
            var path = new List<Location> { startLocation };
            path.AddRange(perm);

            if (endAtStart)
                path.Add(startLocation);

            // Compute total distance for this path
            double dist = 0;
            for (int i = 0; i < path.Count - 1; i++)
                dist += HaversineDistance(
                    path[i].latitude, path[i].longitude,
                    path[i + 1].latitude, path[i + 1].longitude
                );

            // Keep the best
            if (dist < shortest)
            {
                shortest = dist;
                bestPath = new List<Location>(path);
            }
        }

        // Save and report
        optimizedRoute = bestPath;
        totalDistance = shortest;

        Debug.Log("[RouteOptimizer] Optimal Route: " +
                  string.Join(" → ", optimizedRoute.ConvertAll(l => l.name)));
        Debug.Log($"[RouteOptimizer] Total Distance: {totalDistance:F2} km");
    }

    /// <summary>
    /// Great-circle distance in kilometers between two lat/lon points.
    /// </summary>
    private double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371.0; // Earth radius in km
        double dLat = (lat2 - lat1) * Mathf.Deg2Rad;
        double dLon = (lon2 - lon1) * Mathf.Deg2Rad;

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(lat1 * Mathf.Deg2Rad)
                 * Math.Cos(lat2 * Mathf.Deg2Rad)
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    /// <summary>
    /// Recursively yield all permutations of `list` taken `length` at a time.
    /// </summary>
    private IEnumerable<List<T>> GetPermutations<T>(List<T> list, int length)
    {
        if (length == 1)
        {
            foreach (var item in list)
                yield return new List<T> { item };
        }
        else
        {
            for (int i = 0; i < list.Count; i++)
            {
                var current = list[i];
                var remaining = new List<T>(list);
                remaining.RemoveAt(i);

                foreach (var perm in GetPermutations(remaining, length - 1))
                {
                    perm.Insert(0, current);
                    yield return perm;
                }
            }
        }
    }
}
