using UnityEngine;

public class ScreenController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 10f;

    private void Update()
    {
        Vector3 movement = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            movement += new Vector3(0f, 1f, 0f);

        if (Input.GetKey(KeyCode.S))
            movement += new Vector3(0f, -1f, 0f);

        if (Input.GetKey(KeyCode.A))
            movement += new Vector3(-1f, 0f, 0f);

        if (Input.GetKey(KeyCode.D))
            movement += new Vector3(1f, 0f, 0f);

        movement = movement.normalized;

        transform.position += movement * moveSpeed * Time.deltaTime;
    }
}