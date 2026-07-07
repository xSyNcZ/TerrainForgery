using BoardMeshStudio.Core.Generators;
using BoardMeshStudio.Export;

var samplesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "samples");
Directory.CreateDirectory(samplesDirectory);

var outputPath = Path.Combine(samplesDirectory, "hill_test.stl");
var settings = new HillGenerationSettings
{
    Width = 120.0,
    Depth = 120.0,
    Height = 30.0,
    Resolution = 48,
    NoiseStrength = 1.75,
    Seed = 20260707,
    BaseThickness = 5.0
};

var generator = new HillGenerator();
var mesh = generator.Generate(settings);

IModelExporter exporter = new StlAsciiExporter();
exporter.Export(mesh, outputPath);

Console.WriteLine($"STL file: {outputPath}");
Console.WriteLine($"Triangles: {mesh.Triangles.Count}");
