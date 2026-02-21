using UnityEngine;
using UnityEngine.EventSystems;

public class ResizeHandle : MonoBehaviour, IDragHandler
{
    [Header("References")]
    public SmartWindowController mainController;

    [Header("Handle direction")]
    public bool controlHorizontal = false;
    public bool controlVertical = false;

    public void OnDrag(PointerEventData data)
    {
        Vector2 delta = data.delta / GetComponentInParent<Canvas>().scaleFactor;

        mainController.OnDragResize(delta, controlHorizontal, controlVertical);
    }
}