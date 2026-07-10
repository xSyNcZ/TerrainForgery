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
    private bool _isDraggingReferenceMiniature;
    private readonly HashSet<Model3D> _referenceMiniatureModels = new();
    private double _cameraYaw = -45.0;
    private double _cameraPitch = 35.0;
    private double _cameraDistance = 220.0;
    private Point3D _cameraTarget = new(0.0, 0.0, 0.0);
    private double _referenceMiniatureX;
    private double _referenceMiniatureY;
    private bool _referenceMiniatureWasMoved;
    private double _activeScaleMillimeters = TerrainScaleCalculator.BaselineScaleMillimeters;
    private TerrainGeneratorType _activeGeneratorType = TerrainGeneratorType.Hill;

    public MainWindow()
    {
        InitializeComponent();
        _activeScaleMillimeters = ReadSelectedScale();
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

    private void SettingsControl_Changed(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        GeneratePreview();
    }

    private void GeneratorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        var generatorType = GetSelectedGeneratorType();
        if (generatorType != _activeGeneratorType)
        {
            ApplyGeneratorDimensionPreset(generatorType);
            _activeGeneratorType = generatorType;
        }

        GeneratePreview();
    }

    private void ScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded)
        {
            return;
        }

        ApplyScaleChange();
    }

    private void CustomScaleTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded || ScaleComboBox.SelectedIndex != 9)
        {
            return;
        }

        ApplyScaleChange();
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

    private void RandomizeSeedButton_Click(object sender, RoutedEventArgs e)
    {
        SeedTextBox.Text = Random.Shared.Next(0, int.MaxValue).ToString(CultureInfo.InvariantCulture);
        GeneratePreview();
    }

    private void ApplyScaleChange()
    {
        try
        {
            var newScale = ReadSelectedScale();
            var factor = newScale / _activeScaleMillimeters;
            ScaleTextBoxValue(WidthTextBox, factor);
            ScaleTextBoxValue(DepthTextBox, factor);
            ScaleTextBoxValue(HeightTextBox, factor);
            ScaleTextBoxValue(ReferenceMiniatureTextBox, factor);
            ScaleTextBoxValue(NoiseTextBox, factor);
            ScaleTextBoxValue(BaseThicknessTextBox, factor);
            ScaleTextBoxValue(OuterWallThicknessTextBox, factor);
            _referenceMiniatureX *= factor;
            _referenceMiniatureY *= factor;
            _activeScaleMillimeters = newScale;
            if (ScaleComboBox.SelectedIndex != 9)
            {
                CustomScaleTextBox.Text = newScale.ToString("0.###", CultureInfo.InvariantCulture);
            }

            GeneratePreview();
        }
        catch (Exception exception)
        {
            SetStatus(exception.Message);
        }
    }

    private static void ScaleTextBoxValue(TextBox textBox, double factor)
    {
        var value = ReadDouble(textBox, textBox.Name);
        textBox.Text = (value * factor).ToString("0.###", CultureInfo.InvariantCulture);
    }

    private void GeneratePreview(bool resetCameraAfterGeneration = true)
    {
        try
        {
            var settings = ReadSettings();
            var generatorType = GetSelectedGeneratorType();
            var generator = TerrainGeneratorFactory.Create(generatorType);
            var mesh = generator.Generate(settings);

            _currentMesh = mesh;
            _currentBounds = MeshBoundsCalculator.Calculate(mesh);
            AutoPositionReferenceMiniature(settings, _currentBounds);
            RenderMesh(mesh, settings);
            if (resetCameraAfterGeneration)
            {
                ResetCamera();
            }
            else
            {
                UpdateCamera();
            }
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
            ScaleMillimeters = TerrainScaleCalculator.BaselineScaleMillimeters,
            ReferenceMiniatureHeight = ReadDouble(ReferenceMiniatureTextBox, "Reference miniature"),
            Resolution = ReadInt(ResolutionTextBox, "Resolution"),
            TargetTriangleCount = ReadOptionalInt(TargetTriangleCountTextBox, "Target triangles"),
            NoiseStrength = ReadDouble(NoiseTextBox, "Noise strength"),
            Seed = ReadInt(SeedTextBox, "Seed"),
            BaseThickness = ReadDouble(BaseThicknessTextBox, "Base thickness"),
            OuterWallThickness = ReadDouble(OuterWallThicknessTextBox, "Outer wall thickness"),
            IncludeBase = IncludeBaseCheckBox.IsChecked == true,
            Style = GetSelectedStyle()
        };
    }

    private void ApplyGeneratorDimensionPreset(TerrainGeneratorType generatorType)
    {
        if (generatorType != TerrainGeneratorType.BlockBuilding)
        {
            return;
        }

        var miniatureHeight = Math.Max(1.0, ReadDouble(ReferenceMiniatureTextBox, "Reference miniature"));
        SetTextBoxValue(WidthTextBox, Math.Max(ReadDouble(WidthTextBox, "Width"), miniatureHeight * 6.0));
        SetTextBoxValue(DepthTextBox, Math.Max(ReadDouble(DepthTextBox, "Depth"), miniatureHeight * 4.0));
        SetTextBoxValue(HeightTextBox, Math.Max(ReadDouble(HeightTextBox, "Height"), miniatureHeight * 2.8));
    }

    private static void SetTextBoxValue(TextBox textBox, double value)
    {
        textBox.Text = value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private void AutoPositionReferenceMiniature(HillGenerationSettings settings, MeshBounds bounds)
    {
        if (_referenceMiniatureWasMoved)
        {
            return;
        }

        var miniatureHeight = Math.Max(4.0, settings.ReferenceMiniatureHeight);
        var offset = Math.Max(6.0, miniatureHeight * 0.28);
        _referenceMiniatureX = bounds.MaxX + offset;
        _referenceMiniatureY = bounds.CenterY;
    }

    private double ReadSelectedScale()
    {
        return ScaleComboBox.SelectedIndex switch
        {
            0 => 6.0,
            1 => 10.0,
            2 => 15.0,
            3 => 20.0,
            4 => 25.0,
            5 => 28.0,
            6 => 32.0,
            7 => 35.0,
            8 => 54.0,
            _ => ReadDouble(CustomScaleTextBox, "Custom scale")
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
            _ => TerrainStyle.RuggedNatural
        };
    }

    private void RenderMesh(CoreMesh mesh, HillGenerationSettings settings)
    {
        _scene.Children.Clear();
        _referenceMiniatureModels.Clear();
        _scene.Children.Add(new AmbientLight(Color.FromRgb(80, 86, 96)));
        _scene.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-0.45, -0.35, -0.8)));
        if (_currentBounds is not null)
        {
            AddScaleGrid(_currentBounds);
        }

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

        var material = new DiffuseMaterial(new SolidColorBrush(GetStyleColor(settings.Style)));
        var backMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(72, 91, 84)));
        _scene.Children.Add(new GeometryModel3D(geometry, material) { BackMaterial = backMaterial });
        AddReferenceMiniature(settings);
    }

    private static Color GetStyleColor(TerrainStyle style)
    {
        return style switch
        {
            TerrainStyle.Realistic => Color.FromRgb(104, 153, 121),
            TerrainStyle.Stylized => Color.FromRgb(122, 163, 193),
            TerrainStyle.AnimeInspired => Color.FromRgb(168, 139, 207),
            TerrainStyle.MiniatureFriendly => Color.FromRgb(128, 167, 116),
            TerrainStyle.LowPoly => Color.FromRgb(181, 151, 93),
            TerrainStyle.RuggedNatural => Color.FromRgb(126, 116, 96),
            _ => Color.FromRgb(126, 116, 96)
        };
    }

    private void AddReferenceMiniature(HillGenerationSettings settings)
    {
        if (ShowReferenceMiniatureCheckBox.IsChecked != true)
        {
            return;
        }

        var x = _referenceMiniatureX;
        var y = _referenceMiniatureY;
        var height = Math.Max(1.0, settings.ReferenceMiniatureHeight);
        var baseHeight = height * 0.08;
        var legHeight = height * 0.30;
        var bodyHeight = height * 0.44;
        var headSize = height * 0.18;
        var baseDiameter = Math.Max(1.2, height * 0.32);
        var footWidth = baseDiameter * 0.24;
        var torsoWidth = baseDiameter * 0.28;
        var torsoDepth = baseDiameter * 0.18;
        var material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(221, 84, 70)));
        var darkMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(120, 45, 42)));

        var baseZ = baseHeight / 2.0;
        var legsZ = baseHeight + legHeight / 2.0;
        var bodyZ = baseHeight + legHeight + bodyHeight / 2.0;
        var headZ = height - headSize / 2.0;

        AddReferenceMiniatureModel(CreateBoxModel(x, y, baseZ, baseDiameter, baseDiameter, baseHeight, darkMaterial));
        AddReferenceMiniatureModel(CreateBoxModel(x - footWidth * 0.35, y, legsZ, footWidth, footWidth, legHeight, material));
        AddReferenceMiniatureModel(CreateBoxModel(x + footWidth * 0.35, y, legsZ, footWidth, footWidth, legHeight, material));
        AddReferenceMiniatureModel(CreateBoxModel(x, y, bodyZ, torsoWidth, torsoDepth, bodyHeight, material));
        AddReferenceMiniatureModel(CreateBoxModel(x, y, headZ, headSize, headSize, headSize, material));
    }

    private void AddReferenceMiniatureModel(GeometryModel3D model)
    {
        _referenceMiniatureModels.Add(model);
        _scene.Children.Add(model);
    }

    private void AddScaleGrid(MeshBounds bounds)
    {
        var interval = ChooseGridInterval(bounds.LargestDimension);
        var minX = Math.Floor(bounds.MinX / interval) * interval;
        var maxX = Math.Ceiling(bounds.MaxX / interval) * interval;
        var minY = Math.Floor(bounds.MinY / interval) * interval;
        var maxY = Math.Ceiling(bounds.MaxY / interval) * interval;
        var z = Math.Min(0.0, bounds.MinZ) - 0.12;
        var thickness = Math.Max(0.25, interval * 0.025);
        var gridMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(65, 73, 83)));
        var axisMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(116, 135, 154)));

        for (var x = minX; x <= maxX + 0.001; x += interval)
        {
            var material = Math.Abs(x) < 0.001 ? axisMaterial : gridMaterial;
            _scene.Children.Add(CreateBoxModel(x, (minY + maxY) / 2.0, z, thickness, maxY - minY, thickness, material));
        }

        for (var y = minY; y <= maxY + 0.001; y += interval)
        {
            var material = Math.Abs(y) < 0.001 ? axisMaterial : gridMaterial;
            _scene.Children.Add(CreateBoxModel((minX + maxX) / 2.0, y, z, maxX - minX, thickness, thickness, material));
        }
    }

    private static double ChooseGridInterval(double largestDimension)
    {
        if (largestDimension <= 80.0)
        {
            return 10.0;
        }

        if (largestDimension <= 220.0)
        {
            return 25.0;
        }

        return 50.0;
    }

    private static GeometryModel3D CreateBoxModel(
        double centerX,
        double centerY,
        double centerZ,
        double width,
        double depth,
        double height,
        Material material)
    {
        var geometry = new MeshGeometry3D();
        var hx = width / 2.0;
        var hy = depth / 2.0;
        var hz = height / 2.0;

        var p000 = new Point3D(centerX - hx, centerY - hy, centerZ - hz);
        var p100 = new Point3D(centerX + hx, centerY - hy, centerZ - hz);
        var p110 = new Point3D(centerX + hx, centerY + hy, centerZ - hz);
        var p010 = new Point3D(centerX - hx, centerY + hy, centerZ - hz);
        var p001 = new Point3D(centerX - hx, centerY - hy, centerZ + hz);
        var p101 = new Point3D(centerX + hx, centerY - hy, centerZ + hz);
        var p111 = new Point3D(centerX + hx, centerY + hy, centerZ + hz);
        var p011 = new Point3D(centerX - hx, centerY + hy, centerZ + hz);

        AddQuad(geometry, p001, p101, p111, p011);
        AddQuad(geometry, p000, p010, p110, p100);
        AddQuad(geometry, p000, p100, p101, p001);
        AddQuad(geometry, p100, p110, p111, p101);
        AddQuad(geometry, p110, p010, p011, p111);
        AddQuad(geometry, p010, p000, p001, p011);

        return new GeometryModel3D(geometry, material);
    }

    private static void AddQuad(MeshGeometry3D geometry, Point3D a, Point3D b, Point3D c, Point3D d)
    {
        var start = geometry.Positions.Count;
        var normal = CalculateNormal(a, b, c);
        geometry.Positions.Add(a);
        geometry.Positions.Add(b);
        geometry.Positions.Add(c);
        geometry.Positions.Add(d);
        geometry.TriangleIndices.Add(start);
        geometry.TriangleIndices.Add(start + 1);
        geometry.TriangleIndices.Add(start + 2);
        geometry.TriangleIndices.Add(start);
        geometry.TriangleIndices.Add(start + 2);
        geometry.TriangleIndices.Add(start + 3);
        geometry.Normals.Add(normal);
        geometry.Normals.Add(normal);
        geometry.Normals.Add(normal);
        geometry.Normals.Add(normal);
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
        var selectedScale = ReadSelectedScale();
        GeneratorValueTextBlock.Text = generatorType.ToString();
        StyleValueTextBlock.Text = GetStyleLabel(settings.Style);
        ScaleValueTextBlock.Text = string.Format(
            CultureInfo.InvariantCulture,
            "{0:0.#} mm (x{1:0.###})",
            selectedScale,
            TerrainScaleCalculator.GetScaleFactor(selectedScale));
        ReferenceMiniatureValueTextBlock.Text = ShowReferenceMiniatureCheckBox.IsChecked == true
            ? string.Format(CultureInfo.InvariantCulture, "{0:0.#} mm", settings.ReferenceMiniatureHeight)
            : "Hidden";
        ReferenceMiniaturePositionTextBlock.Text = string.Format(
            CultureInfo.InvariantCulture,
            "{0:0.#} x {1:0.#} mm",
            _referenceMiniatureX,
            _referenceMiniatureY);
        FinalDimensionsTextBlock.Text = string.Format(
            CultureInfo.InvariantCulture,
            "{0:0.#} x {1:0.#} x {2:0.#} mm",
            settings.Width,
            settings.Depth,
            settings.Height);
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

    private static string GetStyleLabel(TerrainStyle style)
    {
        return style switch
        {
            TerrainStyle.Realistic => "Realistic",
            TerrainStyle.Stylized => "Stylized",
            TerrainStyle.AnimeInspired => "Anime-inspired",
            TerrainStyle.MiniatureFriendly => "Miniature-friendly",
            TerrainStyle.LowPoly => "Low-poly",
            TerrainStyle.RuggedNatural => "Rugged / natural",
            _ => style.ToString()
        };
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
        _lastMousePosition = e.GetPosition(PreviewViewport);
        _isDraggingReferenceMiniature = e.ChangedButton == MouseButton.Left && IsReferenceMiniatureHit(e.GetPosition(PreviewViewport));
        _isOrbiting = e.ChangedButton == MouseButton.Left && !_isDraggingReferenceMiniature;
        _isPanning = e.ChangedButton == MouseButton.Right || e.ChangedButton == MouseButton.Middle;
        PreviewViewport.CaptureMouse();
    }

    private void PreviewViewport_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isOrbiting && !_isPanning && !_isDraggingReferenceMiniature)
        {
            return;
        }

        var position = e.GetPosition(PreviewViewport);
        var delta = position - _lastMousePosition;
        _lastMousePosition = position;

        if (_isDraggingReferenceMiniature)
        {
            DragReferenceMiniature(delta);
            RedrawPreviewWithoutRegeneration();
            return;
        }

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
        _isDraggingReferenceMiniature = false;
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

    private void RedrawPreviewWithoutRegeneration()
    {
        if (_currentMesh is null || _currentBounds is null)
        {
            return;
        }

        var settings = ReadSettings();
        RenderMesh(_currentMesh, settings);
        UpdateCamera();
        UpdateStats(GetSelectedGeneratorType(), settings, _currentMesh, _currentBounds);
    }

    private bool IsReferenceMiniatureHit(Point position)
    {
        if (ShowReferenceMiniatureCheckBox.IsChecked != true || _referenceMiniatureModels.Count == 0)
        {
            return false;
        }

        var hit = false;
        VisualTreeHelper.HitTest(
            PreviewViewport,
            null,
            result =>
            {
                if (result is RayHitTestResult rayHit && _referenceMiniatureModels.Contains(rayHit.ModelHit))
                {
                    hit = true;
                    return HitTestResultBehavior.Stop;
                }

                return HitTestResultBehavior.Continue;
            },
            new PointHitTestParameters(position));

        return hit;
    }

    private void DragReferenceMiniature(Vector delta)
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

        right.Z = 0.0;
        cameraUp.Z = 0.0;
        if (right.LengthSquared > 0.0)
        {
            right.Normalize();
        }

        if (cameraUp.LengthSquared > 0.0)
        {
            cameraUp.Normalize();
        }

        var scale = _cameraDistance / Math.Max(PreviewViewport.ActualWidth, 1.0);
        var offset = (delta.X * scale) * right + (-delta.Y * scale) * cameraUp;
        _referenceMiniatureX += offset.X;
        _referenceMiniatureY += offset.Y;
        _referenceMiniatureWasMoved = true;
    }

    private void SetStatus(string message)
    {
        StatusTextBlock.Text = message;
    }
}
