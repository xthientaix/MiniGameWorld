using UnityEngine;

namespace Tank
{
    public class Enemy1Manager : EnemyStateManager<Enemy1Manager>
    {
        [SerializeField] private bool canFindPlayer;
        public GameObject alertIcon;

        protected override void Awake()
        {
            base.Awake();
            enemyMovingState = new()
            {
                canFindPlayer = this.canFindPlayer,
            };

            if (canFindPlayer)
            {
                enemyLookingState = new() { alertIcon = this.alertIcon };
                enemyFollowingState = new() { alertIcon = this.alertIcon };
            }
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (alertIcon != null)
            {
                alertIcon.SetActive(false);
            }
            currentState = enemyMovingState;
            currentState.EnterState(this);
        }
    }
}