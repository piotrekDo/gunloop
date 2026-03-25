using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class GameManagerController : MonoBehaviour {

    [Header("Dependencies")]
    [SerializeField] ZombieSpawner m_zombieSpawner;
    [SerializeField] List<BoxCollider2D> m_streetSpawnAreas;

    [Header("Game Settings")]
    [SerializeField] private float m_roundLengthInSeconds = 10f;
    [SerializeField] private int m_startingZombiePopulation = 2;
    [SerializeField] private int m_zombiePopulationGrowthPerRound = 2;
    [SerializeField] private float m_baseSpawnChance = .5f;
    [SerializeField] private float m_spawnChanceGrowth = .05f;
    [SerializeField] private int m_maxZombies;


    [SerializeField] private int m_currentZombies;
    [SerializeField] private int m_currentRound;
    [SerializeField] private int m_currentPopulation;
    [SerializeField] private float m_currentSpawnChance;



    private void OnEnable() {
        GameEvents.Instance.onZombieDies += OnZombieDies;
    }

    private void OnDisable() {
        GameEvents.Instance.onZombieDies -= OnZombieDies;
    }

    private void Awake() {
        m_currentRound = 1;
        m_currentPopulation = m_startingZombiePopulation;
        m_currentSpawnChance = m_baseSpawnChance;
    }

    private void Start() {
        StartCoroutine(ZombieSpawnerRoutine());
    }

    private IEnumerator ZombieSpawnerRoutine() {
        while (true) {
            yield return new WaitForSeconds(m_roundLengthInSeconds);

            foreach (var area in m_streetSpawnAreas) {
                for (int i = 0; i < m_currentPopulation; ++i) {
                    if (m_currentZombies >= m_maxZombies)
                        break; //limit

                    if (Random.Range(0f, 1f) <= m_currentSpawnChance) {
                        if (m_zombieSpawner.TryToSpawnZombie(area))
                            m_currentZombies++;
                    }
                }
            }

            m_currentPopulation += m_zombiePopulationGrowthPerRound;
            m_currentRound++;
            if (m_currentSpawnChance < 1f)
                m_currentSpawnChance += m_spawnChanceGrowth;
        }
    }

    private void OnZombieDies() {
        m_currentZombies--;
    }
}
