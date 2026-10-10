using UnityEngine;
using UnityEngine.InputSystem;

namespace NonECS
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 20f;
        [SerializeField] private float edgeMoveSpeed = 20f;
        [SerializeField] private float zoomSpeed = 1f;
        [SerializeField] private float edgeSize = 15f;
        [SerializeField] private float minHeight = 10f;
        [SerializeField] private float maxHeight = 60f;

        private void Update()
        {
            Move();
            MoveByScreenEdge();
            Zoom();
        }

        private void Move()
        {
            Vector2 input = Vector2.zero;

            if (Keyboard.current.wKey.isPressed)
            {
                input.y += 1f;
            }

            if (Keyboard.current.sKey.isPressed)
            {
                input.y -= 1f;
            }

            if (Keyboard.current.dKey.isPressed)
            {
                input.x += 1f;
            }

            if (Keyboard.current.aKey.isPressed)
            {
                input.x -= 1f;
            }

            Vector3 direction = new Vector3(input.x, 0f, input.y).normalized;
            transform.position += direction * (moveSpeed * Time.deltaTime);
        }

        private void MoveByScreenEdge()
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 direction = Vector3.zero;

            if (mousePosition.x <= edgeSize)
            {
                direction.x -= 1f;
            }

            if (mousePosition.x >= Screen.width - edgeSize)
            {
                direction.x += 1f;
            }

            if (mousePosition.y <= edgeSize)
            {
                direction.z -= 1f;
            }

            if (mousePosition.y >= Screen.height - edgeSize)
            {
                direction.z += 1f;
            }

            transform.position += direction.normalized * (edgeMoveSpeed * Time.deltaTime);
        }

        private void Zoom()
        {
            float scroll = Mouse.current.scroll.ReadValue().y;
            Vector3 position = transform.position;

            position.y -= scroll * zoomSpeed;
            position.y = Mathf.Clamp(position.y, minHeight, maxHeight);

            transform.position = position;
        }
    }
}