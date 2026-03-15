using System;
using System.Collections;
using UnityEngine;

namespace Tank
{
    public class EnemyFollowingState<T> : BaseState<T> where T : EnemyStateManager<T>
    {
        private readonly float lookAtTargetInterval = 1.3f;
        private readonly float outSightTime = 1.5f;

        public GameObject alertIcon;
        private Coroutine lookCoroutine;
        private Coroutine stopFollowCoroutne;

        public override void EnterState(T stateManager)
        {
            base.EnterState(stateManager);
            lookCoroutine = stateManager.StartCoroutine(WaitAndDoRepeat(lookAtTargetInterval, LookAtTarget));
        }

        public override void UpdateState()
        {
            stateManager.rb2D.MovePosition(stateManager.rb2D.position + (stateManager.moveSpeed * Time.fixedDeltaTime * (Vector2)stateManager.transform.right));
        }

        public override void ExitState()
        {
            //stateManager.CancelInvoke(nameof(LookAtTarget));
            stateManager.StopCoroutine(lookCoroutine);
            lookCoroutine = null;
            base.ExitState();
        }

        public override void OnCollisionEnter2D(Collision2D collision)
        {

        }

        public override void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                if (stopFollowCoroutne != null)
                {
                    stateManager.StopCoroutine(stopFollowCoroutne);
                    stopFollowCoroutne = null;
                }
            }
        }

        public override void OnCollisionExit2D(Collision2D collision)
        {

        }

        public override void OnTriggerExit2D(Collider2D collision)
        {
            if (stateManager.gameObject.activeInHierarchy == false)
            {
                return;
            }

            if (collision.CompareTag("Player"))
            {
                //stateManager.Invoke(nameof(StopFollowing), outSightTime);
                stopFollowCoroutne = stateManager.StartCoroutine(WaitAndDo(outSightTime, StopFollowing));
            }
        }

        private void LookAtTarget()
        {
            // nhìn về phía mục tiêu nhưng giới hạn 4 hướng chính
            Vector2 directionToTarget = (stateManager.target.position - stateManager.transform.position).normalized;
            float angle = Mathf.Atan2(directionToTarget.y, directionToTarget.x) * Mathf.Rad2Deg;
            float roundedAngle = Mathf.Round(angle / 90f) * 90f;
            stateManager.transform.rotation = Quaternion.Euler(0, 0, roundedAngle);
            alertIcon.transform.rotation = Quaternion.identity;

            //stateManager.Invoke(nameof(LookAtTarget), lookAtTargetInterval);
        }

        private IEnumerator WaitAndDoRepeat(float time, Action action)
        {
            action?.Invoke();

            while (true)
            {
                yield return new WaitForSeconds(time);

                // nếu stateManager inactive hoặc ko có action nào thì dừng, kết thúc coroutine
                if (stateManager.gameObject.activeInHierarchy == false || action == null)
                {
                    yield break;
                }

                action?.Invoke();
            }
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

        private void StopFollowing()
        {
            alertIcon.SetActive(false);
            stateManager.target = null;
            stateManager.SwitchState(stateManager.enemyMovingState);
        }
    }
}