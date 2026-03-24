using UnityEngine;

public class PathNode {
    public Vector2Int gridPosition;
    public bool isWalkable;

    public int gCost;
    public int hCost;
    public int fCost => gCost + hCost;

    public PathNode parent;

    public PathNode(Vector2Int gridPosition, bool isWalkable) {
        this.gridPosition = gridPosition;
        this.isWalkable = isWalkable;
    }
}