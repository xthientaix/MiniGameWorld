using UnityEngine;

namespace SpaceShip
{
    public class Rock : MonoBehaviour
    {
        private Level level;
        const float defaultSpeed = 1.8f;
        float speed;
        const float verticalSpeed = 0.5f;
        float[] verticalMoveRange = new float[2];
        const float verticalOffsetPos = 0.2f;
        const float horizontalOffsetPos = 2f;

        private int verticalMoveState;
        Vector2 newPos;

        Transform poolObject;

        private void Awake()
        {
            poolObject = GameObject.FindGameObjectWithTag("RockPoolObject").transform;
            //gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            transform.SetParent(poolObject.GetChild(0));
        }

        public void Init(Vector3 pos, Level level, float verticalMove, float multipleSpeed)
        {
            this.level = level;
            transform.position = pos + new Vector3(Random.Range(-horizontalOffsetPos, horizontalOffsetPos), Random.Range(-verticalOffsetPos, verticalOffsetPos));

            speed = defaultSpeed * multipleSpeed;

            if (verticalMove == 0 || level < Level.Hard)
            {
                verticalMoveState = 0;
                verticalMoveRange[0] = 0;
                verticalMoveRange[1] = 0;
            }
            else
            {
                verticalMoveState = Random.Range(0, 2) == 0 ? -1 : 1;
                verticalMoveRange[0] = pos.y - verticalMove;
                verticalMoveRange[1] = pos.y + verticalMove;
            }
        }

        private void FixedUpdate()
        {
            // Di chuyển vật cản
            newPos = transform.position + (Time.fixedDeltaTime * new Vector3(-1 * speed, verticalMoveState * verticalSpeed, 0));
            transform.position = Vector2.MoveTowards(transform.position, newPos, 1);

            // Vật cản xoay vòng liên tục theo thời gian nếu level đủ khó (Extreme)
            if (level == Level.Extreme)
            {
                transform.Rotate(0, 0, -120f * Time.fixedDeltaTime);
            }

            // Nếu di chuyển ra khỏi khu vực hiển thị thì ẩn object và trở về pool
            if (transform.position.x < -10f)
            {
                gameObject.SetActive(false);
                transform.SetParent(poolObject.GetChild(1));
                speed = defaultSpeed;
            }

            // Nếu không di chuyển theo phương dọc thì return
            if (verticalMoveState == 0)
            { return; }

            // Nếu vượt quá giới hạn theo phương dọc thì đổi chiều di chuyển
            if (transform.position.y < verticalMoveRange[0] || transform.position.y > verticalMoveRange[1])
            {
                verticalMoveState *= -1;
            }

        }
    }
}