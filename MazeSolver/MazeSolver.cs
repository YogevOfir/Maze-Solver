using System;
using System.Collections.Generic;
using System.Linq;

namespace MazeSolver
{
    public class MazeSolver
    {
        private char[,] maze;
        private int width;
        private int height;
        private Point start;
        private Point end;

        public MazeSolver(char[,] maze)
        {
            this.maze = maze;
            width = maze.GetLength(0);
            height = maze.GetLength(1);
            start = new Point(0, 0); // Default values
            end = new Point(0, 0);
            FindStartAndEnd();
        }

        private void FindStartAndEnd()
        {
            bool foundStart = false;
            bool foundEnd = false;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (maze[x, y] == 'S')
                    {
                        start = new Point(x, y);
                        foundStart = true;
                    }
                    else if (maze[x, y] == 'E')
                    {
                        end = new Point(x, y);
                        foundEnd = true;
                    }

                    if (foundStart && foundEnd) return;
                }
            }

            if (!foundStart || !foundEnd)
                throw new InvalidOperationException("Maze must contain both start (S) and end (E) points");
        }

        public List<Point> Solve()
        {
            var openSet = new PriorityQueue<Node, int>();
            var closedSet = new HashSet<Point>();
            var cameFrom = new Dictionary<Point, Point>();
            var gScore = new Dictionary<Point, int>();
            var fScore = new Dictionary<Point, int>();

            gScore[start] = 0;
            fScore[start] = Heuristic(start, end);
            openSet.Enqueue(new Node(start, fScore[start]), fScore[start]);

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue().Position;

                if (current == end)
                    return ReconstructPath(cameFrom, current);

                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    var tentativeGScore = gScore[current] + 1;

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, end);

                        if (!openSet.UnorderedItems.Any(x => x.Element.Position == neighbor))
                            openSet.Enqueue(new Node(neighbor, fScore[neighbor]), fScore[neighbor]);
                    }
                }
            }

            return new List<Point>(); // No path found
        }

        private List<Point> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)
        {
            var path = new List<Point> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        private IEnumerable<Point> GetNeighbors(Point point)
        {
            var directions = new[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
            foreach (var (dx, dy) in directions)
            {
                var newX = point.X + dx;
                var newY = point.Y + dy;

                if (newX >= 0 && newX < width && newY >= 0 && newY < height &&
                    (maze[newX, newY] == '.' || maze[newX, newY] == 'E'))
                {
                    yield return new Point(newX, newY);
                }
            }
        }

        private int Heuristic(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }

    public record Point(int X, int Y);
    public record Node(Point Position, int FScore);
} 