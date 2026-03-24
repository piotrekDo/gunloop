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

        List<PathNode> openList = new List<PathNode>();
        HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();
        Dictionary<Vector2Int, PathNode> allNodes = new Dictionary<Vector2Int, PathNode>();

        PathNode startNode = new PathNode(start, true);
        startNode.gCost = 0;
        startNode.hCost = GetHeuristic(start, target);
        openList.Add(startNode);
        allNodes[start] = startNode;

        while (openList.Count > 0) {
            PathNode current = GetLowestFCost(openList);

            if (current.gridPosition == target)
                return RetracePath(current);

            openList.Remove(current);
            closedSet.Add(current.gridPosition);

            foreach (Vector2Int neighbourPos in GetNeighbours(current.gridPosition)) {
                if (closedSet.Contains(neighbourPos))
                    continue;
                if (!IsWalkable(neighbourPos))
                    continue;

                int newGCost = current.gCost + 1;

                if (!allNodes.TryGetValue(neighbourPos, out PathNode neighbour)) {
                    neighbour = new PathNode(neighbourPos, true);
                    allNodes[neighbourPos] = neighbour;
                }

                if (newGCost < neighbour.gCost || !openList.Contains(neighbour)) {
                    neighbour.gCost = newGCost;
                    neighbour.hCost = GetHeuristic(neighbourPos, target);
                    neighbour.parent = current;

                    if (!openList.Contains(neighbour))
                        openList.Add(neighbour);
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

    private PathNode GetLowestFCost(List<PathNode> list) {
        PathNode lowest = list[0];
        foreach (PathNode node in list)
            if (node.fCost < lowest.fCost || (node.fCost == lowest.fCost && node.hCost < lowest.hCost))
                lowest = node;
        return lowest;
    }

    private List<Vector2Int> GetNeighbours(Vector2Int pos) {
        return new List<Vector2Int>
        {
            pos + Vector2Int.up,
            pos + Vector2Int.down,
            pos + Vector2Int.left,
            pos + Vector2Int.right
        };
    }

    private bool IsWalkable(Vector2Int gridPos) {
        Vector2 worldPos = GridToWorld(gridPos);
        return Physics2D.OverlapCircle(worldPos, 0.1f, m_wallLayer) == null;
    }

    private int GetHeuristic(Vector2Int a, Vector2Int b) {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    private Vector2Int WorldToGrid(Vector2 worldPos) {
        return new Vector2Int(
            Mathf.RoundToInt(worldPos.x / k_cellSize),
            Mathf.RoundToInt(worldPos.y / k_cellSize));
    }

    private Vector2 GridToWorld(Vector2Int gridPos) {
        return new Vector2(gridPos.x * k_cellSize, gridPos.y * k_cellSize);
    }
}