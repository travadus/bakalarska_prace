using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Implements a flexible camera controller.
/// </summary>
public class MyCameraController : MonoBehaviour
{
    public Transform cameraTransform;

    [Header("Movement Settings")]
    public float normalSpeed;
    public float fastSpeed;
    public float movementSpeed;
    public float movementTime;
    public float rotationAmount;
    public Vector3 zoomAmount;

    [Header("Mouse Sensitivity")]
    public float mouseZoomSensitivity;
    public float mouseRotationSensitivity;

    [Header("Target Vectors")]
    public Vector3 newPosition;
    public Quaternion newRotation;
    public Vector3 newZoom;

    void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
    }

    void Update()
    {
        HandleMovementInput();
        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        if (!IsTypingInUI())
        {
            // Scroll-wheel based distance adjustment
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
                newZoom += zoomAmount * scroll * mouseZoomSensitivity;

            // Middle-mouse button orbital rotation tracking
            if (Input.GetMouseButton(2))
            {
                float mouseX = Input.GetAxis("Mouse X");
                newRotation *= Quaternion.Euler(Vector3.up * mouseX * mouseRotationSensitivity);
            }
        }
    }

    void HandleMovementInput()
    {
        // Toggle movement speed
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movementSpeed = fastSpeed;
        }
        else
        {
            movementSpeed = normalSpeed;
        }

        if (!IsTypingInUI())
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                newPosition += (transform.forward * movementSpeed);
            }
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                newPosition += (transform.forward * -movementSpeed);
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                newPosition += (transform.right * movementSpeed);
            }
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                newPosition += (transform.right * -movementSpeed);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
            }
            if (Input.GetKey(KeyCode.E))
            {
                newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
            }

            if (Input.GetKey(KeyCode.R))
            {
                newZoom += zoomAmount;
            }
            if (Input.GetKey(KeyCode.F))
            {
                newZoom -= zoomAmount;
            }
        }

        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }

    /// <summary>
    /// Used to prevent camera movement commands while typing.
    /// </summary>
    /// <returns>True if an InputField is active; otherwise, false.</returns>
    private bool IsTypingInUI()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return false;

        return selected.GetComponent<UnityEngine.UI.InputField>() != null
            || selected.GetComponent<TMPro.TMP_InputField>() != null;
    }
}