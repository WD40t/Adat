using UnityEngine;
using UnityEngine.UI;

public class DayChecker : MonoBehaviour
{
    public Button day1;
    public Button day2;

    void Start()
    {
        // Safety check in case SaveDestination is not ready
        if (SaveDestination.Instance == null)
        {
            Debug.LogError("SaveDestination instance not found!");
            return;
        }

        // Show/hide day buttons
        day1.gameObject.SetActive(true); // always show day 1

        // only show day 2 if it has locations
        if (SaveDestination.Instance.day2Location.Count > 0)
        {
            day2.gameObject.SetActive(true);
        }
        else
        {
            day2.gameObject.SetActive(false);
        }
    }
}
