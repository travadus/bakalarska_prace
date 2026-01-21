using UnityEngine;

public class HideDebugVisuals : MonoBehaviour
{
    private void Awake()
    {
        // Najde objekt s názvem "Area" uvnitø prefabu a vypne ho
        Transform areaTransform = transform.Find("Area");
        if (areaTransform != null)
        {
            areaTransform.gameObject.SetActive(false);
        }

        // Najde objekt s názvem "Anchor" uvnitø prefabu a vypne ho
        Transform anchorTransform = transform.Find("Anchor");
        if (anchorTransform != null)
        {
            anchorTransform.gameObject.SetActive(false);
        }
    }
}