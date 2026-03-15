using UnityEngine;

namespace Tank
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        private Transform bulletPoolObject;

        private Rigidbody2D rb2D;

        private void Awake()
        {
            rb2D = GetComponent<Rigidbody2D>();
        }

        void FixedUpdate()
        {
            // Di chuyển viên đạn về phía trước (theo hướng của trục x cục bộ)
            rb2D.MovePosition(rb2D.position + (speed * Time.fixedDeltaTime * (Vector2)transform.right));
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            ReturnToPool();
        }

        public void SetBulletPool(Transform pool)
        {
            bulletPoolObject = pool;
        }

        private void ReturnToPool()
        {
            gameObject.SetActive(false);
            if (bulletPoolObject != null)
            {
                transform.SetParent(bulletPoolObject);
            }
        }

        private void OnEnable()
        {
            GameController.OnLevelLoad += ReturnToPool;
        }

        private void OnDisable()
        {
            GameController.OnLevelLoad -= ReturnToPool;
        }
    }
}