using System.Collections.Generic;
using UnityEngine;

public class Pathfinder {
    private LayerMask m_wallLayer;
    private const float k_cellSize = 0.5f;
    private const int k_maxIterations = 1000;

    // cache walkability — odpytujemy fizykê tylko raz per pozycja
    private static Dictionary<Vector2Int, bool> s_walkabilityCache = new Dictionary<Vector2Int, bool>();

    // reu¿ywalne bufory ¿eby nie alokowaæ co wywo³anie
    private readonly Dictionary<Vector2Int, PathNode> m_allNodes = new Dictionary<Vector2Int, PathNode>();
    private readonly HashSet<Vector2Int> m_closedSet = new HashSet<Vector2Int>();
    private readonly SimplePriorityQueue m_openList = new SimplePriorityQueue();
    private readonly List<Vector2> m_resultPath = new List<Vector2>();

    // sta³e s¹siedztwo — bez alokacji listy co wywo³anie
    private static readonly Vector2Int[] s_neighbours = new Vector2Int[]
    {
        new Vector2Int( 0,  1),
        new Vector2Int( 0, -1),
        new Vector2Int(-1,  0),
        new Vector2Int( 1,  0),
        new Vector2Int(-1,  1),
        new Vector2Int( 1,  1),
        new Vector2Int(-1, -1),
        new Vector2Int( 1, -1),
    };

    public Pathfinder(LayerMask wallLayer) {
        m_wallLayer = wallLayer;
    }

    // wyczyœæ cache gdy mapa siê zmienia (np. zniszczono drzwi)
    public static void InvalidateCache() {
        s_walkabilityCache.Clear();
    }

    public List<Vector2> FindPath(Vector2 startWorld, Vector2 targetWorld) {
        Vector2Int start = WorldToGrid(startWorld);
        Vector2Int target = WorldToGrid(targetWorld);

        m_allNodes.Clear();
        m_closedSet.Clear();
        m_openList.Clear();
        m_resultPath.Clear();

        PathNode startNode = GetNode(start);
        startNode.gCost = 0;
        startNode.hCost = GetHeuristic(start, target);
        startNode.opened = true;
        m_openList.Enqueue(startNode);

        int iterations = 0;

        while (m_openList.Count > 0) {
            if (++iterations > k_maxIterations)
                return null;

            PathNode current = m_openList.Dequeue();

            if (current.gridPosition == target)
                return RetracePath(current);

            m_closedSet.Add(current.gridPosition);

            for (int i = 0; i < s_neighbours.Length; i++) {
                Vector2Int neighbourPos = current.gridPosition + s_neighbours[i];

                if (m_closedSet.Contains(neighbourPos) || !IsWalkable(neighbourPos))
                    continue;

                bool isDiagonal = s_neighbours[i].x != 0 && s_neighbours[i].y != 0;

                if (isDiagonal) {
                    Vector2Int side1 = new Vector2Int(current.gridPosition.x, neighbourPos.y);
                    Vector2Int side2 = new Vector2Int(neighbourPos.x, current.gridPosition.y);
                    if (!IsWalkable(side1) || !IsWalkable(side2))
                        continue;
                }

                int moveCost = isDiagonal ? 14 : 10;
                int newGCost = current.gCost + moveCost;

                PathNode neighbour = GetNode(neighbourPos);

                if (newGCost < neighbour.gCost || !neighbour.opened) {
                    neighbour.gCost = newGCost;
                    neighbour.hCost = GetHeuristic(neighbourPos, target);
                    neighbour.parent = current;

                    if (!neighbour.opened) {
                        m_openList.Enqueue(neighbour);
                        neighbour.opened = true;
                    }
                }
            }
        }

        return null; // brak œcie¿ki
    }

    private PathNode GetNode(Vector2Int pos) {
        if (!m_allNodes.TryGetValue(pos, out PathNode node)) {
            node = new PathNode(pos);
            m_allNodes[pos] = node;
        }
        return node;
    }

    private List<Vector2> RetracePath(PathNode endNode) {
        PathNode current = endNode;
        while (current != null) {
            m_resultPath.Add(GridToWorld(current.gridPosition));
            current = current.parent;
        }
        m_resultPath.Reverse();
        return m_resultPath;
    }

    private bool IsWalkable(Vector2Int gridPos) {
        if (s_walkabilityCache.TryGetValue(gridPos, out bool cached))
            return cached;

        Vector2 worldPos = GridToWorld(gridPos);
        bool walkable = Physics2D.OverlapCircle(worldPos, 0.1f, m_wallLayer) == null;
        s_walkabilityCache[gridPos] = walkable;
        return walkable;
    }

    private int GetHeuristic(Vector2Int a, Vector2Int b) {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return 10 * (dx + dy) - 6 * Mathf.Min(dx, dy);
    }

    private Vector2Int WorldToGrid(Vector2 worldPos) {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / k_cellSize),
            Mathf.RoundToInt(worldPos.y / k_cellSize));
    }

    private Vector2 GridToWorld(Vector2Int gridPos) {
        return new Vector2(gridPos.x * k_cellSize, gridPos.y * k_cellSize);
    }

    private class PathNode {
        public Vector2Int gridPosition;
        public int gCost = int.MaxValue;
        public int hCost;
        public PathNode parent;
        public bool opened;
        public int fCost => gCost + hCost;

        public PathNode(Vector2Int pos) {
            gridPosition = pos;
        }
    }

    private class SimplePriorityQueue {
        private List<PathNode> nodes = new List<PathNode>();
        public int Count => nodes.Count;

        public void Enqueue(PathNode node) {
            nodes.Add(node);
            // insertion sort — szybszy ni¿ Sort() dla ma³ych list
            int i = nodes.Count - 1;
            while (i > 0 && nodes[i].fCost < nodes[i - 1].fCost) {
                (nodes[i], nodes[i - 1]) = (nodes[i - 1], nodes[i]);
                i--;
            }
        }

        public PathNode Dequeue() {
            PathNode node = nodes[0];
            nodes.RemoveAt(0);
            return node;
        }

        public void Clear() => nodes.Clear();
    }
}