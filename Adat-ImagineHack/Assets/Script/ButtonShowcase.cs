using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonShowcase : MonoBehaviour
{
    [Header("Data")]
    public Location location;      // Your ScriptableObject

    [Header("UI References")]
    public TextMeshProUGUI nameText;     // UnityEngine.UI.Text
    public Image mainImage;      // UnityEngine.UI.Image

    void Start()
    {
        RefreshUI();
    }

    /// <summary>
    /// Pulls the values from the Location SO into the UI.
    /// Call this again at runtime if you swap the Location reference.
    /// </summary>
    public void RefreshUI()
    {
        if (location == null) return;

        if (nameText != null)
            nameText.text = location.name;

        if (mainImage != null)
            mainImage.sprite = location.mainImage;
    }
}
