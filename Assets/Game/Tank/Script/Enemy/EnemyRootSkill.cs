using UnityEngine;

namespace Tank
{
    public class EnemyRootSkill : MonoBehaviour
    {
        [SerializeField] GameObject skillObject;
        [SerializeField] float skillDuration = 1.5f;
        [SerializeField] float rootDuration = 2f;
        [SerializeField] float skillCooldown = 10f;

        private void OnEnable()
        {
            Invoke(nameof(UseSkill), Random.Range(skillCooldown - 3f, skillCooldown + 2f));
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(UseSkill));
            CancelInvoke(nameof(EndSkill));
        }

        private void UseSkill()
        {
            CancelInvoke(nameof(UseSkill));
            if (skillObject == null)
            {
                Debug.Log(gameObject.name + " : " + "Skill object is not assigned.");
                return;
            }

            skillObject.SetActive(true);
            Invoke(nameof(EndSkill), skillDuration);
        }

        private void EndSkill()
        {
            CancelInvoke(nameof(EndSkill));
            skillObject.SetActive(false);
            Invoke(nameof(UseSkill), Random.Range(skillCooldown, skillCooldown + 5f));
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                // Apply root effect to player
                if (collision.gameObject.TryGetComponent<PlayerController>(out var player))
                {
                    player.Root(rootDuration);
                }
            }
        }
    }
}