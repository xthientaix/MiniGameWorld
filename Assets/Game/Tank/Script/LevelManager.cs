using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tank
{
    public class LevelManager : MonoBehaviour
    {
        private int currentLevel;
        private List<LevelInfo> levelInfoList = new();
        private LevelInfo currentLevelInfo;
        private List<int> enemyRemainingNumber = new();
        private List<int> enemyToKill = new();

        [SerializeField] TextMeshPro currentLevelText;

        public int LevelInfoCount => levelInfoList.Count;
        public bool IsEnemyRemain => enemyRemainingNumber.Count > 0;
        public LevelInfo CurrentLevelInfo => currentLevelInfo;

        private void Awake()
        {
            //lấy tất cả LevelInfo trong các con của LevelManager
            GetComponentsInChildren<LevelInfo>(true, levelInfoList);
        }

        private void OnEnable()
        {
            foreach (LevelInfo level in levelInfoList)
            {
                level.gameObject.SetActive(false);
            }
        }

        public int SetUpLevel(int level)
        {
            // Ẩn level trước đó nếu có
            if (currentLevelInfo != null)
            {
                currentLevelInfo.gameObject.SetActive(false);
                // Di chuyển tất cả enemy từ active sang inactive ở từng type
                for (int i = 0; i < currentLevelInfo.enemyTypes.Count; i++)
                {
                    Transform active = currentLevelInfo.enemyTypes[i].GetChild(0);
                    Transform inactive = currentLevelInfo.enemyTypes[i].GetChild(1);
                    Helper.MoveChildren(active, inactive, true);
                }
            }

            currentLevel = level;
            currentLevelInfo = levelInfoList[currentLevel];
            currentLevelText.text = "Level " + (currentLevel + 1).ToString();
            enemyRemainingNumber.Clear();
            enemyToKill.Clear();

            // Lấy thông tin số lượng kẻ thù từ LevelInfo của level hiện tại và cập nhật UI
            int totalEnemies = 0;
            for (int i = 0; i < currentLevelInfo.enemyTypes.Count; i++)
            {
                enemyRemainingNumber.Add(currentLevelInfo.enemyNumber[i]);
                enemyToKill.Add(currentLevelInfo.enemyNumber[i]);
                currentLevelInfo.enemyNumberText[i].text = "x  " + enemyToKill[i].ToString();
                totalEnemies += enemyToKill[i];
            }

            currentLevelInfo.gameObject.SetActive(true);
            return totalEnemies;
        }

        public GameObject GetEnemy()
        {
            if (enemyRemainingNumber.Count == 0)
            {
                return null;
            }

            // Lấy ngẫu nhiên 1 loại kẻ thù còn lại
            int type = Random.Range(0, enemyRemainingNumber.Count);
            GameObject enemyObject;
            Transform active = currentLevelInfo.enemyTypes[type].GetChild(0);
            Transform inactive = currentLevelInfo.enemyTypes[type].GetChild(1);
            if (inactive.childCount > 0)
            {
                enemyObject = inactive.GetChild(0).gameObject;
                enemyObject.transform.SetParent(active);
            }
            else
            {
                enemyObject = Instantiate(currentLevelInfo.enemyPrefabs[type], active);
            }

            // Cập nhật số lượng kẻ thù còn lại trong pool , nếu hết thì loại bỏ loại kẻ thù đó khỏi danh sách
            enemyRemainingNumber[type]--;
            if (enemyRemainingNumber[type] <= 0)
            {
                enemyRemainingNumber.RemoveAt(type);
            }

            enemyObject.SetActive(false);
            return enemyObject;
        }

        public void EnemyDie(GameObject enemy)
        {
            Transform type = enemy.transform.parent.parent;
            // Kiểm tra enemy chết thuộc loại nào và cập nhật số lượng enemy cần giết còn lại. Chuyển nó về inactive
            for (int i = 0; i < currentLevelInfo.enemyTypes.Count; i++)
            {
                if (type == currentLevelInfo.enemyTypes[i])
                {
                    enemyToKill[i]--;
                    currentLevelInfo.enemyNumberText[i].text = "x  " + enemyToKill[i].ToString();
                    enemy.transform.SetParent(type.GetChild(1));
                    break;
                }
            }
        }
    }
}
