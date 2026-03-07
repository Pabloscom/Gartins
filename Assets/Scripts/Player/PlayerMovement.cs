using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private Animator animator;
    private InputAction moveAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        moveAction = GameInput.Instance.MoveAction;
    }

    void OnEnable()
    {
        if (moveAction == null)
            moveAction = GameInput.Instance.MoveAction;
    }

    void Update()
    {
        if (moveAction != null)
            movement = moveAction.ReadValue<Vector2>();

        if (animator != null)
        {
            animator.SetFloat("MoveX", movement.x);
            animator.SetFloat("MoveY", movement.y);
            animator.SetFloat("Speed", movement.sqrMagnitude);
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        rb.linearVelocity = movement.normalized * speed;
    }
}
