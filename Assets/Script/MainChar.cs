using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MainChar : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public CharacterController controller; // optional: assign or will GetComponent in Start

    [Header("Look")]
    public Transform cameraTransform; // assign child Camera here
    public float mouseSensitivity = 0.15f; // multiply mouse delta
    public float minPitch = -85f;
    public float maxPitch = 85f;

    float yaw = 0f;
    float pitch = 0f;

    [Header("Jump & Gravity")]
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;
    float verticalVelocity = 0f;

    [Header("Shooting")]
    public float shotRange = 100f;
    public float shotsPerSecond = 4f;
    public Color shotColor = new Color(1f, 0.72f, 0.25f);
    float nextShotTime;
    static Material shotTraceMaterial;

    void Start()
    {
        // lock cursor for FPS-like control
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        if (cameraTransform != null) pitch = cameraTransform.localEulerAngles.x;
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        // Toggle cursor with Escape
        var kb = Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame)
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        HandleMove();
        HandleLook();
        HandleShooting();
    }

    void HandleShooting()
    {
        var mouse = Mouse.current;
        if (cameraTransform == null || mouse == null || !mouse.leftButton.wasPressedThisFrame ||
            Cursor.lockState != CursorLockMode.Locked || Time.time < nextShotTime)
        {
            return;
        }

        nextShotTime = Time.time + 1f / Mathf.Max(0.1f, shotsPerSecond);
        Vector3 origin = cameraTransform.position;
        Vector3 end = origin + cameraTransform.forward * shotRange;
        if (Physics.Raycast(origin, cameraTransform.forward, out RaycastHit hit, shotRange))
        {
            end = hit.point;
        }

        StartCoroutine(ShowShotLine(origin, end));
    }

    IEnumerator ShowShotLine(Vector3 start, Vector3 end)
    {
        var shotObject = new GameObject("Shot Trace");
        var line = shotObject.AddComponent<LineRenderer>();
        if (shotTraceMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader != null) shotTraceMaterial = new Material(shader);
        }
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = 0.025f;
        line.endWidth = 0.008f;
        line.sharedMaterial = shotTraceMaterial;
        line.startColor = shotColor;
        line.endColor = new Color(shotColor.r, shotColor.g, shotColor.b, 0f);
        line.useWorldSpace = true;
        yield return new WaitForSeconds(0.06f);
        Destroy(shotObject);
    }

    void HandleMove()
    {
        var kb = Keyboard.current;
        Vector2 input = Vector2.zero;
        if (kb != null)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) input.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) input.y -= 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) input.x -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) input.x += 1f;
        }
        else
        {
            // fallback to legacy Input if Input System is not available
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");
        }

        if (input.sqrMagnitude > 1f) input.Normalize();

        Vector3 localMove = new Vector3(input.x, 0f, input.y);
        Vector3 move = transform.TransformDirection(localMove) * moveSpeed;

        bool grounded = false;
        if (controller != null)
        {
            grounded = controller.isGrounded;
        }
        else
        {
            // very simple ground check fallback
            grounded = Physics.Raycast(transform.position, Vector3.down, 1.1f);
        }

        if (grounded && verticalVelocity < 0f)
        {
            // small downward value keeps the controller grounded
            verticalVelocity = -2f;
        }

        // Jump input
        bool jumpPressed = false;
        if (kb != null)
        {
            jumpPressed = kb.spaceKey.wasPressedThisFrame;
        }
        else
        {
            jumpPressed = Input.GetKeyDown(KeyCode.Space);
        }

        if (jumpPressed && grounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        // apply gravity
        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = move + Vector3.up * verticalVelocity;

        if (controller != null)
        {
            controller.Move(velocity * Time.deltaTime);
        }
        else
        {
            // fallback movement without controller (will not handle collisions)
            transform.position += velocity * Time.deltaTime;
        }
    }

    void HandleLook()
    {
        var mouse = Mouse.current;
        Vector2 delta = Vector2.zero;
        if (mouse != null)
        {
            delta = mouse.delta.ReadValue();
        }
        else
        {
            delta.x = Input.GetAxis("Mouse X") * 100f * Time.deltaTime;
            delta.y = Input.GetAxis("Mouse Y") * 100f * Time.deltaTime;
        }

        // apply sensitivity and delta time for consistent feel
        float dx = delta.x * mouseSensitivity;
        float dy = delta.y * mouseSensitivity;

        yaw += dx;
        pitch -= dy;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
