using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceShip
{
    public enum Level
    {
        Easy,
        Medium,
        Hard,
        Extreme
    }

    public class GameController : MonoBehaviour
    {
        private readonly List<int> rockPerSpawn = new() { 0, 1, 2, 3, 4 };
        private readonly List<float> weights = new() { 15f, 25f, 25f, 25f, 10f };
        private Level currentLevel = Level.Easy;

        [SerializeField] GameObject joyStick;

        [Space(10)]
        [Header("----GamePlay Setting----")]
        [SerializeField] float levelUpTime = 20f;
        [SerializeField] float spawnRockTime = 3f;
        [SerializeField] float offsetSpawnRockTime = 0.5f;
        [SerializeField] float verticalMoveRock = 0.3f;
        [SerializeField] float[] multipleSpeed = new float[4];

        [Space(10)]
        Transform rockPoolObject;
        Transform skillPoolObject;
        [SerializeField] List<GameObject> rockPrefabs;
        [SerializeField] List<GameObject> skillPrefabs;
        [SerializeField] List<Vector3> rockSpawnPositions = new();
        [SerializeField] Vector3 playerSpawnPlace;

        [Header("----BackGround----")]
        [SerializeField] Transform[] backGround = new Transform[2];
        [SerializeField] Sprite[] backGroundImage;
        [SerializeField] float backGroundSpeed;
        float bgWidth;

        [Header("----Sound----")]
        [SerializeField] AudioClip music;
        [SerializeField] AudioClip deathSound;

        [Header("----Panel----")]
        [SerializeField] GameObject startPanel;
        [SerializeField] GameObject pausePanel;
        [SerializeField] GameObject gameOverPanel;
        [SerializeField] GameObject blurScreen;
        [SerializeField] Image blackScreen;
        [SerializeField] Image lvlUpImage;

        [Header("----Button----")]
        [SerializeField] GameObject pauseButton;

        [Header("----Text----")]
        [SerializeField] TextMeshProUGUI scoreText;
        [SerializeField] TextMeshProUGUI skillTimeText;
        [SerializeField] TextMeshProUGUI highestScore;
        [SerializeField] TextMeshProUGUI finalScore;

        AudioSource audioSource;

        bool isPause;
        bool isSkilling;

        float score;
        float skillTime;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            rockPoolObject = GameObject.FindGameObjectWithTag("RockPoolObject").transform;
            skillPoolObject = GameObject.FindGameObjectWithTag("SkillPoolObject").transform;
        }

        private void Start()
        {
            Time.timeScale = 0;
            currentLevel = Level.Easy;

            blackScreen.gameObject.SetActive(true);

            blurScreen.SetActive(true);
            startPanel.SetActive(true);
            pausePanel.SetActive(true);
            gameOverPanel.SetActive(true);
            //pausePanel.SetActive(false);
            //gameOverPanel.SetActive(false);
            lvlUpImage.gameObject.SetActive(false);
            pauseButton.SetActive(false);
            scoreText.gameObject.SetActive(false);
            skillTimeText.gameObject.SetActive(false);

            blackScreen.DOFade(0, 0.8f).SetEase(Ease.OutCubic).SetUpdate(true).OnComplete(() =>
            {
                blackScreen.gameObject.SetActive(false);
            });

            if (backGround[0] == null)
            {
                Debug.Log("Chưa gắn Background");
            }
            else
            {
                ResetBackGround();
            }

            SetUpAudio(music, true);
            audioSource.Play();
        }

        private void Update()
        {
            // Di chuyển background lien tục sang trái
            foreach (Transform bg in backGround)
            {
                bg.position += backGroundSpeed * Time.deltaTime * Vector3.left;

                if (bg.position.x <= -bgWidth)
                {
                    bg.position += new Vector3(bgWidth * backGround.Length, 0, 0);
                }
            }

            if (!isPause)
            {
                score += Time.deltaTime;
                //làm tròn điểm 2 chữ số thập phân
                scoreText.text = "Time : " + Mathf.Round(score * 100f) / 100f;
            }

            if (isSkilling)
            {
                skillTime -= Time.deltaTime;
                if (skillTime <= 0)
                {
                    skillTime = 0;
                    isSkilling = false;
                    skillTimeText.gameObject.SetActive(false);
                }
                else
                {
                    //làm tròn thời gian kỹ năng 1 chữ số thập phân
                    skillTimeText.text = "Skill : " + Mathf.Round(skillTime * 10f) / 10f;
                }
            }
        }

        public void ActivateSkill(float time)
        {
            skillTime = time;
            if (isSkilling) return;

            isSkilling = true;
            skillTimeText.gameObject.SetActive(true);
        }

        public void DeActivSkill()
        {
            if (!isSkilling) return;

            skillTime = 0;
            isSkilling = false;
            skillTimeText.gameObject.SetActive(false);
        }

        public void PressRestartButton()
        {
            Sequence sequence = DOTween.Sequence().SetUpdate(true);
            blackScreen.gameObject.SetActive(true);
            sequence.Append(blackScreen.DOFade(1, 0.5f).SetEase(Ease.InCubic));
            sequence.AppendCallback(() =>
            {
                pauseButton.SetActive(false);
                //pausePanel.SetActive(false);
                //gameOverPanel.SetActive(false);
                pausePanel.transform.localPosition = new Vector3(0, 800, 0);
                gameOverPanel.transform.localPosition = new Vector3(0, 800, 0);
                startPanel.SetActive(true);
                ResetRockAndSkill();
                ResetBackGround();
                score = 0;
                currentLevel = Level.Easy;

                // Tìm player với tag "Player" và đặt vị trí về vị trí spawn
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                player.transform.position = playerSpawnPlace;
                player.GetComponent<PlayerController>().ResetPlayer();
            });
            sequence.Append(blackScreen.DOFade(0, 0.5f).SetEase(Ease.InCubic));
            sequence.OnComplete(() =>
            {
                blackScreen.gameObject.SetActive(false);
                SetUpAudio(music, true);
                audioSource.Play();
            });
        }

        public void PressStartButton()
        {
            startPanel.SetActive(false);
            pauseButton.SetActive(true);
            blurScreen.SetActive(false);
            score = 0;
            scoreText.gameObject.SetActive(true);

            Time.timeScale = 1;
            isPause = false;

            Invoke(nameof(SpawnRock), spawnRockTime);
            Invoke(nameof(LevelUp), levelUpTime - spawnRockTime * 2);
        }

        public void PressPauseButton()
        {
            isPause = !isPause;

            Sequence seq = DOTween.Sequence().SetUpdate(true);
            if (isPause)
            {
                pauseButton.SetActive(!isPause);
                blurScreen.SetActive(isPause);
                Time.timeScale = 0;
                seq.Append(pausePanel.transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutCubic));
            }
            else
            {
                seq.Append(pausePanel.transform.DOLocalMoveY(800, 0.5f).SetEase(Ease.InCubic));
                seq.AppendCallback(() =>
                {
                    blurScreen.SetActive(isPause);
                    Time.timeScale = 1;
                    pauseButton.SetActive(!isPause);
                });
            }
        }

        public void GameOver()
        {
            Time.timeScale = 0;
            CancelInvoke(nameof(SpawnRock));
            CancelInvoke(nameof(LevelUp));

            pauseButton.SetActive(false);
            scoreText.gameObject.SetActive(false);
            skillTimeText.gameObject.SetActive(false);

            audioSource.Stop();
            SetUpAudio(deathSound, false);
            audioSource.Play();

            ShowScore();
        }

        private void ShowScore()
        {
            if (score > PlayerPrefs.GetFloat("HighestScore", 0))
            {
                PlayerPrefs.SetFloat("HighestScore", score);
            }
            highestScore.text = "Highest : " + (Mathf.Round(PlayerPrefs.GetFloat("HighestScore", 0) * 100f) / 100f) + "s";
            finalScore.text = "Your Time : " + (Mathf.Round(score * 100f) / 100f) + "s";

            Sequence seq = DOTween.Sequence().SetUpdate(true);
            seq.AppendInterval(0.8f);
            seq.AppendCallback(() => blurScreen.SetActive(true));
            seq.Append(gameOverPanel.transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutCubic));
        }

        private void ResetRockAndSkill()
        {
            CancelInvoke(nameof(SpawnRock));
            CancelInvoke(nameof(SpawnInvincibleSkill));
            CancelInvoke(nameof(SpawnInvertControl));

            Helper.MoveChildren(rockPoolObject.GetChild(0), rockPoolObject.GetChild(1), true);

            foreach (Transform skill in skillPoolObject)
            {
                Helper.MoveChildren(skill.GetChild(0), skill.GetChild(1), true);
            }
        }

        private void ResetBackGround()
        {
            // Chọn ngẫu nhiên hình nền từ mảng backGroundImage và thiết lập cho tất cả các background
            Sprite selectedBG = backGroundImage[UnityEngine.Random.Range(0, backGroundImage.Length)];
            foreach (Transform bg in backGround)
            {
                bg.GetComponent<SpriteRenderer>().sprite = selectedBG;
            }

            // Lấy kích thước chiều rộng của background trong scene để đặt vị trí cho các background
            bgWidth = backGround[0].GetComponent<SpriteRenderer>().bounds.size.x;
            for (int i = 0; i < backGround.Length; i++)
            {
                backGround[i].position = new Vector3(i * bgWidth, 0, 0);
            }
        }

        private void SetUpAudio(AudioClip audioClip, bool loop)
        {
            audioSource.clip = audioClip;
            audioSource.loop = loop;
        }

        private void SpawnRock()
        {
            CancelInvoke(nameof(SpawnRock));
            Invoke(nameof(SpawnRock), spawnRockTime);

            // Random numberOfRocks với tỷ lệ cho trước
            int numberOfRocks = Helper.WeightedRandom(rockPerSpawn, weights);
            if (numberOfRocks == 0) return;

            List<Vector3> spawnPositions = Helper.PickRandom(rockSpawnPositions, numberOfRocks);
            GameObject rock;
            Transform inactivePool = rockPoolObject.GetChild(1);

            // Khởi tạo các viên đá tại các vị trí đã chọn
            foreach (Vector3 position in spawnPositions)
            {
                if (inactivePool.childCount == 0)
                {
                    // Tạo mới rock từ prefab nếu không có rock nào trong pool
                    rock = Instantiate(rockPrefabs[UnityEngine.Random.Range(0, rockPrefabs.Count)]);
                }
                else
                {
                    rock = inactivePool.GetChild(UnityEngine.Random.Range(0, inactivePool.childCount)).gameObject;
                }
                rock.GetComponent<Rock>().Init(position, currentLevel, verticalMoveRock, multipleSpeed[(int)currentLevel]);
                rock.SetActive(true);
            }
        }

        private void SpawnInvincibleSkill()
        {
            CancelInvoke(nameof(SpawnInvincibleSkill));
            Invoke(nameof(SpawnInvincibleSkill), UnityEngine.Random.Range(10f, 20f));

            // Chỉ xuất hiện skill từ level Hard trở lên
            if (currentLevel < Level.Hard) return;

            // Tỉ lệ xuất hiện skill 30%
            //if (UnityEngine.Random.Range(0, 10) > 2) return;

            Transform inactivePool = skillPoolObject.GetChild(0).GetChild(1);
            GameObject skill;

            if (inactivePool.childCount == 0)
            {
                // Tạo mới skill từ prefab nếu không có skill nào trong pool
                skill = Instantiate(skillPrefabs[0]);
            }
            else
            {
                skill = inactivePool.GetChild(0).gameObject;
            }
            // Chọn vị trí ngẫu nhiên để spawn skill
            skill.transform.position = new Vector3(15f, UnityEngine.Random.Range(-4.5f, 4.5f), 0);
            skill.SetActive(true);
        }

        private void SpawnInvertControl()
        {
            CancelInvoke(nameof(SpawnInvertControl));
            Invoke(nameof(SpawnInvertControl), UnityEngine.Random.Range(15f, 25f));

            // Chỉ xuất hiện invert từ level Extreme
            if (currentLevel != Level.Extreme) return;

            // Tỉ lệ xuất hiện invert 30%
            //if (UnityEngine.Random.Range(0, 10) > 2) return;

            Transform inactivePool = skillPoolObject.GetChild(1).GetChild(1);
            GameObject invert;

            if (inactivePool.childCount == 0)
            {
                // Tạo mới invert từ prefab nếu không có invert nào trong pool
                invert = Instantiate(skillPrefabs[1]);
            }
            else
            {
                invert = inactivePool.GetChild(0).gameObject;
            }
            // Chọn vị trí ngẫu nhiên để spawn skill
            invert.transform.position = new Vector3(15f, UnityEngine.Random.Range(-4.5f, 4.5f), 0);
            invert.SetActive(true);
        }

        private void LevelUp()
        {
            // Tăng độ khó của trò chơi
            // Nếu đã đạt đến mức độ khó cao nhất thì không tăng nữa
            if (currentLevel == Level.Extreme) return;

            // Tạm thời kéo dài thời gian spawn rock để tạo khoảng nghỉ và gọi lại hàm LevelUp sau một khoảng thời gian
            CancelInvoke(nameof(SpawnRock));
            Invoke(nameof(SpawnRock), spawnRockTime * 2.5f);
            CancelInvoke(nameof(LevelUp));
            Invoke(nameof(LevelUp), levelUpTime);

            // Hiệu ứng Level Up nhấp nháy
            Sequence seq = DOTween.Sequence();
            seq.Append(DOVirtual.DelayedCall(spawnRockTime * 2, () =>
            {
                currentLevel++;
                spawnRockTime -= offsetSpawnRockTime;
                Debug.Log("Level Up to " + currentLevel.ToString());
                lvlUpImage.gameObject.SetActive(true);

                if (currentLevel == Level.Hard)
                {
                    // Bắt đầu spawn skill bất tử từ level Hard
                    Debug.Log("Start spawning Invincible Skill");
                    Invoke(nameof(SpawnInvincibleSkill), UnityEngine.Random.Range(0f, 10f));
                }

                if (currentLevel == Level.Extreme)
                {
                    // Bắt đầu spawn skill đảo ngược điều khiển từ level Extreme
                    Debug.Log("Start spawning Invert Control Skill");
                    Invoke(nameof(SpawnInvertControl), UnityEngine.Random.Range(5f, 15f));
                }
            }, false));
            seq.Append(lvlUpImage.DOFade(0.5f, 0.35f).SetLoops(6, LoopType.Yoyo));
            seq.OnComplete(() =>
            {
                lvlUpImage.gameObject.SetActive(false);
                lvlUpImage.color = Color.white;
            });
        }
    }
}