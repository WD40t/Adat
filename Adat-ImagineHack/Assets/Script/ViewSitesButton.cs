using UnityEngine;
using UnityEngine.UI;

public class ViewSitesButton : MonoBehaviour
{
    public GameObject viewSite;

    // Update is called once per frame
    void Update()
    {
        if (SaveDestination.Instance.GetSlotCount() == 0)
        {
            viewSite.gameObject.SetActive(false);
        }else
        {
            viewSite.gameObject.SetActive(true);
        }
    }
}
