using UnityEngine;

namespace Tank
{
    public class EnemyShoot : MonoBehaviour
    {
        [SerializeField] private Transform bulletPool;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float minShootInterval = 1f;
        [SerializeField] private float maxShootInterval = 2.5f;

        private void OnEnable()
        {
            Invoke(nameof(Shoot), Random.Range(minShootInterval, maxShootInterval));
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(Shoot));
        }

        private void Shoot()
        {
            if (bulletPool.childCount > 0)
            {
                Transform bullet = bulletPool.GetChild(0);
                bullet.SetPositionAndRotation(bulletPool.position, transform.rotation);
                bullet.SetParent(null);
                bullet.gameObject.SetActive(true);
            }
            else
            {
                GameObject bullet = Instantiate(bulletPrefab, bulletPool.position, transform.rotation, null);
                bullet.GetComponent<Bullet>().SetBulletPool(bulletPool);
                bullet.SetActive(true);
            }

            Invoke(nameof(Shoot), Random.Range(minShootInterval, maxShootInterval));
        }
    }
}