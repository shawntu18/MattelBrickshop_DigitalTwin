using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Public variables will show up in the Unity Inspector
    public float moveSpeed = 10f;
    public float rotateSpeed = 5f;
    public float zoomSpeed = 10f;

    private float rotationX = 0f;
    private float rotationY = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // Lock the cursor to the center of the screen when playing
        // Cursor.lockState = CursorLockMode.Locked;
        // You can uncomment the line above if you prefer the cursor to be hidden
    }

    // Update is called once per frame
    void Update()
    {
        // == MOVEMENT (W/A/S/D) ==
        float horizontalInput = Input.GetAxis("Horizontal"); // A/D keys
        float verticalInput = Input.GetAxis("Vertical");     // W/S keys

        Vector3 moveDirection = new Vector3(horizontalInput, 0, verticalInput);
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.Self);

        // == ROTATION (Right Mouse Button) ==
        if (Input.GetMouseButton(1)) // 1 is the right mouse button
        {
            rotationX += Input.GetAxis("Mouse X") * rotateSpeed;
            rotationY -= Input.GetAxis("Mouse Y") * rotateSpeed;
            
            // Clamp the vertical rotation to prevent flipping upside down
            rotationY = Mathf.Clamp(rotationY, -90f, 90f);

            transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);
        }

        // == ZOOM (Mouse Scroll Wheel) ==
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        Vector3 zoomDirection = new Vector3(0, 0, scrollInput);
        transform.Translate(zoomDirection * zoomSpeed, Space.Self);
    }
}