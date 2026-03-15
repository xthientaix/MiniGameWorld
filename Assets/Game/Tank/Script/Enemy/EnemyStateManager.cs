using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tank
{
    public abstract class EnemyStateManager<T> : MonoBehaviour where T : EnemyStateManager<T>
    {
        public BaseState<T> currentState;
        public EnemyMovingState<T> enemyMovingState;
        public EnemyLookingState<T> enemyLookingState;
        public EnemyFollowingState<T> enemyFollowingState;

        public Rigidbody2D rb2D;
        protected GameController gameController;

        public Transform target = default;

        [Space(10)]
        [Header("----- Setup -----")]
        [SerializeField] protected Transform canvas;
        [SerializeField] protected Image hpBar;
        protected bool hpBarVisible = default;
        protected SpriteRenderer spriteRenderer;
        protected Color originalColor;

        [Header("----- Gameplay -----")]
        public float moveSpeed = 2f;
        [SerializeField] protected int maxHP;
        protected int currentHP;

        // khởi tạo danh sách các hướng di chuyển với 4 hướng chính
        public readonly List<Vector2> moveDirections = new() { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        protected virtual void Awake()
        {
            rb2D = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        }

        protected virtual void Start()
        {
            originalColor = spriteRenderer.color;
            hpBar.transform.parent.gameObject.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            currentHP = maxHP;
            hpBar.fillAmount = 1f;
        }

        protected virtual void OnDisable()
        {
        }

        protected void FixedUpdate()
        {
            currentState?.UpdateState();
        }

        private void LateUpdate()
        {
            if (hpBarVisible)
            {
                // cập nhật vị trí canvas của thanh máu
                UpdateHPBarCanvasLocation();
            }
        }

        protected void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("PlayerBullet"))
            {
                Shot();
                currentState.OnCollisionEnter2D(collision);
                return;
            }

            //currentState.OnCollisionEnter2D(collision);
        }

        protected void OnCollisionExit2D(Collision2D collision)
        {
            currentState.OnCollisionExit2D(collision);
        }

        protected void OnTriggerEnter2D(Collider2D collision)
        {
            currentState.OnTriggerEnter2D(collision);
        }

        protected void OnTriggerExit2D(Collider2D collision)
        {
            currentState.OnTriggerExit2D(collision);
        }

        private void Shot()
        {
            currentHP--;
            currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            if (currentHP == 0)
            {
                gameObject.SetActive(false);
                gameController.EnemyDie(gameObject);
                return;
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

        public void SwitchState(BaseState<T> newState)
        {
            currentState?.ExitState();
            currentState = newState;
            currentState.EnterState((T)this);
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