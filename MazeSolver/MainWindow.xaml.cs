using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace MazeSolver;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MazeGenerator? mazeGenerator;
    private const int CellSize = 30;
    private List<Point>? solutionPath;
    private int currentPathIndex = 0;
    private DispatcherTimer? animationTimer;

    public MainWindow()
    {
        InitializeComponent();
        GenerateMaze();
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        GenerateMaze();
    }

    private void SolveButton_Click(object sender, RoutedEventArgs e)
    {
        if (mazeGenerator == null) return;

        var maze = mazeGenerator.GenerateMaze();
        var solver = new MazeSolver(maze);
        solutionPath = solver.Solve();

        if (solutionPath.Count == 0)
        {
            MessageBox.Show("No path found!");
            return;
        }

        // Start animation
        currentPathIndex = 0;
        animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(50)
        };
        animationTimer.Tick += AnimationTimer_Tick;
        animationTimer.Start();
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        if (solutionPath == null || currentPathIndex >= solutionPath.Count)
        {
            animationTimer?.Stop();
            return;
        }

        var point = solutionPath[currentPathIndex];
        var cell = new Rectangle
        {
            Width = CellSize,
            Height = CellSize,
            Fill = Brushes.Blue,
            Opacity = 0.5
        };

        Canvas.SetLeft(cell, point.X * CellSize);
        Canvas.SetTop(cell, point.Y * CellSize);
        MazeCanvas.Children.Add(cell);

        currentPathIndex++;
    }

    private void GenerateMaze()
    {
        if (!int.TryParse(WidthInput.Text, out int width) || !int.TryParse(HeightInput.Text, out int height))
        {
            MessageBox.Show("Please enter valid numbers for width and height.");
            return;
        }

        mazeGenerator = new MazeGenerator(width, height);
        var maze = mazeGenerator.GenerateMaze();
        DrawMaze(maze);
    }

    private void DrawMaze(char[,] maze)
    {
        MazeCanvas.Children.Clear();
        
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);

        // Calculate canvas size based on maze dimensions
        MazeCanvas.Width = width * CellSize;
        MazeCanvas.Height = height * CellSize;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cell = new Rectangle
                {
                    Width = CellSize,
                    Height = CellSize,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                // Set cell color based on content
                switch (maze[x, y])
                {
                    case '#': // Wall
                        cell.Fill = Brushes.Black;
                        break;
                    case '.': // Path
                        cell.Fill = Brushes.White;
                        break;
                    case 'S': // Start
                        cell.Fill = Brushes.Green;
                        break;
                    case 'E': // End
                        cell.Fill = Brushes.Red;
                        break;
                }

                Canvas.SetLeft(cell, x * CellSize);
                Canvas.SetTop(cell, y * CellSize);
                MazeCanvas.Children.Add(cell);
            }
        }
    }
}