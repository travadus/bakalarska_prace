using UnityEngine;

public class ToggleCanvas : MonoBehaviour
{
    public GameObject canvasToToggle;
    private bool isVisible = false;

    void Start()
    {
        if (canvasToToggle != null)
            canvasToToggle.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isVisible = !isVisible;

            if (canvasToToggle != null)
                canvasToToggle.SetActive(isVisible);
        }
    }
}
