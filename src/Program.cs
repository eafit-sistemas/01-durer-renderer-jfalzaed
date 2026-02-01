using System;
using System.Collections.Generic;
using SkiaSharp;

public static class Program
{
    public static void Main()
    {
        InputData data = InputData.LoadFromJson("input.json");

        Shape2D projected = ProjectShape(data.Model);

        projected.Print();

        try
        {
            Render(projected, data.Parameters, "output.png");
        }
        catch
        {
        }
    }

    private static Shape2D ProjectShape(Model3D model)
    {
        int n = model.VertexTable.Length;
        float[][] points = new float[n][];

        for (int i = 0; i < n; i++)
        {
            float x = model.VertexTable[i][0];
            float y = model.VertexTable[i][1];
            float z = model.VertexTable[i][2];

            points[i] = new float[]
            {
                x / z,
                y / z
            };
        }

        return new Shape2D
        {
            Points = points,
            Lines = model.EdgeTable
        };
    }

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
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };

        SKPoint[] screen = new SKPoint[shape.Points.Length];

        for (int i = 0; i < shape.Points.Length; i++)
        {
            float x = (shape.Points[i][0] - parameters.XMin)
                        / (parameters.XMax - parameters.XMin)
                        * res;

            float y = (parameters.YMax - shape.Points[i][1])
                        / (parameters.YMax - parameters.YMin)
                        * res;

            screen[i] = new SKPoint(x, y);
        }

        foreach (var edge in shape.Lines)
        {
            canvas.DrawLine(
                screen[edge[0]],
                screen[edge[1]],
                paint
            );
        }

        using SKFileWStream fs = new(outputPath);
        bitmap.Encode(fs, SKEncodedImageFormat.Png, 100);
    }
}
