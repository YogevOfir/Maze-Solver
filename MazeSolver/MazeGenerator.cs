using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeSolver
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
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    maze[x, y] = '#';

            // Start point
            maze[1, 1] = 'S';

            CarvePath(1, 1);

            // End point
            maze[width - 2, height - 2] = 'E';

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
                    maze[newX, newY] = '.';
                    maze[x + direction[0] / 2, y + direction[1] / 2] = '.'; // Carve intermediate space
                    CarvePath(newX, newY);
                }
            }
        }

        private bool IsValidMove(int x, int y)
        {
            if (maze == null) return false;
            return x > 0 && x < width - 1 && y > 0 && y < height - 1 && maze[x, y] == '#';
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
