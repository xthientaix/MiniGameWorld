using UnityEngine;

namespace SpaceShip
{
    public enum SkillType
    {
        Invincible,
        InvertControl
    }

    public class Skill : MonoBehaviour
    {
        /* ----- Skill/InvertControl đc tạo và di chuyển sang trái với tốc độ cố định ----- */


        public SkillType skillType;
        [SerializeField] float skillSpeed;

        Transform poolObject;

        private void Awake()
        {
            poolObject = GameObject.FindGameObjectWithTag("SkillPoolObject").transform;
            gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (skillType == SkillType.Invincible)
            {
                transform.SetParent(poolObject.GetChild(0).GetChild(0));
            }
            else
            {
                transform.SetParent(poolObject.GetChild(1).GetChild(0));
            }
        }

        private void FixedUpdate()
        {
            // Di chuyển object sang trái
            transform.position += skillSpeed * Time.fixedDeltaTime * Vector3.left;

            // Nếu di chuyển ra khỏi khu vực hiển thị thì ẩn object và trở về pool
            if (transform.position.x < -9f)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                if (skillType == SkillType.Invincible)
                {
                    transform.SetParent(poolObject.GetChild(0).GetChild(1));
                }
                else
                {
                    transform.SetParent(poolObject.GetChild(1).GetChild(1));
                }

                // Ẩn object
                gameObject.SetActive(false);
            }
        }
    }
}