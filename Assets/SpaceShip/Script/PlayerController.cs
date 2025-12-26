using TMPro;
using UnityEngine;

namespace SpaceShip
{
    public class PlayerController : MonoBehaviour
    {
        FixedJoystick joystick;
        [SerializeField] Vector2 limitSpace = new(8.3f, 4.6f);
        [SerializeField] float spaceShipSpeed;

        [Header("-----Skill-----")]
        [SerializeField] float skillDuration;
        [SerializeField] float InvertDuration;
        [SerializeField] TextMeshProUGUI invertTimeText;
        float remainInvertTime;

        GameController gameController;
        SpriteRenderer spriteRenderer;
        Collider2D col;
        int invertState = 1; // 1: normal, -1: inverted

        Vector2 predictPos;

        private void Awake()
        {
            joystick = GameObject.FindGameObjectWithTag("Joystick").GetComponent<FixedJoystick>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();

            // Tìm tag "GameController" và lấy component GameController
            gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        }

        private void Start()
        {
            ResetPlayer();
        }

        private void Update()
        {
            // Cập nhật thời gian đảo ngược điều khiển
            if (invertState == -1)
            {
                remainInvertTime -= Time.deltaTime;
                if (remainInvertTime <= 0)
                {
                    invertState = 1;
                    invertTimeText.gameObject.SetActive(false);
                }
                else
                {
                    invertTimeText.text = (Mathf.Round(remainInvertTime * 10f) / 10f).ToString();
                }
            }
        }

        private void FixedUpdate()
        {
            predictPos = transform.position + spaceShipSpeed * Time.fixedDeltaTime * new Vector3(joystick.Horizontal, joystick.Vertical * invertState, 0);

            if (predictPos.x < -limitSpace.x || predictPos.x > limitSpace.x)
            {
                predictPos.x = transform.position.x;
            }
            if (predictPos.y < -limitSpace.y || predictPos.y > limitSpace.y)
            {
                predictPos.y = transform.position.y;
            }

            transform.position = Vector2.MoveTowards(transform.position, predictPos, 1);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Skill"))
            {
                if (collision.gameObject.GetComponent<Skill>().skillType == SkillType.Invincible)
                {
                    // Làm SpaceShip trong suốt 50% và đi xuyên vật cản trong 5 giây
                    spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
                    col.isTrigger = true;

                    // Kích hoạt kỹ năng trong GameController
                    gameController.ActivateSkill(skillDuration);

                    CancelInvoke(nameof(SpaceShipBackToNormal));
                    Invoke(nameof(SpaceShipBackToNormal), skillDuration);
                }
                else
                {
                    // Đảo ngược điều khiển trong 5 giây
                    invertState = -1;
                    remainInvertTime = InvertDuration;
                    invertTimeText.gameObject.SetActive(true);
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Rock"))
            {
                gameController.GameOver();
            }
        }

        private void SpaceShipBackToNormal()
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            col.isTrigger = false;
        }

        public void ResetPlayer()
        {
            SpaceShipBackToNormal();
            gameController.DeActivSkill();
            invertState = 1;
            remainInvertTime = 0;
            invertTimeText.gameObject.SetActive(false);
        }
    }
}
