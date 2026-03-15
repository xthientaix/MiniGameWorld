using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Tank
{
    public class LevelInfo : MonoBehaviour
    {
        [Header("----- Enemy -----")]
        public List<Transform> enemyTypes;
        public List<GameObject> enemyPrefabs;
        public List<int> enemyNumber;
        public List<TextMeshPro> enemyNumberText;
        public List<SpawnArea> spawnAreas;

        private void Awake()
        {
            // Kiểm tra tính nhất quán của dữ liệu
            if (enemyTypes.Count != enemyPrefabs.Count || enemyTypes.Count != enemyNumber.Count || enemyTypes.Count != enemyNumberText.Count)
            {
                Debug.LogError("LevelInfo: Mismatch in the counts of enemyTypes, enemyPrefabs, enemyNumber, or enemyNumberText.");
            }

            GetComponentsInChildren<SpawnArea>(true, spawnAreas);
        }
    }
}
