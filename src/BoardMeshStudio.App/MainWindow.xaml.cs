using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using BoardMeshStudio.Core.Analysis;
using BoardMeshStudio.Core.Generators;
using BoardMeshStudio.Export;
using Microsoft.Win32;
using CoreMesh = BoardMeshStudio.Core.Geometry.Mesh;

namespace BoardMeshStudio.App;

public partial class MainWindow : Window
{
    private readonly Model3DGroup _scene = new();
    private readonly ModelVisual3D _sceneVisual = new();
    private CoreMesh? _currentMesh;
    private MeshBounds? _currentBounds;
    private Point _lastMousePosition;
    private bool _isOrbiting;
    private bool _isPanning;
    private double _cameraYaw = -45.0;
    private double _cameraPitch = 35.0;
    private double _cameraDistance = 220.0;
    private Point3D _cameraTarget = new(0.0, 0.0, 0.0);

    public MainWindow()
    {
        InitializeComponent();
        InitializeScene();
        GeneratePreview();
    }

    private void InitializeScene()
    {
        PreviewViewport.Children.Add(_sceneVisual);
        _sceneVisual.Content = _scene;
        ResetCamera();
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        GeneratePreview();
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentMesh is null)
        {
            SetStatus("Generate a mesh before exporting.");
            return;
        }

        var dialog = new SaveFileDialog
        {
            Title = "Export STL",
            Filter = "STL files (*.stl)|*.stl",
            FileName = $"{GetSelectedGeneratorType().ToString().ToLowerInvariant()}_preview.stl",
            InitialDirectory = Path.Combine(Directory.GetCurrentDirectory(), "samples")
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        new StlAsciiExporter().Export(_currentMesh, dialog.FileName);
        ExportPathTextBlock.Text = dialog.FileName;
        SetStatus($"Exported {dialog.FileName}");
    }

    private void ResetCameraButton_Click(object sender, RoutedEventArgs e)
    {
        ResetCamera();
    }

    private void GeneratePreview()
    {
        try
        {
            var settings = ReadSettings();
            var generatorType = GetSelectedGeneratorType();
            var generator = TerrainGeneratorFactory.Create(generatorType);
            var mesh = generator.Generate(settings);

            _currentMesh = mesh;
            _currentBounds = MeshBoundsCalculator.Calculate(mesh);
            RenderMesh(mesh);
            ResetCamera();
            UpdateStats(generatorType, settings, mesh, _currentBounds);
            SetStatus($"Generated {generatorType} mesh with {mesh.Triangles.Count} triangles.");
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message);
            MessageBox.Show(this, exception.Message, "Generation failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private HillGenerationSettings ReadSettings()
    {
        return new HillGenerationSettings
        {
            Width = ReadDouble(WidthTextBox, "Width"),
            Depth = ReadDouble(DepthTextBox, "Depth"),
            Height = ReadDouble(HeightTextBox, "Height"),
            Resolution = ReadInt(ResolutionTextBox, "Resolution"),
            TargetTriangleCount = ReadOptionalInt(TargetTriangleCountTextBox, "Target triangles"),
            NoiseStrength = ReadDouble(NoiseTextBox, "Noise strength"),
            Seed = ReadInt(SeedTextBox, "Seed"),
            BaseThickness = ReadDouble(BaseThicknessTextBox, "Base thickness"),
            IncludeBase = IncludeBaseCheckBox.IsChecked == true,
            Style = GetSelectedStyle()
        };
    }

    private static double ReadDouble(TextBox textBox, string label)
    {
        if (double.TryParse(textBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        if (double.TryParse(textBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
        {
            return value;
        }

        throw new InvalidOperationException($"{label} must be a number.");
    }

    private static int ReadInt(TextBox textBox, string label)
    {
        if (int.TryParse(textBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        throw new InvalidOperationException($"{label} must be an integer.");
    }

    private static int? ReadOptionalInt(TextBox textBox, string label)
    {
        if (string.IsNullOrWhiteSpace(textBox.Text))
        {
            return null;
        }

        if (int.TryParse(textBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
        {
            return value;
        }

        throw new InvalidOperationException($"{label} must be an integer.");
    }

    private TerrainGeneratorType GetSelectedGeneratorType()
    {
        return GeneratorComboBox.SelectedIndex switch
        {
            0 => TerrainGeneratorType.Hill,
            1 => TerrainGeneratorType.Crater,
            2 => TerrainGeneratorType.Rock,
            3 => TerrainGeneratorType.Wall,
            4 => TerrainGeneratorType.BlockBuilding,
            _ => TerrainGeneratorType.Hill
        };
    }

    private TerrainStyle GetSelectedStyle()
    {
        return StyleComboBox.SelectedIndex switch
        {
            0 => TerrainStyle.Realistic,
            1 => TerrainStyle.Stylized,
            2 => TerrainStyle.AnimeInspired,
            3 => TerrainStyle.MiniatureFriendly,
            4 => TerrainStyle.LowPoly,
            5 => TerrainStyle.RuggedNatural,
            _ => TerrainStyle.Realistic
        };
    }

    private void RenderMesh(CoreMesh mesh)
    {
        _scene.Children.Clear();
        _scene.Children.Add(new AmbientLight(Color.FromRgb(80, 86, 96)));
        _scene.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-0.45, -0.35, -0.8)));

        var geometry = new MeshGeometry3D();
        var index = 0;

        foreach (var triangle in mesh.Triangles)
        {
            var a = ToPoint3D(triangle.A);
            var b = ToPoint3D(triangle.B);
            var c = ToPoint3D(triangle.C);
            var normal = CalculateNormal(a, b, c);

            geometry.Positions.Add(a);
            geometry.Positions.Add(b);
            geometry.Positions.Add(c);
            geometry.TriangleIndices.Add(index++);
            geometry.TriangleIndices.Add(index++);
            geometry.TriangleIndices.Add(index++);
            geometry.Normals.Add(normal);
            geometry.Normals.Add(normal);
            geometry.Normals.Add(normal);
        }

        var material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(104, 153, 121)));
        var backMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(72, 91, 84)));
        _scene.Children.Add(new GeometryModel3D(geometry, material) { BackMaterial = backMaterial });
    }

    private static Point3D ToPoint3D(BoardMeshStudio.Core.Geometry.Vertex vertex)
    {
        return new Point3D(vertex.X, vertex.Y, vertex.Z);
    }

    private static Vector3D CalculateNormal(Point3D a, Point3D b, Point3D c)
    {
        var normal = Vector3D.CrossProduct(b - a, c - a);
        if (normal.LengthSquared > 0.0)
        {
            normal.Normalize();
        }

        return normal;
    }

    private void UpdateStats(TerrainGeneratorType generatorType, HillGenerationSettings settings, CoreMesh mesh, MeshBounds bounds)
    {
        GeneratorValueTextBlock.Text = generatorType.ToString();
        StyleValueTextBlock.Text = settings.Style.ToString();
        BaseValueTextBlock.Text = settings.IncludeBase ? "With base" : "Without base";
        TriangleCountTextBlock.Text = mesh.Triangles.Count.ToString(CultureInfo.InvariantCulture);
        TargetTriangleCountValueTextBlock.Text = settings.TargetTriangleCount?.ToString(CultureInfo.InvariantCulture) ?? "-";
        BoundsTextBlock.Text = string.Format(
            CultureInfo.InvariantCulture,
            "{0:0.#} x {1:0.#} x {2:0.#} mm",
            bounds.Width,
            bounds.Depth,
            bounds.Height);
    }

    private void ResetCamera()
    {
        if (_currentBounds is not null)
        {
            _cameraTarget = new Point3D(_currentBounds.CenterX, _currentBounds.CenterY, _currentBounds.CenterZ);
            _cameraDistance = Math.Max(60.0, _currentBounds.LargestDimension * 1.8);
        }

        _cameraYaw = -45.0;
        _cameraPitch = 35.0;
        UpdateCamera();
    }

    private void UpdateCamera()
    {
        var yaw = DegreesToRadians(_cameraYaw);
        var pitch = DegreesToRadians(_cameraPitch);
        var horizontal = _cameraDistance * Math.Cos(pitch);
        var position = new Point3D(
            _cameraTarget.X + horizontal * Math.Cos(yaw),
            _cameraTarget.Y + horizontal * Math.Sin(yaw),
            _cameraTarget.Z + _cameraDistance * Math.Sin(pitch));

        PreviewCamera.Position = position;
        PreviewCamera.LookDirection = _cameraTarget - position;
        PreviewCamera.UpDirection = new Vector3D(0.0, 0.0, 1.0);
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

    private void PreviewViewport_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _lastMousePosition = e.GetPosition(this);
        _isOrbiting = e.ChangedButton == MouseButton.Left;
        _isPanning = e.ChangedButton == MouseButton.Right || e.ChangedButton == MouseButton.Middle;
        PreviewViewport.CaptureMouse();
    }

    private void PreviewViewport_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isOrbiting && !_isPanning)
        {
            return;
        }

        var position = e.GetPosition(this);
        var delta = position - _lastMousePosition;
        _lastMousePosition = position;

        if (_isOrbiting)
        {
            _cameraYaw += delta.X * 0.35;
            _cameraPitch = Math.Clamp(_cameraPitch - delta.Y * 0.35, 8.0, 82.0);
        }
        else if (_isPanning)
        {
            PanCamera(delta);
        }

        UpdateCamera();
    }

    private void PreviewViewport_MouseUp(object sender, MouseButtonEventArgs e)
    {
        _isOrbiting = false;
        _isPanning = false;
        PreviewViewport.ReleaseMouseCapture();
    }

    private void PreviewViewport_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        var factor = e.Delta > 0 ? 0.88 : 1.12;
        _cameraDistance = Math.Clamp(_cameraDistance * factor, 8.0, 5000.0);
        UpdateCamera();
    }

    private void PanCamera(Vector delta)
    {
        var look = PreviewCamera.LookDirection;
        if (look.LengthSquared == 0.0)
        {
            return;
        }

        look.Normalize();
        var up = PreviewCamera.UpDirection;
        up.Normalize();
        var right = Vector3D.CrossProduct(look, up);
        if (right.LengthSquared == 0.0)
        {
            return;
        }

        right.Normalize();
        var cameraUp = Vector3D.CrossProduct(right, look);
        cameraUp.Normalize();

        var scale = _cameraDistance / Math.Max(PreviewViewport.ActualWidth, 1.0);
        var offset = (-delta.X * scale) * right + (delta.Y * scale) * cameraUp;
        _cameraTarget += offset;
    }

    private void SetStatus(string message)
    {
        StatusTextBlock.Text = message;
    }
}
