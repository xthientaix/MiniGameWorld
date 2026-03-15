using System;
using System.Collections;
using UnityEngine;

namespace Tank
{
    public class EnemyLookingState<T> : BaseState<T> where T : EnemyStateManager<T>
    {
        bool isPlayerInSight;
        private readonly float lookDuration = 1f;

        public GameObject alertIcon;

        public override void EnterState(T stateManager)
        {
            base.EnterState(stateManager);
            isPlayerInSight = true;
            LookAtTarget();
        }

        public override void UpdateState()
        {

        }

        public override void ExitState()
        {

        }

        public override void OnCollisionEnter2D(Collision2D collision)
        {

        }

        public override void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                isPlayerInSight = true;
            }
        }

        public override void OnCollisionExit2D(Collision2D collision)
        {

        }

        public override void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                isPlayerInSight = false;
            }
        }

        private void LookAtTarget()
        {
            // nhìn về phía mục tiêu nhưng giới hạn 4 hướng chính
            Vector2 directionToTarget = (stateManager.target.position - stateManager.transform.position).normalized;
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            float roundedAngle = Mathf.Round(angle / 90f) * 90f;
            stateManager.transform.rotation = Quaternion.Euler(0, 0, roundedAngle);
            alertIcon.SetActive(true);
            alertIcon.transform.rotation = Quaternion.identity;

            stateManager.StartCoroutine(WaitAndDo(lookDuration, SwitchToFollowingState));
        }

        private IEnumerator WaitAndDo(float time, Action action)
        {
            yield return new WaitForSeconds(time);

            // nếu stateManager inactive hoặc ko có action nào thì dừng, kết thúc coroutine
            if (stateManager.gameObject.activeInHierarchy == false || action == null)
            {
                yield break;
            }
            action?.Invoke();
        }

        private void SwitchToFollowingState()
        {
            if (isPlayerInSight)
            {
                stateManager.SwitchState(stateManager.enemyFollowingState);
            }
            else
            {
                alertIcon.SetActive(false);
                stateManager.target = null;
                stateManager.SwitchState(stateManager.enemyMovingState);
            }
        }
    }
}