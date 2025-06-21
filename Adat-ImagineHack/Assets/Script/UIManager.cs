using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    [Header("All Locations")]
    public List<Location> locations;            // your destinations list

    [Header("Your Hotel (optional)")]
    public Location hotelLocation;              // assign your Hotel SO here

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
    public Button likeButton;

    [Header("Start / End Selection (if used)")]
    public TMP_Dropdown startDropdown;
    public TMP_Dropdown endDropdown;
    public TMP_Text startLabel;
    public TMP_Text endLabel;

    [Header("Cost Calculator UI")]
    public TMP_InputField paxInputField;
    public Button calculateCostButton;
    public TMP_Text totalCostText;

    private int currentIndex = 0;

    void Start()
    {
        // 1) Populate the first location view
        if (locations.Count > 0)
            PopulateUI(locations[currentIndex]);

        // 2) Hook up paging & liking
        nextButton?.onClick.AddListener(ShowNextLocation);
        likeButton?.onClick.AddListener(OnLikeButtonPressed);

        // 3) Build start/end dropdowns (if you’re using them)
        //SetupDropdowns();

        // 4) Hook up cost‐calculator button
        calculateCostButton?.onClick.AddListener(OnCalculateCost);

        // 5) Initialize UI states
        UpdateLikeButtonState();
        //UpdateStartEndLabels();
        UpdateTotalCostDisplay();
    }


    public void ShowNextLocation()
    {
        if (locations.Count == 0) return;

        currentIndex = (currentIndex + 1) % locations.Count;
        PopulateUI(locations[currentIndex]);
        UpdateLikeButtonState();
    }

    private void OnLikeButtonPressed()
    {
        var loc = locations[currentIndex];
        bool added = SaveDestination.Instance.AddLocation(loc);

        if (added)
        {
            Debug.Log($"Saved {loc.name}!");
            // optionally give UI feedback: e.g. change button color or text
        }
        else
        {
            Debug.LogWarning("Couldn’t save. Either already saved or max of 4 reached.");
            // optionally pop up a warning to the player
        }

        UpdateLikeButtonState();
    }

    /// <summary>
    /// Enables/disables the like button if this location is already saved
    /// or if we've hit the max capacity.
    /// </summary>
    private void UpdateLikeButtonState()
    {
        if (SaveDestination.Instance == null || likeButton == null)
            return;

        var saved = SaveDestination.Instance.SavedLocations;
        var currentLoc = locations[currentIndex];

        // disable if already saved or capacity full
        likeButton.interactable =
            !saved.Contains(currentLoc) &&
            saved.Count < SaveDestination.Instance.MaxLocations;
    }

    public void PopulateUI(Location data)
    {
        nameText.text = data.name;
        shortDescriptionText.text = data.shortDescription;
        ticketPriceText.text = "RM " + data.ticketPrice.ToString("F2");
        descriptionText.text = data.description;
        cultureTagText.text = "Tag: " + data.cultureTag;
        locationTypeText.text = "Type: " + data.locationType;

        mainImage.sprite = data.mainImage;
        image1.sprite = data.image1;
        image2.sprite = data.image2;

        MapManager.Instance.UpdateMap((float)data.latitude, (float)data.longitude);
    }

    public void OnCalculateCost()
    {
        // 1. parse pax
        if (!int.TryParse(paxInputField.text, out int pax) || pax < 1)
        {
            totalCostText.text = "Enter a valid number of pax!";
            return;
        }

        // 2. gather ALL the ticketed stops
        var dests = new List<Location>();

        // (a) start – skip hotel if you want, assuming hotelLocation has ticketPrice = 0
        if (SaveDestination.Instance.StartLocation != null)
            dests.Add(SaveDestination.Instance.StartLocation);

        // (b) liked intermediates
        dests.AddRange(SaveDestination.Instance.SavedLocations);

        // (c) end
        if (SaveDestination.Instance.EndLocation != null)
            dests.Add(SaveDestination.Instance.EndLocation);

        // 3. sum their ticketPrice
        float sum = 0f;
        foreach (var loc in dests)
            sum += loc.ticketPrice;

        // 4. multiply by pax
        float total = sum * pax;

        // 5. display
        totalCostText.text = $"Total Cost: RM {total:F2}";
    }

    private void UpdateTotalCostDisplay()
    {
        // optional: clear or show a default
        totalCostText.text = "Total Cost: RM 0.00";
    }
}
