using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tank
{
    public class TurretManager : MonoBehaviour
    {
        [SerializeField] private Transform bulletPool;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private List<Transform> turrets = new();

        [Space(10)]
        [Header("----- Sound -----")]
        [SerializeField] private AudioClip fireSound;
        private AudioSource audioSource;

        [Space(10)]
        [Header("----- Gun Settings -----")]
        [Space(5)]
        [SerializeField] private float firePointDistance;           // điểm bắn của súng nhỏ
        [SerializeField] private float extendDistance = 1f;         // khoảng cách súng chìa ra
        [SerializeField] private float extendDuration = 0.5f;       // thời gian chìa ra/thụt vào
        [SerializeField] private float fireDelay = 0.3f;            // độ trễ sau khi súng chìa ra trước khi bắn
        [SerializeField] private float fireInterval = 0.3f;         // khoảng cách giữa các loạt đạn
        [SerializeField] private int burstCount = 3;                // số loạt đạn

        private List<Transform> guns;       // danh sách các súng nhỏ

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            // Thêm tất cả súng nhỏ trong các turret vào danh sách guns
            guns = new List<Transform>();
            foreach (Transform turret in turrets)
            {
                foreach (Transform gun in turret)
                {
                    guns.Add(gun);
                }
            }
        }

        public void ActivateTurret()
        {
            StartCoroutine(TurretRoutine());
        }

        public void ResetTurret()
        {
            StopAllCoroutines();
            foreach (Transform gun in guns)
            {
                gun.localPosition = gun.right * 0.25f;
            }
        }

        private IEnumerator TurretRoutine()
        {
            if (guns == null || guns.Count == 0)
            {
                Debug.LogWarning("No guns assigned to TurretManager.");
                yield break;
            }

            // 1. Súng chìa ra
            foreach (Transform gun in guns)
            {
                Vector3 targetPos = gun.localPosition + gun.right * extendDistance;
                gun.DOLocalMove(targetPos, extendDuration);
            }
            yield return new WaitForSeconds(extendDuration + fireDelay);

            // 2. Bắn 3 loạt đạn
            for (int i = 0; i < burstCount; i++)
            {
                FireAllGuns();
                if (fireSound != null)
                {
                    audioSource.PlayOneShot(fireSound);
                }
                else
                {
                    Debug.Log("Fire sound not assigned in TurretManager.");
                }
                yield return new WaitForSeconds(fireInterval);
            }

            // 3. Súng thụt vào
            foreach (Transform gun in guns)
            {
                Vector3 targetPos = gun.localPosition - gun.right * extendDistance;
                gun.DOLocalMove(targetPos, extendDuration); // giả sử vị trí ban đầu là (0,0,0) local
            }
        }

        private void FireAllGuns()
        {
            foreach (Transform gun in guns)
            {
                if (bulletPool.childCount > 0)
                {
                    Transform bullet = bulletPool.GetChild(0);
                    bullet.SetPositionAndRotation(gun.position + gun.right * firePointDistance, gun.rotation);
                    bullet.SetParent(null);
                    bullet.gameObject.SetActive(true);
                }
                else
                {
                    GameObject bullet = Instantiate(bulletPrefab, gun.position + gun.right * firePointDistance, gun.rotation, null);
                    bullet.GetComponent<Bullet>().SetBulletPool(bulletPool);
                    bullet.SetActive(true);
                }
            }
        }
    }
}