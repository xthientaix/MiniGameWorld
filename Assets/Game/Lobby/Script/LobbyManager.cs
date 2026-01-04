using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Lobby
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] Image blackScreen;
        [SerializeField] float fadeDuration;

        private void Start()
        {
            Time.timeScale = 1f;
            FadeOut();
        }

        public void LoadScene(int sceneIndex)
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.DOFade(1f, fadeDuration).OnComplete(() =>
            {
                SceneManager.LoadScene(sceneIndex);
            });
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void FadeOut()
        {
            blackScreen.gameObject.SetActive(true);
            blackScreen.color = new Color(0, 0, 0, 1);
            blackScreen.DOFade(0f, fadeDuration).OnComplete(() =>
            {
                blackScreen.gameObject.SetActive(false);
            });
        }
    }
}
