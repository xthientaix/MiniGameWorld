using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Tank
{
    public class PlayerController : MonoBehaviour
    {
        private FixedJoystick joystick;
        private GameController gameController;
        private Rigidbody2D rb2D;
        private AudioSource audioSource;

        [Header("----- Setup -----")]
        [SerializeField] private Transform bulletPool;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform canvas;
        [SerializeField] private Image hpBar;
        [SerializeField] private AudioClip shootSound;
        [SerializeField] private AudioClip hitSound;
        private bool hpBarVisible = false;
        private SpriteRenderer spriteRenderer;
        private Color originalColor;


        [Header("----- Gameplay -----")]
        public bool canMove = true;
        [SerializeField] float moveSpeed;
        [SerializeField] private int maxHP;
        private int currentHP;
        [SerializeField] private float shootInterval = 0.3f;
        private bool canShoot = true;
        private Vector2 moveDirection = new();


        private void Awake()
        {
            joystick = GameObject.FindGameObjectWithTag("Joystick").GetComponent<FixedJoystick>();
            gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
            rb2D = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            currentHP = maxHP;
            hpBar.fillAmount = 1f;
            canMove = true;
        }

        private void Start()
        {
            originalColor = spriteRenderer.color;
            hpBar.transform.parent.gameObject.SetActive(false);
        }

        private void FixedUpdate()
        {
            if (joystick.Horizontal == 0 && joystick.Vertical == 0)
            {
                return;
            }

            if (Mathf.Abs(joystick.Horizontal) > Mathf.Abs(joystick.Vertical))
            {
                moveDirection = new Vector2(joystick.Horizontal, 0).normalized;
            }
            else
            {
                moveDirection = new Vector2(0, joystick.Vertical).normalized;
            }

            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);

            if (!canMove) return;
            rb2D.MovePosition(rb2D.position + (moveSpeed * Time.fixedDeltaTime * (Vector2)transform.right));
        }

        private void LateUpdate()
        {
            if (hpBarVisible)
            {
                // cập nhật vị trí canvas của thanh máu
                UpdateHPBarCanvasLocation();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("EnemyBullet"))
            {
                Shot();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {

        }

        private void Shot()
        {
            currentHP--;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            if (currentHP == 0)
            {
                gameObject.SetActive(false);
                gameController.PlayerDied();
                return;
            }

            if (hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
            else
            {
                Debug.Log("Hit sound not assigned in PlayerController.");
            }

            hpBar.fillAmount = (float)currentHP / maxHP;
            if (!hpBarVisible)
            {
                hpBarVisible = true;
                hpBar.transform.parent.gameObject.SetActive(true);
            }
            CancelInvoke(nameof(HideHPBar));
            Invoke(nameof(HideHPBar), 2f);

            // Hủy tween cũ nếu đang chạy
            spriteRenderer.DOKill();

            // Đổi màu sang đỏ nhạt , tween về màu gốc sau 0.5s
            spriteRenderer.DOColor(Color.Lerp(originalColor, Color.red, 0.5f), 0.1f)
                .OnComplete(() =>
                {
                    spriteRenderer.DOColor(originalColor, 0.2f).SetDelay(0.3f);
                });
        }

        public void Root(float duaration)
        {
            CancelInvoke(nameof(Unroot));
            canMove = false;
            Invoke(nameof(Unroot), duaration);
        }

        private void Unroot()
        {
            canMove = true;
        }

        public void Shoot()
        {
            if (!canShoot || Time.timeScale == 0) return;

            if (bulletPool.childCount > 0)
            {
                Transform bullet = bulletPool.GetChild(0);
                bullet.SetPositionAndRotation(bulletPool.position, transform.rotation);
                bullet.SetParent(null);
                bullet.gameObject.SetActive(true);
            }
            else
            {
                GameObject bullet = Instantiate(bulletPrefab, bulletPool.position, transform.rotation, null);
                bullet.GetComponent<Bullet>().SetBulletPool(bulletPool);
                bullet.SetActive(true);
            }

            if (shootSound != null)
            {
                audioSource.PlayOneShot(shootSound);
            }
            else
            {
                Debug.Log("Shoot sound not assigned in PlayerController.");
            }

            canShoot = false;
            Invoke(nameof(CanShoot), shootInterval);
        }

        private void CanShoot()
        {
            canShoot = true;
        }

        public void UpdateHPBarCanvasLocation()
        {
            canvas.SetPositionAndRotation(transform.position + new Vector3(0, 0.7f, 0), Quaternion.identity);
        }

        private void HideHPBar()
        {
            hpBar.transform.parent.gameObject.SetActive(false);
            hpBarVisible = false;
        }
    }
}