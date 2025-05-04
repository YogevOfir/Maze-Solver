using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Text;
using System.IO;

namespace MazeSolverNew;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private const int DefaultCellSize = 30;
    private int currentCellSize = DefaultCellSize;
    private char[,]? currentMaze;
    private MazeGenerator? mazeGenerator;
    private List<Point>? solutionPath;
    private int currentPathIndex;
    private DispatcherTimer? animationTimer;

    public MainWindow()
    {
        InitializeComponent();
        ZoomSlider.Value = DefaultCellSize;
        ZoomText.Text = $"{DefaultCellSize}px";
        GenerateMaze();
    }

    private void GenerateMaze()
    {
        if (!int.TryParse(WidthInput.Text, out int width) || !int.TryParse(HeightInput.Text, out int height))
        {
            MessageBox.Show("Please enter valid numbers for width and height.", "Invalid Input", 
                          MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (width < 5 || height < 5)
        {
            MessageBox.Show("Width and height must be at least 5.", "Invalid Size", 
                          MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (width > 73 || height > 73)
        {
            MessageBox.Show("Width and height cannot exceed 50.", "Invalid Size", 
                          MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        StatusText.Text = "Generating maze...";
        mazeGenerator = new MazeGenerator(width, height);
        currentMaze = mazeGenerator.GenerateMaze();
        DrawMaze(currentMaze);
        StatusText.Text = "Maze generated. Click 'Solve Maze' to find a path.";
    }

    private void DrawMaze(char[,] maze)
    {
        if (maze == null) return;

        currentMaze = maze;
        MazeCanvas.Children.Clear();
        
        int width = maze.GetLength(0);
        int height = maze.GetLength(1);

        // Calculate canvas size based on maze dimensions and current cell size
        MazeCanvas.Width = width * currentCellSize;
        MazeCanvas.Height = height * currentCellSize;

        // Build debug text
        var debugText = new StringBuilder();
        debugText.AppendLine("Maze Debug View:");
        debugText.AppendLine("Legend: # = Wall, . = Path, S = Start, E = End");
        debugText.AppendLine();

        // First pass: draw the maze and collect debug info
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = new Rectangle
                {
                    Width = currentCellSize,
                    Height = currentCellSize,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1
                };

                // Set cell color based on content
                switch (maze[x, y])
                {
                    case '#': // Wall
                        cell.Fill = Brushes.Black;
                        debugText.Append('#');
                        break;
                    case '.': // Path
                        cell.Fill = Brushes.White;
                        debugText.Append('.');
                        break;
                    case 'S': // Start
                        cell.Fill = Brushes.Green;
                        debugText.Append('S');
                        break;
                    case 'E': // End
                        cell.Fill = Brushes.Red;
                        debugText.Append('E');
                        break;
                    default:
                        cell.Fill = Brushes.Purple; // Unknown cell type
                        debugText.Append('?');
                        break;
                }
                debugText.Append(' ');

                Canvas.SetLeft(cell, x * currentCellSize);
                Canvas.SetTop(cell, y * currentCellSize);
                MazeCanvas.Children.Add(cell);
            }
            debugText.AppendLine();
        }

        // Write to file
        try
        {
            File.WriteAllText("maze_debug.txt", debugText.ToString());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error writing debug file: {ex.Message}", "Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void SpeedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (SpeedText != null)
        {
            SpeedText.Text = $"{e.NewValue}ms";
            if (animationTimer != null)
            {
                animationTimer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
            }
        }
    }

    private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        currentCellSize = (int)e.NewValue;
        if (ZoomText != null)
        {
            ZoomText.Text = $"{currentCellSize}px";
        }
        if (currentMaze != null)
        {
            DrawMaze(currentMaze);
        }
    }

    private void ZoomInButton_Click(object sender, RoutedEventArgs e)
    {
        if (ZoomSlider.Value < ZoomSlider.Maximum)
        {
            ZoomSlider.Value += 5;
        }
    }

    private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
    {
        if (ZoomSlider.Value > ZoomSlider.Minimum)
        {
            ZoomSlider.Value -= 5;
        }
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        GenerateMaze();
    }

    private void SolveButton_Click(object sender, RoutedEventArgs e)
    {
        if (mazeGenerator == null || currentMaze == null) return;

        StatusText.Text = "Solving maze...";
        var solver = new MazeSolver(currentMaze);
        solutionPath = solver.Solve();

        if (solutionPath.Count == 0)
        {
            MessageBox.Show("No path found!", "No Solution", 
                          MessageBoxButton.OK, MessageBoxImage.Information);
            StatusText.Text = "No path found.";
            return;
        }

        StatusText.Text = "Path found. Animating solution...";
        // Start animation
        currentPathIndex = 0;
        animationTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(SpeedSlider.Value)
        };
        animationTimer.Tick += AnimationTimer_Tick;
        animationTimer.Start();
    }

    private void AnimationTimer_Tick(object? sender, EventArgs e)
    {
        if (solutionPath == null || currentPathIndex >= solutionPath.Count)
        {
            animationTimer?.Stop();
            StatusText.Text = "Solution complete.";
            return;
        }

        var point = solutionPath[currentPathIndex];
        var cell = new Rectangle
        {
            Width = currentCellSize,
            Height = currentCellSize,
            Fill = Brushes.Blue,
            Opacity = 0.5
        };

        Canvas.SetLeft(cell, point.X * currentCellSize);
        Canvas.SetTop(cell, point.Y * currentCellSize);
        MazeCanvas.Children.Add(cell);

        currentPathIndex++;
        StatusText.Text = $"Animating solution... {currentPathIndex}/{solutionPath.Count}";
    }
}