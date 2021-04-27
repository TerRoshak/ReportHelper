using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MathNet.Numerics;
using static CB.Helpers.ReportHelper.DocHelper;

namespace CB.Helpers.ReportHelper
{
    public enum PlotType { Dots, Line, Bar }
    public enum AxisValueType { Raw, OADate, OADateRelative, DataPointNumber }
    public enum AxisType { X, Y }

    public class PlotSettings
    {
        private List<AxisSettings> Axes = new List<AxisSettings>();
        private List<PlotData> Plots = new List<PlotData>();

        /// <summary>
        /// not implemented yet
        /// </summary>
        public Boolean ShowLegend = false;

        public string title = "";

        public FontSettings TitleFont = new FontSettings()
        {
            Size = DocHelper.MM2Dibs(4)
        };

        public FontSettings AxisFont = new FontSettings()
        {
            Size = DocHelper.MM2Dibs(3)
        };

        public AxisSettings First(AxisType t)
        {
            IEnumerable<AxisSettings> n = Axes.Where(x => x.AxisType == t);
            if (n.Count() < 1) return null;
            return n.ElementAt(0);
        }

        public AxisSettings Second(AxisType t)
        {
            IEnumerable<AxisSettings> n = Axes.Where(x => x.AxisType == t);
            if (n.Count() < 2) return null;
            return n.ElementAt(1);
        }

        public Boolean AddAxis(AxisSettings axis)
        {
            if (Axes.Count() == 4) return false;

            if (Axes.Where(x => x.AxisType == axis.AxisType).Count() == 2) return false;

            Axes.Add(axis);

            return true;
        }

        public Boolean AddData(PlotData Data)
        {
            if (Axes.Contains(Data.XAxis) && Axes.Contains(Data.YAxis))
            {
                Plots.Add(Data);
                Data.XAxis.Info.Merge(Data.Data.Select(x => x.Item1).ToArray());
                Data.YAxis.Info.Merge(Data.Data.Select(x => x.Item2).ToArray());
                return true;
            }
            else return false;
        }

        public PlotData[] GetPlotData()
        {
            return Plots.ToArray();
        }

        public AxisSettings[] GetAxisSettings()
        {
            return Axes.ToArray();
        }

    }

    public class AxisSettings
    {

        public int Divider = 10;
        public int SubDivider = 10;
        public string AxisText = "";
        public AxisValueType AxisValueType = AxisValueType.DataPointNumber;
        public String AxisValueFormat = "0.00";
        public AxisType AxisType = AxisType.X;

        public Brush AxisBrush = Brushes.LightGray;
        public double AxisThickness = DocHelper.MM2Dibs(0.2);

        public Boolean DrawAverageLine = false;

        public Boolean LongDivider = false;
        public Boolean LongSubDivider = false;

        public Brush DividerBrush = Brushes.Gray;
        public double DividerBrushThickness = DocHelper.MM2Dibs(0.2);
        public double DividerShortLength = DocHelper.MM2Dibs(0.5);

        public Brush SubDividerBrush = Brushes.LightGray;
        public double SubDividerBrushThickness = DocHelper.MM2Dibs(0.2);
        public double SubDividerShortLength = DocHelper.MM2Dibs(0.25);

        private DataInfo _info = new DataInfo();
        public DataInfo Info { get { return _info; } }

    }

    public class PlotData
    {
        public PlotType pType = PlotType.Line;
        /// <summary>
        /// Dots, Line use Item1 as X, Item2 as Y
        /// Bar uses X as midPoint, Y as Height
        /// Width is determined by the X distances
        /// </summary>
        public Tuple<double, double>[] Data;
        public AxisSettings XAxis;
        public AxisSettings YAxis;

        public Brush PlotColor = Brushes.Black;
        public double PlotThickness = DocHelper.MM2Dibs(0.25);
    }

    public class DataInfo {

        public double Max = 0;
        public double Min = 0;
        public double Range = 0;
        public int Values = 0;

        private Boolean empty = true;

        public DataInfo()
        {

        }

        public DataInfo(double[] Data)
        {
            Max = Data.Max();
            Min = Data.Min();
            Range = Max - Min;

            Values = Data.Length;

            empty = false;
        }

        public void Merge(double[] Data)
        {
            DataInfo info = new DataInfo(Data);

            if (empty)
            {
                Max = info.Max;
                Min = info.Min;
                Range = info.Range;
                Values = info.Values;
                empty = false;
                return;
            }

            Max = (info.Max > Max) ? info.Max : Max;
            Min = (info.Min > Min) ? info.Min : Min;

            Range = Max - Min;

            if (Data.Length > Values) Values = Data.Length;
        }

    }

    public static class PlotHelper
    {

        public static Canvas NewPlot(double Width, double Height, PlotSettings Options)
        {

            Canvas content = new Canvas
            {
                Width = Width,
                Height = Height,
                Background = Brushes.White,
                ClipToBounds = true
            };

            //PlotSettings plotSettings = Options;

            //get info for sizings ...


            Thickness Distance = new Thickness(DocHelper.MM2Dibs(15),
                                                DocHelper.MM2Dibs(15),
                                                DocHelper.MM2Dibs(15),
                                                DocHelper.MM2Dibs(7.5));

            Size PlotSize = new Size(Width - (Distance.Left + Distance.Right), Height - (Distance.Top + Distance.Bottom));
            
            
            //Plot

            //double xScaler = rangeX / plot.Width;
            //double yScaler = rangeY / plot.Height;

            Rect plotArea = new Rect(new Point(Distance.Left, Distance.Top), PlotSize);
            Size contentSize = new Size(Width, Height);

            //draw plots
            //plotline
            PlotData[] plotData = Options.GetPlotData();
            Canvas[] plots = new Canvas[plotData.Count()];
            for (int i = 0; i < plotData.Length; i++)
            {
                DocHelper.PutElementOn(NewPlotLine(plotData[i], PlotSize), content, Top: Distance.Top, Left: Distance.Left);
                //
                //plots[i] = NewPlotLine(plotData[i], PlotSize);
            }

            // AXIS
            DrawPlotAxis(content, Options.First(AxisType.X), contentSize, plotArea, true);
            DrawPlotAxis(content, Options.First(AxisType.Y), contentSize, plotArea, true);
            if (Options.Second(AxisType.X) != null)
            {
                DrawPlotAxis(content, Options.Second(AxisType.X), contentSize, plotArea, false);
            }
            if (Options.Second(AxisType.Y) != null)
            {
                DrawPlotAxis(content, Options.Second(AxisType.Y), contentSize, plotArea, false);
            }

            //draw title
            TextBlock titleText = DocHelper.NewText(Options.title,
                Options.TitleFont);

            DocHelper.PutElementOn(titleText, content, CenterHorizontal: true, Top: DocHelper.MM2Dibs(3));

            Rectangle border = new Rectangle
            {
                Width = Width,
                Height = Height,
                Stroke = Brushes.DarkGray,
                StrokeThickness = DocHelper.MM2Dibs(0.25)
            };

            DocHelper.PutElementOn(border, content);


            return content;
        }

        private static void DrawPlotAxis(Canvas On, AxisSettings axis, Size plotSize, Rect plotRect, Boolean First = true)
        {
            int dividers = axis.Divider * axis.SubDivider;
            Point StartPoint, EndPoint;

            //draw dividers and subdividers
            for (int i = 0; i < (dividers) + 1; i++)
            {
                Boolean isDivider = (i % axis.SubDivider == 0);

                if (axis.AxisType == AxisType.X)
                {
                    double Coord = plotRect.Left + (i * (plotRect.Width / dividers));
                    double yPos = (First) ? plotRect.Bottom : plotRect.Top;

                    StartPoint = new Point(
                        Coord,
                        yPos - (isDivider ? axis.DividerShortLength : axis.SubDividerShortLength));
                    EndPoint = new Point(
                        Coord,
                        yPos + (isDivider ? axis.DividerShortLength : axis.SubDividerShortLength));
                }
                else
                {
                    double Coord = plotRect.Top + (i * (plotRect.Height / dividers));
                    double xPos = (First) ? plotRect.Left : plotRect.Right;

                    StartPoint = new Point(
                        xPos - (isDivider ? axis.DividerShortLength : axis.SubDividerShortLength),
                        Coord);
                    EndPoint = new Point(
                        xPos + (isDivider ? axis.DividerShortLength : axis.SubDividerShortLength),
                        Coord);
                }

                Line dividerLine = new Line
                {
                    X1 = StartPoint.X,
                    Y1 = StartPoint.Y,
                    X2 = EndPoint.X,
                    Y2 = EndPoint.Y,
                    Stroke = (isDivider ? axis.DividerBrush : axis.SubDividerBrush),
                    StrokeThickness = (isDivider ? axis.DividerBrushThickness : axis.SubDividerBrushThickness),
                    StrokeStartLineCap = PenLineCap.Flat,
                    StrokeEndLineCap = PenLineCap.Triangle
                };

                DocHelper.PutElementOn(dividerLine, On);

                //if divider draw text
                if (isDivider)
                {
                    double val = 0;
                    if (axis.AxisType == AxisType.X)
                    {
                        val = axis.Info.Min + (i * (axis.Info.Range / dividers));
                    } else
                    {
                        val = axis.Info.Max - (i * (axis.Info.Range / dividers));
                    }

                    string valString = "";
                    switch (axis.AxisValueType)
                    {
                        case AxisValueType.Raw: valString = val.ToString(axis.AxisValueFormat); break;
                        case AxisValueType.DataPointNumber: valString = (axis.AxisType == AxisType.X)?(i * axis.Info.Values / dividers).ToString():""; break;
                        case AxisValueType.OADate: valString = (DateTime.FromOADate(val)).ToString(axis.AxisValueFormat); break;
                        case AxisValueType.OADateRelative: valString = (DateTime.FromOADate(val - axis.Info.Min)).ToString(axis.AxisValueFormat); break;
                    }

                    TextBlock axisValueText = DocHelper.NewText(valString,
                        new DocHelper.FontSettings
                        {
                            Size = DocHelper.MM2Dibs(3),
                        });

                    Point TextPosition;

                    if (axis.AxisType == AxisType.X)
                    {
                        TextPosition = new Point(StartPoint.X - (axisValueText.Width / 2), 0);

                        if (First)
                        {
                            TextPosition.Y = plotRect.Bottom + (axisValueText.Height * 0.5);
                        } else
                        {
                            TextPosition.Y = plotRect.Top - (axisValueText.Height * 1.5);
                        }
                    } else
                    {
                        TextPosition = new Point(0, StartPoint.Y - (axisValueText.Height / 2));

                        if (First)
                        {
                            TextPosition.X = (plotRect.Left - axisValueText.Width) / 2;
                        }
                        else
                        {
                            TextPosition.X = plotRect.Right + (((plotSize.Width - plotRect.Right) - axisValueText.Width) / 2);
                        }
                    }

                    DocHelper.PutElementOn(axisValueText, On, Left: TextPosition.X, Top: TextPosition.Y);
                }
            }

            //Draw axis
            StartPoint = new Point();
            EndPoint = new Point();

            if (axis.AxisType == AxisType.X)
            {
                if (First)
                {
                    StartPoint = plotRect.BottomLeft;
                    EndPoint = plotRect.BottomRight;
                } else
                {
                    StartPoint = plotRect.TopLeft;
                    EndPoint = plotRect.TopRight;
                }
            } else
            {
                if (First)
                {
                    StartPoint = plotRect.BottomLeft;
                    EndPoint = plotRect.TopLeft;
                }
                else
                {
                    StartPoint = plotRect.TopRight;
                    EndPoint = plotRect.BottomRight;
                }
            }

            Line axisLine = new Line
            {
                X1 = StartPoint.X,
                Y1 = StartPoint.Y,
                X2 = EndPoint.X,
                Y2 = EndPoint.Y,
                Stroke = axis.AxisBrush,
                StrokeThickness = axis.AxisThickness,
                StrokeStartLineCap = PenLineCap.Flat,
                StrokeEndLineCap = PenLineCap.Flat
            };

            DocHelper.PutElementOn(axisLine, On);

        }

        private static Canvas NewPlotLine(PlotData pd, Size sz)
        {
            
            //Plot
            Canvas plot = new Canvas()
            {
                Width = sz.Width,
                Height = sz.Height,
                Background = Brushes.Transparent
            };

            double xScaler = pd.XAxis.Info.Range / sz.Width;
            double yScaler = pd.YAxis.Info.Range / sz.Height;

            PointCollection pc = new PointCollection();
            for (int i = 0; i < pd.Data.Length; i++)
            {
                double xPart = ((pd.Data[i].Item1 - pd.XAxis.Info.Min) / pd.XAxis.Info.Range) *sz.Width;
                double yPart = (1.0d - ((pd.Data[i].Item2 - pd.YAxis.Info.Min) / pd.YAxis.Info.Range)) * sz.Height;
                pc.Add(new Point(xPart,yPart));
            }

            //plotline
            if (pd.pType == PlotType.Line)
            {
                Polyline pl = new Polyline
                {
                    Points = pc,
                    Stroke = pd.PlotColor,
                    StrokeThickness = pd.PlotThickness,
                    Width = sz.Width,
                    Height = sz.Height
                };

                DocHelper.PutElementOn(pl, plot, sz.Width, sz.Height);

            }
            else if (pd.pType == PlotType.Dots)
            {
                const float width = 2;
                const float radius = width / 2;
                foreach (Point point in pc)
                {
                    Ellipse ellipse = new Ellipse();
                    ellipse.SetValue(Canvas.LeftProperty, point.X - radius);
                    ellipse.SetValue(Canvas.TopProperty, point.Y - radius);
                    ellipse.Fill = pd.PlotColor;
                    ellipse.Stroke = pd.PlotColor;
                    ellipse.StrokeThickness = pd.PlotThickness;
                    ellipse.Width = width;
                    ellipse.Height = width;
                    plot.Children.Add(ellipse);
                }
            }

            /*
                        //draw average line
            double yAvg = y.Average();
            Line yAvgLine = new Line
            {
                X1 = 0,
                X2 = plot.Width,
                Y1 = plot.Height - ((yAvg - minY) / yScaler),
                Y2 = plot.Height - ((yAvg - minY) / yScaler),
                Stroke = Brushes.Red,
                StrokeThickness = DocHelper.MM2Dibs(0.2),
                StrokeStartLineCap = PenLineCap.Flat,
                StrokeEndLineCap = PenLineCap.Flat
            };

            DocHelper.PutElementOn(yAvgLine, plot);


            //simple linear regression
            Tuple<double, double> simpleLinear = Fit.Line(x, y);

            double fit = GoodnessOfFit.RSquared(x.Select(x => simpleLinear.Item1 + simpleLinear.Item2 * x), y);
            Line l = new Line
            {
                X1 = 0,
                X2 = Width,
                Y1 = (plot.Height - ((simpleLinear.Item2 - minY) / yScaler)),
                Y2 = (plot.Height - ((((simpleLinear.Item1 * rangeX) + simpleLinear.Item2) - minY) / yScaler)),
                Stroke = Brushes.Blue,
                StrokeThickness = DocHelper.MM2Dibs(0.75)
            };

            plot.Children.Add(l);
            //DocHelper.PutElementOn(l, content);
            */

            return plot;
        }

        public static Tuple<double,double>[] PackageData(double[] xs, double[] ys)
        {
            Tuple<double, double>[] d1 = new Tuple<double, double>[xs.Length];
            for (int i = 0; i < xs.Length; i++) { d1[i] = new Tuple<double, double>(xs[i], ys[i]); };
            return d1;
        }
    }
}
