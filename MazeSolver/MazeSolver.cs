using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MazeSolverNew
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
            var debugLog = new StringBuilder();
            debugLog.AppendLine("Starting maze solve...");
            debugLog.AppendLine($"Start point: ({start.X}, {start.Y})");
            debugLog.AppendLine($"End point: ({end.X}, {end.Y})");
            debugLog.AppendLine("\nMaze validation:");
            
            // Create a copy of the maze to ensure it doesn't change
            char[,] mazeCopy = (char[,])maze.Clone();
            
            // Validate the maze
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    debugLog.Append($"{maze[x, y]} ");
                }
                debugLog.AppendLine();
            }
            
            // Write the maze validation immediately
            File.WriteAllText("solver_debug.txt", debugLog.ToString());
            
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
                debugLog.AppendLine($"\nCurrent position: ({current.X}, {current.Y}) = {maze[current.X, current.Y]}");

                // Verify maze hasn't changed
                if (maze[current.X, current.Y] != mazeCopy[current.X, current.Y])
                {
                    debugLog.AppendLine($"WARNING: Maze changed at ({current.X}, {current.Y})!");
                    debugLog.AppendLine($"Original: {mazeCopy[current.X, current.Y]}, Current: {maze[current.X, current.Y]}");
                    File.WriteAllText("solver_debug.txt", debugLog.ToString());
                    throw new InvalidOperationException("Maze was modified during solving!");
                }

                if (current == end)
                {
                    debugLog.AppendLine("Reached end point!");
                    File.WriteAllText("solver_debug.txt", debugLog.ToString());
                    return ReconstructPath(cameFrom, current);
                }

                closedSet.Add(current);

                foreach (var neighbor in GetNeighbors(current))
                {
                    debugLog.AppendLine($"  Checking neighbor: ({neighbor.X}, {neighbor.Y}) = {maze[neighbor.X, neighbor.Y]}");
                    
                    // Verify maze hasn't changed
                    if (maze[neighbor.X, neighbor.Y] != mazeCopy[neighbor.X, neighbor.Y])
                    {
                        debugLog.AppendLine($"WARNING: Maze changed at ({neighbor.X}, {neighbor.Y})!");
                        debugLog.AppendLine($"Original: {mazeCopy[neighbor.X, neighbor.Y]}, Current: {maze[neighbor.X, neighbor.Y]}");
                        File.WriteAllText("solver_debug.txt", debugLog.ToString());
                        throw new InvalidOperationException("Maze was modified during solving!");
                    }
                    
                    if (closedSet.Contains(neighbor))
                    {
                        debugLog.AppendLine("    Already visited, skipping");
                        continue;
                    }

                    var tentativeGScore = gScore[current] + 1;
                    debugLog.AppendLine($"    Tentative G score: {tentativeGScore}");

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, end);
                        debugLog.AppendLine($"    Updated scores - G: {gScore[neighbor]}, F: {fScore[neighbor]}");

                        if (!openSet.UnorderedItems.Any(x => x.Element.Position == neighbor))
                        {
                            openSet.Enqueue(new Node(neighbor, fScore[neighbor]), fScore[neighbor]);
                            debugLog.AppendLine("    Added to open set");
                        }
                    }
                    else
                    {
                        debugLog.AppendLine("    Better path exists, skipping");
                    }
                }
            }

            debugLog.AppendLine("\nNo path found!");
            File.WriteAllText("solver_debug.txt", debugLog.ToString());
            return new List<Point>();
        }

        private List<Point> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)
        {
            var path = new List<Point> { current };
            var debugLog = new StringBuilder();
            debugLog.AppendLine("\nReconstructing path:");
            debugLog.AppendLine($"End: ({current.X}, {current.Y})");

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
                debugLog.AppendLine($"From: ({current.X}, {current.Y})");
            }

            path.Reverse();
            debugLog.AppendLine("\nFinal path:");
            foreach (var point in path)
            {
                debugLog.AppendLine($"({point.X}, {point.Y}) = {maze[point.X, point.Y]}");
            }

            File.AppendAllText("solver_debug.txt", debugLog.ToString());
            return path;
        }

        public IEnumerable<Point> GetNeighbors(Point point)
        {
            var directions = new[] { (0, 1), (1, 0), (0, -1), (-1, 0) };
            foreach (var (dx, dy) in directions)
            {
                var newX = point.X + dx;
                var newY = point.Y + dy;

                // Check if the new position is within bounds
                if (newX < 0 || newX >= width || newY < 0 || newY >= height)
                    continue;

                // Check if the new position is a wall
                if (maze[newX, newY] == '#')
                    continue;

                // Only allow movement to paths (.), start (S), or end (E)
                if (maze[newX, newY] == '.' || maze[newX, newY] == 'S' || maze[newX, newY] == 'E')
                {
                    yield return new Point(newX, newY);
                }
            }
        }

        private int Heuristic(Point a, Point b)
        {
            // Check if the target point is a wall
            if (maze[b.X, b.Y] == '#')
                return int.MaxValue;
                
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }

    public record Point(int X, int Y);
    public record Node(Point Position, int FScore);
} 