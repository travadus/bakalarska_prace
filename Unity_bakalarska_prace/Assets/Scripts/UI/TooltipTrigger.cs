using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Tooltip Data")]
    public string header;
    [TextArea] public string content;

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipSystem.Instance.Show(content, header);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipSystem.Instance.Hide();
    }
}