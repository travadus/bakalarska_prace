using UnityEngine;

public class HideDebugVisuals : MonoBehaviour
{
    private void Awake()
    {
        Transform areaTransform = transform.Find("Area");
        if (areaTransform != null)
        {
            areaTransform.gameObject.SetActive(false);
        }

        Transform anchorTransform = transform.Find("Anchor");
        if (anchorTransform != null)
        {
            anchorTransform.gameObject.SetActive(false);
        }
    }
}