using System.Collections.Generic;
using UnityEngine;

public class Pathfinder {
    private LayerMask m_wallLayer;
    private const float k_cellSize = 0.5f;

    public Pathfinder(LayerMask wallLayer) {
        m_wallLayer = wallLayer;
    }

    public List<Vector2> FindPath(Vector2 startWorld, Vector2 targetWorld) {
        Vector2Int start = WorldToGrid(startWorld);
        Vector2Int target = WorldToGrid(targetWorld);

        SimplePriorityQueue<PathNode> openList = new SimplePriorityQueue<PathNode>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, PathNode> allNodes = new Dictionary<Vector2Int, PathNode>();

        PathNode startNode = new PathNode(start, true) {
            gCost = 0,
            hCost = GetHeuristic(start, target)
        };
        openList.Enqueue(startNode);
        allNodes[start] = startNode;

        while (openList.Count > 0) {
            PathNode current = openList.Dequeue();
            if (current.gridPosition == target)
                return RetracePath(current);

            closedSet.Add(current.gridPosition);

            foreach (Vector2Int neighbourPos in GetNeighbours(current.gridPosition)) {
                if (closedSet.Contains(neighbourPos) || !IsWalkable(neighbourPos))
                    continue;

                bool isDiagonal = neighbourPos.x != current.gridPosition.x && neighbourPos.y != current.gridPosition.y;

                // blokada ruchu po rogach
                if (isDiagonal) {
                    Vector2Int side1 = new Vector2Int(current.gridPosition.x, neighbourPos.y);
                    Vector2Int side2 = new Vector2Int(neighbourPos.x, current.gridPosition.y);
                    if (!IsWalkable(side1) || !IsWalkable(side2))
                        continue;
                }

                int moveCost = isDiagonal ? 14 : 10;
                int newGCost = current.gCost + moveCost;

                if (!allNodes.TryGetValue(neighbourPos, out PathNode neighbour)) {
                    neighbour = new PathNode(neighbourPos, true);
                    allNodes[neighbourPos] = neighbour;
                }

                if (newGCost < neighbour.gCost || !neighbour.opened) {
                    neighbour.gCost = newGCost;
                    neighbour.hCost = GetHeuristic(neighbourPos, target);
                    neighbour.parent = current;

                    if (!neighbour.opened) {
                        openList.Enqueue(neighbour);
                        neighbour.opened = true;
                    }
                }
            }
        }

        return null;
    }

    private List<Vector2> RetracePath(PathNode endNode) {
        List<Vector2> path = new List<Vector2>();
        PathNode current = endNode;

        while (current != null) {
            path.Add(GridToWorld(current.gridPosition));
            current = current.parent;
        }

        path.Reverse();
        return path;
    }

    private List<Vector2Int> GetNeighbours(Vector2Int pos) {
        return new List<Vector2Int>
        {
            pos + Vector2Int.up,
            pos + Vector2Int.down,
            pos + Vector2Int.left,
            pos + Vector2Int.right,
            pos + Vector2Int.up + Vector2Int.left,
            pos + Vector2Int.up + Vector2Int.right,
            pos + Vector2Int.down + Vector2Int.left,
            pos + Vector2Int.down + Vector2Int.right
        };
    }

    private bool IsWalkable(Vector2Int gridPos) {
        Vector2 worldPos = GridToWorld(gridPos);
        return Physics2D.OverlapCircle(worldPos, 0.1f, m_wallLayer) == null;
    }

    private int GetHeuristic(Vector2Int a, Vector2Int b) {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return 10 * (dx + dy) - 6 * Mathf.Min(dx, dy); // diagonal distance
    }

    private Vector2Int WorldToGrid(Vector2 worldPos) {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / k_cellSize),
            Mathf.RoundToInt(worldPos.y / k_cellSize));
    }

    private Vector2 GridToWorld(Vector2Int gridPos) {
        return new Vector2(gridPos.x * k_cellSize, gridPos.y * k_cellSize);
    }

    // ---- PATH NODE ----
    private class PathNode {
        public Vector2Int gridPosition;
        public int gCost = int.MaxValue;
        public int hCost;
        public PathNode parent;
        public bool walkable;
        public bool opened;

        public int fCost => gCost + hCost;

        public PathNode(Vector2Int pos, bool walkable) {
            this.gridPosition = pos;
            this.walkable = walkable;
        }
    }

    // ---- PROSTA KOLEJKA PRIORYTETOWA ----
    private class SimplePriorityQueue<T> where T : PathNode {
        private List<T> nodes = new List<T>();

        public int Count => nodes.Count;

        public void Enqueue(T node) {
            nodes.Add(node);
            nodes.Sort((a, b) => a.fCost.CompareTo(b.fCost)); // prosty sort po fCost
        }

        public T Dequeue() {
            if (nodes.Count == 0)
                return null;
            T node = nodes[0];
            nodes.RemoveAt(0);
            return node;
        }
    }
}