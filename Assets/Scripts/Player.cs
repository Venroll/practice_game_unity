using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private float speed = 0f;
    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new InputSystem_Actions() ;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 move = inputActions.Player.Move.ReadValue<Vector2>();
        rb.MovePosition(rb.position + move * speed * Time.fixedDeltaTime);
    }
}
