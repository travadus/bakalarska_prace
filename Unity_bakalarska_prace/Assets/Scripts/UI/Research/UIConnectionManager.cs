using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Generating and positioning connector lines between nodes.
/// </summary>
public class UIConnectionManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject linePrefab;

    /// <summary>
    /// Container for rendered lines to ensure consistent draw order and hierarchy organization.
    /// </summary>
    [SerializeField] private Transform linesLayer;

    /// <summary>
    /// Procedurally creates a visual connection between two UI nodes.
    /// Calculates distance, midpoint, and rotation to align a UI image between the target coordinates.
    /// </summary>
    /// <param name="nodeA">The starting node's RectTransform.</param>
    /// <param name="nodeB">The destination node's RectTransform.</param>
    /// <param name="color">The color assigned to the connecting line.</param>
    public void ConnectNodes(RectTransform nodeA, RectTransform nodeB, Color color)
    {
        if (linePrefab == null || linesLayer == null) return;

        GameObject newLine = Instantiate(linePrefab, linesLayer);

        newLine.transform.localScale = Vector3.one;
        newLine.transform.localPosition = Vector3.zero;

        RectTransform lineRect = newLine.GetComponent<RectTransform>();
        Image lineImage = newLine.GetComponent<Image>();

        lineImage.color = color;
        lineImage.raycastTarget = false;

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