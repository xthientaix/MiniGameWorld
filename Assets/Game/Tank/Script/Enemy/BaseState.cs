using UnityEngine;

namespace Tank
{
    public abstract class BaseState<T>
    {
        protected T stateManager;

        public virtual void EnterState(T stateManager)
        {
            this.stateManager = stateManager;
        }

        public abstract void UpdateState();

        public virtual void ExitState()
        {
            this.stateManager = default;
        }

        public abstract void OnCollisionEnter2D(Collision2D collision);

        public abstract void OnCollisionExit2D(Collision2D collision);


        public abstract void OnTriggerEnter2D(Collider2D collision);

        public abstract void OnTriggerExit2D(Collider2D collision);
    }
}