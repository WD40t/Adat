using System.Collections.Generic;
using UnityEngine;

public class ButtonContainer : MonoBehaviour
{
    public List<Location> locationStored; // Assigned via Inspector

    public void OnButtonClickTransfer()
    {
        if (locationStored != null && locationStored.Count > 0)
        {
            LocationTransfer.Instance.CopyFrom(locationStored);
            Debug.Log("[ButtonContainer] Locations copied to LocationTransfer.");
        }
        else
        {
            Debug.LogWarning("[ButtonContainer] No locations to transfer.");
        }
    }
}