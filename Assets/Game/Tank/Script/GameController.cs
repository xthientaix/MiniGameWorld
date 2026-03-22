using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tank
{
    public class GameController : MonoBehaviour
    {
        private int currentLevel;
        private int totalLevels;
        private LevelManager levelManager;
        [SerializeField] private List<SpawnArea> enemySpawnAreas = new();
        private List<bool> enemySpawnAreaAvailable = new();
        private List<Vector3> enemySpawnPositions = new();
        private int totalEnemies;

        [SerializeField] Transform player;
        [SerializeField] Transform playerSpawnPosition;

        [Header("---- Gameplay ----")]
        [SerializeField] private int maxEnemiesOnField = 5;
        private int currentEnemiesOnField;

        [Space(10)]
        [Header("---- UI ----")]
        [SerializeField] Image blackScreen;
        [SerializeField] SpriteRenderer backgroundImage;
        [SerializeField] SpriteRenderer backgroundColor;
        [SerializeField] Sprite[] backgroundSprites;
        [Space(10)]
        [SerializeField] GameObject startPanel;
        [SerializeField] GameObject completePanel;
        [SerializeField] GameObject pausePanel;
        [SerializeField] TextMeshProUGUI completeText;
        [Space(10)]
        [SerializeField] GameObject replayButton;
        [SerializeField] GameObject nextLevelButton;
        [SerializeField] GameObject pauseButton;

        [Space(10)]
        [Header("---- Sound ----")]
        [SerializeField] AudioClip[] music;
        [SerializeField] AudioClip completeSound;
        [SerializeField] AudioClip defeatSound;
        private AudioSource audioSource;

        // tạo 1 delegate để truyền hàm logic vào , dùng khi mỗi lần load level
        //  +returntopool của bullet , để bullet về pool
        public static event Action OnLevelLoad;

        private void Awake()
        {
            levelManager = GameObject.FindGameObjectWithTag("LevelManager").GetComponent<LevelManager>();
            // enemySpawnAreas.AddRange(GetComponentsInChildren<SpawnArea>());
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            Time.timeScale = 0f;
            startPanel.SetActive(true);
            pausePanel.SetActive(false);
            completePanel.SetActive(false);
            pauseButton.SetActive(false);

            totalLevels = levelManager.LevelInfoCount;
            currentLevel = 0;

            blackScreen.gameObject.SetActive(true);
            totalEnemies = levelManager.SetUpLevel(currentLevel);
            GetSpawnAreasInfo();
            player.position = playerSpawnPosition.position;
            FadeOut();
        }

        private void ChangeBackground()
        {
            // Thay đổi background dựa trên currentLevel, ví dụ: sau 3 level thì đổi background 1 lần
            int backgroundIndex = currentLevel / 3;

            // Thay đổi màu nền background đen hoặc trắng dựa trên backgroundIndex
            // backgroundIndex là số chẵn thì backgroundColor là trắng
            // backgroundIndex là số lẻ thì backgroundColor là đen
            if (backgroundIndex % 2 == 0)
            {
                backgroundColor.color = Color.white;
            }
            else
            {
                backgroundColor.color = Color.black;
            }

            if (backgroundIndex < backgroundSprites.Length)
            {
                backgroundImage.sprite = backgroundSprites[backgroundIndex];
            }
            else
            {
                // Nếu vượt quá số lượng background có sẵn, có thể giữ nguyên background cuối cùng hoặc quay lại background đầu tiên
                backgroundImage.sprite = backgroundSprites[^1];

                // Hoặc quay lại background đầu tiên
                // background.sprite = backgroundSprites[0];
            }
        }

        private void IsBoss()
        {
            // Kiểm tra nếu level hiện tại là level boss thì phát nhạc boss
            // Level boss là bội số của 3 tính từ level 0, tức là level 2, 5, 8...
            int levelForMusic = currentLevel + 1;   // Cộng 1 để tính từ level 1 thay vì level 0
            if (levelForMusic % 3 == 0)
            {
                // Nhạc boss sẽ là nhạc thứ (levelForMusic / 3) - 1 trong mảng music, vì levelForMusic bắt đầu từ 1
                int musicIndex = (levelForMusic / 3) - 1;
                // Kiểm tra nếu musicIndex vượt quá số lượng nhạc có sẵn thì lấy nhạc cuối cùng trong mảng
                if (musicIndex >= music.Length) { musicIndex = music.Length - 1; }

                audioSource.clip = music[musicIndex];
                audioSource.loop = true;
                audioSource.Play();
            }
        }

        public void PressPlay()
        {
            startPanel.transform.DOScale(0.2f, 0.5f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
            {
                startPanel.SetActive(false);
                startPanel.transform.localScale = Vector3.one;
                pauseButton.SetActive(true);
                Time.timeScale = 1f;
                currentEnemiesOnField = 0;

                // Kiểm tra nếu level hiện tại là level boss thì thực hiện hành động liên quan đến boss
                IsBoss();

                // Bắt đầu spawn kẻ thù sau 1 giây, mỗi 2 giây spawn tiếp 1 kẻ thù
                // Repeat để tạo việc spawn liên tục khi khởi đầu level
                // Có điều kiện dừng spawn liên tục trong hàm SpawnEnemy
                InvokeRepeating(nameof(SpawnEnemy), 1f, 2f);
            });
        }

        public void PressNextLevel()
        {
            if (currentLevel < totalLevels - 1)
            {
                currentLevel++;
            }
            else
            {
                // Nếu đã là level cuối cùng thì hiện thông báo đã hoàn thành tất cả level
                Debug.Log("All levels completed!");
            }

            FadeIn(() => levelManager.SetUpLevel(currentLevel));
        }

        public void PressReplayLevel()
        {
            FadeIn(() => levelManager.SetUpLevel(currentLevel));
        }

        public void PressPause()
        {
            if (Time.timeScale == 0f)
            {
                // Resume game
                pausePanel.transform.DOScale(0.2f, 0.5f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() =>
                {
                    pausePanel.SetActive(false);
                    Time.timeScale = 1f;
                    pauseButton.SetActive(true);
                });
                return;
            }

            // Pause game
            Time.timeScale = 0f;
            pauseButton.SetActive(false);
            pausePanel.SetActive(true);
            pausePanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        }

        public void PressHome()
        {
            // Quay về menu chính , load scene menu chính
            Sequence fadeSequence = DOTween.Sequence().SetUpdate(true);
            blackScreen.gameObject.SetActive(true);
            fadeSequence.Append(blackScreen.DOFade(1, 0.5f).SetEase(Ease.OutCubic));
            fadeSequence.AppendCallback(() =>
            {
                SceneManager.LoadScene(0);
            });
        }

        private void SpawnEnemy()
        {
            // Kiểm tra điều kiện spawn
            // Nếu không còn kẻ thù để spawn hoặc đã đạt giới hạn kẻ thù trên field thì dừng việc spawn
            if (!levelManager.IsEnemyRemain || currentEnemiesOnField == maxEnemiesOnField)
            {
                if (currentEnemiesOnField == maxEnemiesOnField)
                {
                    // Dừng toàn bộ việc spawn nếu đã đạt giới hạn kẻ thù trên field
                    Debug.Log("Max enemies on field reached. Cannot spawn more.");
                }
                CancelInvoke(nameof(SpawnEnemy));
                return;
            }

            // Lấy danh sách vị trí spawn khả dụng
            enemySpawnPositions.Clear();
            for (int i = 0; i < enemySpawnAreas.Count; i++)
            {
                if (enemySpawnAreaAvailable[i])
                {
                    enemySpawnPositions.Add(enemySpawnAreas[i].transform.position);
                }
            }
            if (enemySpawnPositions.Count == 0)
            {
                // Không có vị trí spawn khả dụng. Thử lại sau 1 giây.
                Invoke(nameof(SpawnEnemy), 1f);
                return;
            }

            // Chọn ngẫu nhiên 1 vị trí spawn khả dụng và spawn kẻ thù
            Vector3 spawnPosition = enemySpawnPositions[UnityEngine.Random.Range(0, enemySpawnPositions.Count)];
            GameObject enemy = levelManager.GetEnemy();
            enemy.transform.position = spawnPosition;
            enemy.SetActive(true);
            currentEnemiesOnField++;
        }

        private void FadeIn(Func<int> logic)
        {
            Sequence fadeSequence = DOTween.Sequence().SetUpdate(true);
            blackScreen.gameObject.SetActive(true);

            fadeSequence.Append(blackScreen.DOFade(1, 0.5f).SetEase(Ease.OutCubic));
            fadeSequence.AppendCallback(() =>
            {
                // Sau khi fade in hoàn tất, có thể thực hiện các hành động khác nếu cần
                totalEnemies = logic?.Invoke() ?? 0;
                if (totalEnemies > 0)
                {
                    GetSpawnAreasInfo();
                }
                OnLevelLoad?.Invoke(); // Gọi delegate để trả bullet về pool
                completePanel.SetActive(false);
                startPanel.SetActive(true);

                player.gameObject.SetActive(false);
                player.SetPositionAndRotation(playerSpawnPosition.position, Quaternion.Euler(new Vector3(0, 0, 90)));
            }).OnComplete(() => FadeOut());
        }

        private void FadeOut()
        {
            player.gameObject.SetActive(true);
            ChangeBackground();
            blackScreen.DOFade(0, 0.5f).SetEase(Ease.InCubic).SetUpdate(true).OnComplete(() =>
            {
                blackScreen.gameObject.SetActive(false);
            });
        }

        private void GetSpawnAreasInfo()
        {
            enemySpawnAreaAvailable.Clear();

            enemySpawnAreas = levelManager.CurrentLevelInfo.spawnAreas;
            for (int i = 0; i < enemySpawnAreas.Count; i++)
            {
                enemySpawnAreas[i].areaID = i;
                enemySpawnAreaAvailable.Add(true);
            }
        }

        public void SetSpawnAreaAvailability(int areaID, bool isAvailable)
        {
            if (areaID >= 0 && areaID < enemySpawnAreaAvailable.Count)
            {
                enemySpawnAreaAvailable[areaID] = isAvailable;
            }
        }

        private void GameOver(bool isWin)
        {
            Time.timeScale = 0f;
            pauseButton.SetActive(false);

            Sequence sequence = DOTween.Sequence().SetUpdate(true);
            sequence.AppendInterval(0.8f);
            sequence.AppendCallback(() =>
            {
                completeText.text = isWin ? "Completed" : "Defeated";
                nextLevelButton.SetActive(isWin);
                replayButton.SetActive(!isWin);
                completePanel.SetActive(true);
                audioSource.Stop();
                audioSource.loop = false;
                audioSource.PlayOneShot(isWin ? completeSound : defeatSound);

                completePanel.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
            });
        }

        public void PlayerDied()
        {
            GameOver(false);
        }

        public void EnemyDie(GameObject enemy)
        {
            // Cập nhật thông tin cho LevelManager , đồng thời gọi hàm spawn enemy sau 3 giây
            totalEnemies--;
            currentEnemiesOnField--;
            levelManager.EnemyDie(enemy);

            if (totalEnemies <= 0)
            {
                // Hoàn thành level
                Debug.Log("Level " + (currentLevel + 1) + " completed!");
                GameOver(true);
            }
            else
            {
                Invoke(nameof(SpawnEnemy), 3f);
            }
        }
    }
}