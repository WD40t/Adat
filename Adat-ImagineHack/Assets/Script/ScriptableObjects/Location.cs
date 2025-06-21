using UnityEngine;

[CreateAssetMenu(fileName = "Location", menuName = "Scriptable Objects/Location")]
public class Location : ScriptableObject
{
    [Header("Main Details")]
    public string name;
    [TextArea(3, 100)]
    public string shortDescription;
    public float ticketPrice;

    [Header("Coordinates")]
    public double latitude;
    public double longitude;

    [Header("Image")]
    public Sprite mainImage;
    public Sprite image1;
    public Sprite image2;

    [Header("Text")]
    [TextArea(3, 100)]
    public string description;

    [Header("Tags")]
    public CultureTag cultureTag;
    public LocationType locationType;

    public enum CultureTag
    {
        Malay, Chinese, Hindu, Muslim, Christian, Buddhist, Confucianist
    }

    public enum LocationType
    {
        Church, Memorial, Temple
    }
}