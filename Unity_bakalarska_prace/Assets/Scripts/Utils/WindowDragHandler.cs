using UnityEngine;
using UnityEngine.EventSystems;

public class WindowDragHandler : MonoBehaviour, IDragHandler
{
    [Header("What can move")]
    [SerializeField] private RectTransform windowToMove;

    private Canvas parentCanvas;

    private void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentCanvas == null) return;
        windowToMove.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
    }
}