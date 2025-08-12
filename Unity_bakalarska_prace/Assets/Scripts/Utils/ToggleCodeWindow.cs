using UnityEngine;

public class ToggleCanvas : MonoBehaviour
{
    public GameObject objectToToggle;
    private bool isVisible = false;

    void Start()
    {
        if (objectToToggle != null)
            objectToToggle.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isVisible = !isVisible;

            if (objectToToggle != null)
                objectToToggle.SetActive(isVisible);
        }
    }
}
