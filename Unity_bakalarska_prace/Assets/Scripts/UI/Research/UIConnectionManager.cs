using UnityEngine;
using UnityEngine.UI;

public class UIConnectionManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject linePrefab;

    // ZMÌNA: Budeme tomu øíkat pøesnì - Vrstva pro èáry
    [SerializeField] private Transform linesLayer;

    public void ConnectNodes(RectTransform nodeA, RectTransform nodeB, Color color)
    {
        if (linePrefab == null || linesLayer == null) return;

        // 1. Vytvoøíme èáru v oddìlené vrstvì (LinesLayer)
        GameObject newLine = Instantiate(linePrefab, linesLayer);

        // Reset transformace (pro jistotu)
        newLine.transform.localScale = Vector3.one;
        newLine.transform.localPosition = Vector3.zero;

        RectTransform lineRect = newLine.GetComponent<RectTransform>();
        Image lineImage = newLine.GetComponent<Image>();

        lineImage.color = color;
        lineImage.raycastTarget = false; // Aby se dalo klikat skrz èáry

        // 2. Výpoèet pozic
        // POZOR: Protože LinesLayer i NodesLayer jsou "sourozenci" a mají stejnou velikost (Stretch),
        // mùžeme stále používat localPosition tlaèítek.

        // Musíme ale zajistit, abychom brali pozici relativnì k Contentu, 
        // ne relativnì k NodesLayer, pokud jsou nody zanoøené.
        // Ale pokud mají Layers stejný Anchor (Stretch), mìlo by to sedìt.

        // Pro jistotu pøevedeme souøadnice, kdyby se Layers pohnuly:
        Vector3 posA = linesLayer.InverseTransformPoint(nodeA.position);
        Vector3 posB = linesLayer.InverseTransformPoint(nodeB.position);

        posA.z = 0;
        posB.z = 0;

        Vector3 midPoint = (posA + posB) / 2;
        float distance = Vector3.Distance(posA, posB);

        Vector3 direction = posB - posA;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        lineRect.localPosition = midPoint;
        lineRect.sizeDelta = new Vector2(distance, 3f);
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
    }
}