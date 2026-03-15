using System.Collections;
using UnityEngine;

namespace Tank
{
    public class EnemyMovingState<T> : BaseState<T> where T : EnemyStateManager<T>
    {
        public bool canFindPlayer = false;
        private Coroutine changeDirectionCoroutine;

        public override void EnterState(T stateManager)
        {
            base.EnterState(stateManager);
            RandomDirection();
            changeDirectionCoroutine = stateManager.StartCoroutine(ChangeDirectionRoutine());
        }

        public override void UpdateState()
        {
            stateManager.rb2D.MovePosition(stateManager.rb2D.position + (stateManager.moveSpeed * Time.fixedDeltaTime * (Vector2)stateManager.transform.right));
        }

        public override void ExitState()
        {
            if (changeDirectionCoroutine != null)
            {
                stateManager.StopCoroutine(changeDirectionCoroutine);
                changeDirectionCoroutine = null;
            }

            base.ExitState();
        }

        public override void OnCollisionEnter2D(Collision2D collision)
        {
            if (stateManager.gameObject.activeInHierarchy == false)
            {
                return;
            }

            if (collision.gameObject.CompareTag("PlayerBullet"))
            {
                stateManager.StopCoroutine(changeDirectionCoroutine);
                RandomDirection();
                changeDirectionCoroutine = stateManager.StartCoroutine(ChangeDirectionRoutine());
                return;
            }
        }

        public override void OnTriggerEnter2D(Collider2D collision)
        {
            if (canFindPlayer && collision.CompareTag("Player"))
            {
                stateManager.target = collision.transform;
                stateManager.SwitchState(stateManager.enemyLookingState);
            }
        }

        public override void OnCollisionExit2D(Collision2D collision)
        {

        }

        public override void OnTriggerExit2D(Collider2D collision)
        {

        }

        private void RandomDirection()
        {
            // chọn ngẫu nhiên một hướng di chuyển từ danh sách
            int randomIndex = Random.Range(0, stateManager.moveDirections.Count);
            Vector2 randomDirection = stateManager.moveDirections[randomIndex];

            // xoay theo hướng đã chọn
            float angle = Mathf.Atan2(randomDirection.y, randomDirection.x) * Mathf.Rad2Deg;
            stateManager.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private IEnumerator ChangeDirectionRoutine()
        {
            while (true)
            {
                float delay = Random.Range(1f - (stateManager.moveSpeed - 2f), 2f - (stateManager.moveSpeed - 2f));
                yield return new WaitForSeconds(delay);
                RandomDirection();
            }
        }
    }
}