using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("All Locations")]
    public List<Location> locations; // Drag your ScriptableObjects here in the Inspector

    [Header("Text Fields")]
    public TMP_Text nameText;
    public TMP_Text distanceText;
    public TMP_Text shortDescriptionText;
    public TMP_Text ticketPriceText;
    public TMP_Text descriptionText;
    public TMP_Text cultureTagText;
    public TMP_Text locationTypeText;

    [Header("Image Fields")]
    public Image mainImage;
    public Image image1;
    public Image image2;

    [Header("Controls")]
    public Button nextButton;

    private int currentIndex = 0;

    void Start()
    {
        if (locations.Count > 0)
        {
            PopulateUI(locations[currentIndex]);
        }

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ShowNextLocation);
        }
    }

    public void ShowNextLocation()
    {
        if (locations.Count == 0) return;

        currentIndex++;
        if (currentIndex >= locations.Count)
        {
            currentIndex = 0; // wrap around
        }

        PopulateUI(locations[currentIndex]);
    }

    public void PopulateUI(Location data)
    {
        nameText.text = data.name;
        distanceText.text = data.distance+" KM";
        shortDescriptionText.text = data.shortDescription;
        ticketPriceText.text = "RM " + data.ticketPrice.ToString("F2");
        descriptionText.text = data.description;
        cultureTagText.text = "Tag: " + data.cultureTag.ToString();
        locationTypeText.text = "Type: " + data.locationType.ToString();

        // set images (null check)
        mainImage.sprite = data.mainImage;
        image1.sprite = data.image1;
        image2.sprite = data.image2;

        // update map based on location
        MapManager.Instance.UpdateMap((float)data.latitude, (float)data.longitude);
    }
}
