using UnityEngine;

namespace Tank
{
    public class SpawnArea : MonoBehaviour
    {
        private GameController gameController;

        public int areaID;
        private int tankInArea;

        private void Awake()
        {
            gameController = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        }

        private void Start()
        {
            tankInArea = 0;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            gameController.SetSpawnAreaAvailability(areaID, false);
            tankInArea++;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            tankInArea--;
            if (tankInArea <= 0)
            {
                gameController.SetSpawnAreaAvailability(areaID, true);
                tankInArea = 0;
            }
        }
    }
}