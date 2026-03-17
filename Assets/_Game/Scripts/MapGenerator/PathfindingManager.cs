using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour
{
    public static PathfindingManager Instance { get; private set; }

    private PerlinMapGenerator mapGen;

    void Awake()
    {
        Instance = this;
        mapGen = FindObjectOfType<PerlinMapGenerator>();
    }

    public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
    {
        var openSet = new PriorityQueue<Vector2Int>();
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, float>();

        gScore[start] = 0;
        openSet.Enqueue(start, Heuristic(start, end));

        Vector2Int[] dirs = {
        Vector2Int.up, Vector2Int.down,
        Vector2Int.left, Vector2Int.right
    };

        while (openSet.Count > 0)
        {
            var current = openSet.Dequeue();

            if (current == end)
                return ReconstructPath(cameFrom, current);

            foreach (var d in dirs)
            {
                var neighbor = current + d;

                if (!mapGen.InBounds(neighbor.x, neighbor.y)) continue;
                if (mapGen.grid[neighbor.x, neighbor.y] == PerlinMapGenerator.Grid.WALL) continue;
                if (mapGen.grid[neighbor.x, neighbor.y] == PerlinMapGenerator.Grid.EMPTY) continue;

                float tentativeG = gScore[current] + 1f;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    openSet.Enqueue(neighbor, tentativeG + Heuristic(neighbor, end));
                }
            }
        }

        return null; // tidak ada path
    }

    float Heuristic(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y); // Manhattan distance

    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
    {
        var path = new List<Vector2Int>();
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }

    public class PriorityQueue<T>
    {
        private List<(T item, float priority)> elements = new();
        public int Count => elements.Count;

        public void Enqueue(T item, float priority) =>
            elements.Add((item, priority));

        public T Dequeue()
        {
            int best = 0;
            for (int i = 1; i < elements.Count; i++)
                if (elements[i].priority < elements[best].priority) best = i;
            var item = elements[best].item;
            elements.RemoveAt(best);
            return item;
        }
    }
}