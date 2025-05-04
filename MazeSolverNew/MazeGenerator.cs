using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeSolverNew
{
    public class MazeGenerator
    {
        private int width;
        private int height;
        private char[,]? maze;
        private Random random = new Random();

        private int[][] directionOffsets = new int[][] 
        { 
            new int[] {0, 2}, 
            new int[] {2, 0}, 
            new int[] {0, -2}, 
            new int[] {-2, 0} 
        };

        public MazeGenerator(int width, int height)
        {
            this.width = Math.Max(5, width);
            this.height = Math.Max(5, height);
        }

        public char[,] GenerateMaze()
        {
            maze = new char[width, height];
            // Initialize all cells as walls
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    maze[x, y] = '#';

            // Start point
            maze[1, 1] = 'S';

            // Generate paths
            CarvePath(1, 1);

            // End point
            maze[width - 2, height - 2] = 'E';

            // Ensure all paths are properly connected
            ValidatePaths();

            return maze;
        }

        private void CarvePath(int x, int y)
        {
            if (maze == null) return;

            // Shuffle the directions
            var directions = directionOffsets.OrderBy(d => random.Next()).ToArray();

            foreach (var direction in directions)
            {
                int newX = x + direction[0];
                int newY = y + direction[1];

                if (IsValidMove(newX, newY))
                {
                    // Carve the path
                    maze[newX, newY] = '.';
                    // Carve the intermediate cell
                    maze[x + direction[0] / 2, y + direction[1] / 2] = '.';
                    CarvePath(newX, newY);
                }
            }
        }

        private bool IsValidMove(int x, int y)
        {
            if (maze == null) return false;
            // Check if the position is within bounds and is a wall
            return x > 0 && x < width - 1 && y > 0 && y < height - 1 && maze[x, y] == '#';
        }

        private void ValidatePaths()
        {
            if (maze == null) return;

            // Ensure all paths are properly connected
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (maze[x, y] == '.')
                    {
                        // Count adjacent paths
                        int adjacentPaths = 0;
                        if (maze[x - 1, y] != '#') adjacentPaths++;
                        if (maze[x + 1, y] != '#') adjacentPaths++;
                        if (maze[x, y - 1] != '#') adjacentPaths++;
                        if (maze[x, y + 1] != '#') adjacentPaths++;

                        // If a path cell has only one connection, it's a dead end
                        if (adjacentPaths == 1)
                        {
                            // Make it a wall to prevent invalid paths
                            maze[x, y] = '#';
                        }
                    }
                }
            }
        }

        public string GetMazeAsString()
        {
            if (maze == null) return string.Empty;
            
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    sb.Append(maze[x, y]);
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
}
