using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace CB.Helpers.ReportHelper
{
    /// <summary>
    /// Drawing related Helpers and Tools
    /// </summary>
    public static class DrawingHelpers
    {
        //return a US formatted double string - used for Geometry constructors
        //may be put into Helpers instead..
        private static String USd(double d)
        {
            return d.ToString(CultureInfo.GetCultureInfo("en-US"));
        }

        /// <summary>
        /// Returns a square tiled DrawingBrush
        /// Does not work nicely with < 1 Linethickness atm
        /// </summary>
        /// <param name="Foreground">Line brush</param>
        /// <param name="Background">Background brush</param>
        /// <param name="Linethickness">Line Thickness</param>
        /// <returns></returns>
        public static DrawingBrush HatchRectBrush(Brush Foreground, Brush Background, double Linethickness)
        {
            DrawingGroup gg = new DrawingGroup();
            
            //Background
            gg.Children.Add(new GeometryDrawing(Background, new Pen(Foreground, Linethickness), new RectangleGeometry(new Rect(0, 0, 10, 10))));
            //https://docs.microsoft.com/de-de/dotnet/desktop/wpf/graphics-multimedia/path-markup-syntax
            //Move 0,10
            //Line 0,0 -> 0, 10 -> 10, 0 -> 10, 1 -> 1, 1 -> 1,10
            //gg.Children.Add(new GeometryDrawing(Background, new Pen(Foreground, Linethickness), Geometry.Parse("M0, 10 L 0, 0 10, 0 10, 1 1, 1 1, 10")));

            DrawingBrush result = new DrawingBrush(gg);
            result.TileMode = TileMode.Tile;
            result.Viewport = new Rect(0, 0, 10, 10);
            result.ViewportUnits = BrushMappingMode.Absolute;

            if (result.CanFreeze) result.Freeze();

            return result;
        }

        /// <summary>
        /// Creates a 45 degree (Topleft to Bottomright tiled DrawingBrush)
        /// </summary>
        /// <param name="Foreground">Linebrush</param>
        /// <param name="Background">Background</param>
        /// <param name="Linethickness">Line thickness</param>
        /// <param name="DistanceLines">Horizontal distance between lines in dibs</param>
        /// <returns></returns>
        public static DrawingBrush Hatch45Brush(Brush Foreground, Brush Background, double Linethickness, double DistanceLines)
        {
            DrawingGroup gg = new DrawingGroup();

            //Background
            gg.Children.Add(new GeometryDrawing(Background, new Pen(Background, Linethickness), new RectangleGeometry(new Rect(0, 0, DistanceLines, DistanceLines))));
            //https://docs.microsoft.com/de-de/dotnet/desktop/wpf/graphics-multimedia/path-markup-syntax
            //Move 0,10
            //Line 0,0 -> 0, 10 -> 10, 0 -> 10, 1 -> 1, 1 -> 1,10
            String LineGen = "M 0,0 " +
                             "L " + USd(DistanceLines) + "," + USd(DistanceLines) + " " +
                             "M " + USd(DistanceLines / 2.0) + "," + USd(-(DistanceLines / 2.0)) + " " +
                             "L " + USd(DistanceLines * 1.5) + "," + USd(DistanceLines / 2.0) + " " +
                             "M " + USd(-(DistanceLines / 2.0)) + "," + USd(DistanceLines / 2.0) + " " +
                             "L " + USd(DistanceLines / 2.0) + "," + USd(DistanceLines * 1.5);

            gg.Children.Add(new GeometryDrawing(Background, new Pen(Foreground, Linethickness),
                Geometry.Parse(LineGen)));
            //

            DrawingBrush result = new DrawingBrush(gg);
            result.TileMode = TileMode.Tile;
            result.Viewport = new Rect(0, 0, DistanceLines, DistanceLines);
            result.ViewportUnits = BrushMappingMode.Absolute;
            result.Viewbox = new Rect(0, 0, DistanceLines, DistanceLines);
            result.ViewboxUnits = BrushMappingMode.Absolute;

            if (result.CanFreeze) result.Freeze();

            return result;
        }
    }
}
