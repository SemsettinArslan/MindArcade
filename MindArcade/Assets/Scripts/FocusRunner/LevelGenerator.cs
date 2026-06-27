using UnityEngine;
using System.Collections.Generic;

namespace FocusRunner
{
    public class LevelGenerator : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private StroopObject _stroopPrefab;
        [SerializeField] private GameObject _obstaclePrefab;

        [Header("Spawn Settings")]
        [SerializeField] private float _spawnDistance = 50f;
        [SerializeField] private float _spawnInterval = 2f;
        [SerializeField] private float _speed = 10f;

        [Header("Player Y Hizalama Ayarları")]
        [Tooltip("Oyuncu nesnesi referansı. Boş bırakılırsa sahnede otomatik aranır.")]
        [SerializeField] private Transform playerTransform;

        private float _timer;
        private List<StroopObject> _activeObjects = new List<StroopObject>();

        private void Start()
        {
            if (playerTransform == null)
            {
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }
        }

        private void Update()
        {
            if (!FocusRunnerManager.Instance.IsGameRunning) return;

            MoveObjects();
            
            _timer += Time.deltaTime;
            if (_timer >= _spawnInterval)
            {
                SpawnWave();
                _timer = 0;
            }
        }

        private void MoveObjects()
        {
            for (int i = _activeObjects.Count - 1; i >= 0; i--)
            {
                StroopObject obj = _activeObjects[i];
                obj.transform.Translate(Vector3.back * _speed * Time.deltaTime);

                if (obj.transform.position.z < -5f)
                {
                    _activeObjects.RemoveAt(i);
                    Destroy(obj.gameObject); // Simple cleanup, could be pooling
                }
            }
        }

        private void SpawnWave()
        {
            int lane = Random.Range(0, FocusRunnerConstants.TotalLanes);
            bool isObstacle = Random.value < DifficultyManager.Instance.ObstacleFrequency;

            // Eğer engel ise ve _obstaclePrefab atanmışsa onu spawn et, aksi halde normal prefab'i kullan
            GameObject prefabToSpawn = (isObstacle && _obstaclePrefab != null) ? _obstaclePrefab : _stroopPrefab.gameObject;
            GameObject spawnedObj = Instantiate(prefabToSpawn, transform);

            float spawnY = playerTransform != null ? playerTransform.position.y : 0.5f;
            spawnedObj.transform.position = new Vector3(FocusRunnerConstants.GetLaneX(lane), spawnY, _spawnDistance);

            StroopObject newObj = spawnedObj.GetComponent<StroopObject>();
            if (newObj == null)
            {
                // Eğer engel prefab'inde StroopObject yoksa otomatik ekle (hareket ve çarpışma mantığı için)
                newObj = spawnedObj.AddComponent<StroopObject>();
            }

            if (isObstacle)
            {
                newObj.Initialize(new StroopData { colorName = "DUR", colorValue = Color.gray }, Color.gray, true);
            }
            else
            {
                // Logic based on current rule to guarantee at least one "correct" item sometimes
                GenerateStroopData(out StroopData data, out Color ink);
                newObj.Initialize(data, ink);
            }

            _activeObjects.Add(newObj);
        }

        private void GenerateStroopData(out StroopData data, out Color ink)
        {
            // Pick a random semantic color
            data = FocusRunnerConstants.GetRandomColor();
            // Pick a random visual color (ink)
            ink = FocusRunnerConstants.GetRandomColor().colorValue;

            // Distractor logic: 20% chance to force a conflict (Stroop Effect)
            if (Random.value < 0.2f)
            {
                // Ensure ink color is NOT the same as data.colorValue to create conflict
                StroopData fake = FocusRunnerConstants.GetRandomColor();
                while (fake.colorValue == data.colorValue) fake = FocusRunnerConstants.GetRandomColor();
                ink = fake.colorValue;
            }
        }

        public void SetSpeed(float newSpeed) => _speed = newSpeed;
        public void SetSpawnInterval(float newInterval) => _spawnInterval = newInterval;
    }
}
