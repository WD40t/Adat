using TMPro;
using UnityEngine;

public class paymentManager : MonoBehaviour
{
    public TextMeshProUGUI text1;
    public TextMeshProUGUI text2;
    public TextMeshProUGUI text3;
    public TextMeshProUGUI text4;
    public TextMeshProUGUI totalText;

    private float totalPrice = 0f;
    private void Start()
    {
        if (SaveDestination.Instance.picked == 1)
        {
            LoadDay1Summary();
        } else
        {
            LoadDay2Summary();
        }
    }

    public void LoadDay1Summary()
    {
        DisplayLocationSummary(SaveDestination.Instance.day1Location);
    }

    public void LoadDay2Summary()
    {
        DisplayLocationSummary(SaveDestination.Instance.day2Location);
    }

    private void DisplayLocationSummary(System.Collections.Generic.List<Location> locations)
    {
        totalPrice = 0f;

        text1.text = FormatLocation(locations, 0);
        text2.text = FormatLocation(locations, 1);
        text3.text = FormatLocation(locations, 2);
        text4.text = FormatLocation(locations, 3);

        totalText.text = $"Total Price: RM {totalPrice:F2}";
    }

    private string FormatLocation(System.Collections.Generic.List<Location> list, int index)
    {
        if (index >= list.Count) return "";

        Location loc = list[index];
        totalPrice += loc.ticketPrice;
        return $"{loc.name} - RM {loc.ticketPrice:F2}";
    }
}