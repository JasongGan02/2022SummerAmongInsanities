using System.Collections.Generic;
using UnityEngine;

public class Pathfinding
{
    public class Node
    {
        public Vector2Int Position;
        public Node Parent;
        public int G; // Cost from start to the current node
        public int H; // Estimated cost from the current node to the end
        public int F; // Total cost (G + H)

        public Node(Vector2Int pos, Node parent = null)
        {
            Position = pos;
            Parent = parent;
            G = H = F = 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is Node node)
                return Position.Equals(node.Position);
            return false;
        }

        public override int GetHashCode()
        {
            return Position.GetHashCode();
        }
    }

    public static List<Vector2Int> AstarPath(Vector2Int start, Vector2Int goal, int[,] costGrid, int width, int height, int minX, int minY)
    {
        Node startNode = new Node(start);
        Node endNode = new Node(goal);
        List<Node> openList = new List<Node>();
        HashSet<Node> closedList = new HashSet<Node>();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList[0];
            int currentIndex = 0;

            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].F < currentNode.F)
                {
                    currentNode = openList[i];
                    currentIndex = i;
                }
            }

            openList.RemoveAt(currentIndex);
            closedList.Add(currentNode);

            if (currentNode.Equals(endNode))
            {
                List<Vector2Int> path = new List<Vector2Int>();
                Node current = currentNode;
                while (current != null)
                {
                    path.Add(new Vector2Int(current.Position.x + minX, current.Position.y + minY));
                    current = current.Parent;
                }
                path.Reverse();
                return path;
            }

            List<Node> children = new List<Node>();
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var direction in directions)
            {
                Vector2Int nodePosition = currentNode.Position + direction;
                if (nodePosition.x >= 0 && nodePosition.x < width && nodePosition.y >= 0 && nodePosition.y < height)
                {
                    Node newNode = new Node(nodePosition, currentNode);
                    if (!closedList.Contains(newNode))
                    {
                        children.Add(newNode);
                    }
                }
            }

            foreach (var child in children)
            {
                if (!openList.Contains(child))
                {
                    child.G = currentNode.G + costGrid[child.Position.x, child.Position.y];
                    child.H = (child.Position.x - endNode.Position.x) * (child.Position.x - endNode.Position.x) + (child.Position.y - endNode.Position.y) * (child.Position.y - endNode.Position.y);
                    child.F = child.G + child.H;

                    openList.Add(child);
                }
                else
                {
                    int newG = currentNode.G + costGrid[child.Position.x, child.Position.y];
                    if (newG < child.G) // Check if new path to child is better
                    {
                        child.G = newG;
                        child.F = child.G + child.H;
                    }
                }
            }
        }

        return new List<Vector2Int>(); // Return empty if no path is found
    }

    public static int CalculatePathCost(List<Vector2Int> path, int[,] costGrid, int minX, int minY)
    {
        int totalCost = 0;

        foreach (var point in path)
        {
            int gridX = point.x - minX;
            int gridY = point.y - minY;

            totalCost += costGrid[gridX, gridY];
        }

        return totalCost;
    }

    public static List<Vector2> PathPointToCenter(List<Vector2Int> path)
    {
        List<Vector2> convertedPath = new List<Vector2>();

        foreach (Vector2Int point in path)
        {
            Vector2 newPoint = new Vector2(point.x + 0.5f, point.y + 0.5f);
            convertedPath.Add(newPoint);
        }

        return convertedPath;
    }

    public static int DistanceToGround(int x, int y)
    {
        Vector2Int position = new Vector2Int(x, y);
        if (y == 0 || WorldGenerator.GetDataFromWorldPos(position) != null)
        {
            return 0;
        }
        for (int i = y - 1; i >= 0; i--)
        {
            Vector2Int position1 = new Vector2Int(x, i);
            if (WorldGenerator.GetDataFromWorldPos(position1) != null)
            {
                return (y - i);
            }
        }
        return -1;  // false value: input y is invalid
    }

    public static bool IsNeighborOfTile(int x, int y)
    {
        Vector2Int position = new Vector2Int(x + 1, y);
        TileObject tile = WorldGenerator.GetDataFromWorldPos(position);
        if (tile != null) { return true; }
        position = new Vector2Int(x - 1, y);
        tile = WorldGenerator.GetDataFromWorldPos(position);
        if (tile != null) { return true; }
        return false;
    }

    public static bool IsLadder(int x, int y)
    {
        return false;
    }

    public static bool IsNeighborTileReachable(int x, int y)
    {
        Vector2Int position = new Vector2Int(x + 1, y - 1);
        TileObject tile = WorldGenerator.GetDataFromWorldPos(position);
        if (tile != null) { return true; }
        position = new Vector2Int(x - 1, y - 1);
        tile = WorldGenerator.GetDataFromWorldPos(position);
        if (tile != null) { return true; }
        return false;
    }

    public static void LogHealthGrid(int[,] healthGrid)
    {
        int width = healthGrid.GetLength(0);
        int height = healthGrid.GetLength(1);
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                sb.AppendFormat("{0,-2}  ", healthGrid[x, y]);
            }
            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }
}
