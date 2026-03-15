namespace Tank
{
    public class Enemy2Manager : EnemyStateManager<Enemy2Manager>
    {
        protected override void Awake()
        {
            base.Awake();
            enemyMovingState = new() { canFindPlayer = true };
            enemyLookingState = new();
            enemyFollowingState = new();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            currentState = enemyMovingState;
            currentState.EnterState(this);
        }
    }
}