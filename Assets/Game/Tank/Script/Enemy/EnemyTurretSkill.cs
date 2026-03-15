using DG.Tweening;
using UnityEngine;

namespace Tank
{
    public class EnemyTurretSkill : MonoBehaviour
    {
        [SerializeField] float skillCooldown = 10f;
        TurretManager turretManager;
        SpriteRenderer tankRenderer;
        Color originalColor;
        [SerializeField] Color skillColor;

        AudioSource audioSource;
        [SerializeField] AudioClip skillActivateSound;

        private void Awake()
        {
            tankRenderer = GetComponent<SpriteRenderer>();
            originalColor = tankRenderer.color;

            // add thêm component AudioSource nếu chưa có
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        private void Start()
        {
            turretManager = transform.parent.parent.parent.GetComponentInChildren<TurretManager>();
            if (turretManager == null)
            {
                Debug.Log(gameObject.name + " : " + "TurretManager is not found");
            }

            //originalColor = tankRenderer.color;
        }

        private void OnEnable()
        {
            Invoke(nameof(UseSkill), Random.Range(skillCooldown - 3f, skillCooldown + 2f));
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(UseSkill));

            if (turretManager != null)
            {
                turretManager.ResetTurret();
            }
            if (tankRenderer != null)
            {
                tankRenderer.color = originalColor;
            }
        }

        private void UseSkill()
        {
            CancelInvoke(nameof(UseSkill));

            // Kiểm tra nếu tankRenderer hoặc turretManager chưa được gán, log lỗi và thoát
            if (tankRenderer == null)
            {
                Debug.Log("Tank Renderer not assigned!");
                return;
            }
            if (turretManager == null)
            {
                Debug.Log(gameObject.name + " : " + "TurretManager is not assigned.");
                return;
            }

            // Hủy tween cũ nếu đang chạy
            tankRenderer.DOKill();

            // Phát âm thanh kích hoạt kỹ năng
            if (skillActivateSound != null)
            {
                audioSource.PlayOneShot(skillActivateSound);
            }
            else
            {
                Debug.Log("Skill activate sound not assigned in EnemyTurretSkill.");
            }

            Sequence seq = DOTween.Sequence();
            // Nhấp nháy 3 lần (mỗi lần 0.2s đổi màu , 0.2s về màu gốc)
            for (int i = 0; i < 3; i++)
            {
                seq.Append(tankRenderer.DOColor(skillColor, 0.2f));
                seq.Append(tankRenderer.DOColor(originalColor, 0.2f));
            }

            // Sau khi nhấp nháy xong, đảm bảo màu trở về gốc . Kich hoạt turret
            seq.OnComplete(() =>
            {
                tankRenderer.color = originalColor;
                turretManager.ActivateTurret();
            });

            Invoke(nameof(UseSkill), Random.Range(skillCooldown, skillCooldown + 5f));
        }
    }
}