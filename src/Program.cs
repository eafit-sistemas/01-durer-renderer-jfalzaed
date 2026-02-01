using System;
using SkiaSharp;

public static class Program
{
    public static void Main()
    {
        InputData data = InputData.LoadFromJson("input.json");

        Shape2D projected = ProjectShape(data.Model, data.Parameters);

        Render(projected, data.Parameters, "output.jpg");
    }

    // =========================
    // PROJECTION + PRINT
    // =========================
    private static Shape2D ProjectShape(Model3D model, RenderParameters parameters)
    {
        int n = model.VertexTable.Length;

        float[][] projected = new float[n][];
        float[][] screen = new float[n][];

        // -------- Projection --------
        for (int i = 0; i < n; i++)
        {
            float x = model.VertexTable[i][0];
            float y = model.VertexTable[i][1];
            float z = model.VertexTable[i][2];

            float xp = x / z;
            float yp = y / z;

            projected[i] = new float[] { xp, yp };
        }

        // -------- Print projected vertices --------
        Console.WriteLine("CUBE:");
        Console.WriteLine("Projected vertices:");
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"[{i}] ({projected[i][0]}, {projected[i][1]})");
        }

        // -------- Screen transformation --------
        for (int i = 0; i < n; i++)
        {
            float sx = (projected[i][0] - parameters.XMin)
                        / (parameters.XMax - parameters.XMin)
                        * parameters.Resolution;

            float sy = (parameters.YMax - projected[i][1])
                        / (parameters.YMax - parameters.YMin)
                        * parameters.Resolution;

            screen[i] = new float[] { sx, sy };
        }

        // -------- Print screen coordinates --------
        Console.WriteLine("Screen coordinates:");
        for (int i = 0; i < n; i++)
        {
            Console.WriteLine($"[{i}] ({screen[i][0]}, {screen[i][1]})");
        }

        return new Shape2D
        {
            Points = screen,
            Lines = model.EdgeTable
        };
    }

    // =========================
    // RENDER WITH SKIASHARP
    // =========================
    private static void Render(Shape2D shape, RenderParameters parameters, string outputPath)
    {
        int res = parameters.Resolution;

        using SKBitmap bitmap = new(res, res);
        using SKCanvas canvas = new(bitmap);
        canvas.Clear(SKColors.White);

        using SKPaint paint = new()
        {
            Color = SKColors.Black,
            StrokeWidth = 2,
            IsAntialias = true
        };

        foreach (var line in shape.Lines)
        {
            var p1 = shape.Points[line[0]];
            var p2 = shape.Points[line[1]];

            canvas.DrawLine(
                p1[0], p1[1],
                p2[0], p2[1],
                paint
            );
        }

        using SKFileWStream fs = new(outputPath);
        bitmap.Encode(fs, SKEncodedImageFormat.Png, 100);
    }
}

