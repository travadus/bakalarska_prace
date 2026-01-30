using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragHandler : MonoBehaviour, IDragHandler
{
    [Header("Co se má hýbat?")]
    [SerializeField] private RectTransform windowToMove; // Sem pøetáhneš hlavní okno (TESTCodeWindow)

    private Canvas parentCanvas;

    private void Start()
    {
        // Automaticky najdeme Canvas, abychom znali jeho Scale Factor
        // To je dùležité, aby se okno hýbalo stejnì rychle jako myš, i když máš jiné rozlišení.
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentCanvas == null) return;

        // Pøièteme pohyb myši (delta) k pozici okna
        // Dìlíme scaleFactorem, aby to bylo pøesné na všech rozlišeních
        windowToMove.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
    }
}