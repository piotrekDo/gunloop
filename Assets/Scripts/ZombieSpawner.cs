using UnityEngine;

public class ZombieSpawner : MonoBehaviour {

    [SerializeField] GameObject m_zombiePrefarb;
    [SerializeField] private LayerMask m_wallLayer;

    public bool TryToSpawnZombie(BoxCollider2D zone) {
        Vector2 point = GetRandomPoint(zone);
        if (Physics2D.OverlapCircle(point, 0.3f, m_wallLayer) != null)
            return false; // punkt w œcianie, pomiñ
        SpawnZombie(point);
        return true;
    }

    private Vector2 GetRandomPoint(BoxCollider2D area) {
        Bounds bounds = area.bounds;
        float x = Random.Range(bounds.min.x, bounds.max.x);
        float y = Random.Range(bounds.min.y, bounds.max.y);
        return new Vector2(x, y);
    }

    private void SpawnZombie(Vector2 spawnPoint) {
        GameObject zombie = Instantiate(m_zombiePrefarb);
        zombie.transform.position = spawnPoint;
    }
}
