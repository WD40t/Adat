using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Text Fields")]
    public TMP_Text nameText;
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

    [Header("Cost Calculator UI")]
    public TMP_InputField paxInputField;
    public Button calculateCostButton;
    public TMP_Text totalCostText;

    private int currentIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Multiple UIManagers found. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        nextButton?.onClick.AddListener(ShowNextLocation);
        likeButton?.onClick.AddListener(OnLikeButtonPressed);
        calculateCostButton?.onClick.AddListener(OnCalculateCost);

        var list = LocationTransfer.Instance?.locationStored;
        if (list != null && list.Count > 0)
        {
            currentIndex = 0;
            PopulateUI(list[currentIndex]);
            UpdateLikeButtonState();
        }
    }

    public void ShowNextLocation()
    {
        var list = LocationTransfer.Instance?.locationStored;
        if (list == null || list.Count == 0) return;

        currentIndex = (currentIndex + 1) % list.Count;
        PopulateUI(list[currentIndex]);
        UpdateLikeButtonState();
    }

    private void OnLikeButtonPressed()
    {
        var list = LocationTransfer.Instance?.locationStored;
        if (list == null || list.Count == 0) return;

        var loc = list[currentIndex];
        bool added = SaveDestination.Instance.AddLocation(loc);

        if (added)
            Debug.Log($"Saved {loc.name}!");
        else
            Debug.LogWarning("Couldn’t save. Already saved or max reached.");

        UpdateLikeButtonState();
        ShowNextLocation();
        SaveDestination.Instance.DistributeLocations();
    }

    private void UpdateLikeButtonState()
    {
        var list = LocationTransfer.Instance?.locationStored;
        if (SaveDestination.Instance == null || likeButton == null || list == null || list.Count == 0)
            return;

        var currentLoc = list[currentIndex];
        var saved = SaveDestination.Instance.SavedLocations;

        likeButton.interactable = !saved.Contains(currentLoc) &&
                                  saved.Count < SaveDestination.Instance.MaxLocations;
    }

    private void PopulateUI(Location data)
    {
        nameText.text = data.name;
        shortDescriptionText.text = data.shortDescription;
        ticketPriceText.text = $"RM {data.ticketPrice:F2}";
        descriptionText.text = data.description;
        cultureTagText.text = $"Tag: {data.cultureTag}";
        locationTypeText.text = $"Type: {data.locationType}";

        mainImage.sprite = data.mainImage;
        image1.sprite = data.image1;
        image2.sprite = data.image2;

        if (MapManager.Instance != null)
            MapManager.Instance.UpdateMap((float)data.latitude, (float)data.longitude);
    }

    private void OnCalculateCost()
    {
        if (!int.TryParse(paxInputField.text, out int pax) || pax < 1)
        {
            totalCostText.text = "Enter a valid number of pax!";
            return;
        }

        float total = 0f;
        foreach (var loc in SaveDestination.Instance.SavedLocations)
            total += loc.ticketPrice;

        total *= pax;
        totalCostText.text = $"Total Cost: RM {total:F2}";
    }
}
