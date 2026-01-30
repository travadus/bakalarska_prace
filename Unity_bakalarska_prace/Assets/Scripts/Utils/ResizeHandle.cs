using UnityEngine;
using UnityEngine.EventSystems;

public class ResizeHandle : MonoBehaviour, IDragHandler
{
    [Header("Reference")]
    public SmartWindowController mainController; // Pøetáhni sem hlavní okno

    [Header("Smìr úchytu")]
    public bool controlHorizontal = false;
    public bool controlVertical = false;

    public void OnDrag(PointerEventData data)
    {
        // Pøepoèet pohybu myši s ohledem na mìøítko Canvasu (aby to nebylo moc rychlé/pomalé)
        Vector2 delta = data.delta / GetComponentInParent<Canvas>().scaleFactor;

        mainController.OnDragResize(delta, controlHorizontal, controlVertical);
    }
}