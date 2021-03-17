using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CB.Helpers.ReportHelper
{

    [Serializable()]
    public class GridLineOptions
    {
        public GridLinesVisibilityEnum Visibility = GridLinesVisibilityEnum.Both;
        /// <summary>
        /// Used for border and all other lines, when not defined otherwise
        /// </summary>
        public Pen GridLinesPen = new Pen(Brushes.Black, 1.0d);
        public Pen VerticalGridLinesPen = null;
        public Pen HorizontalGridLinesPen = null;
        public Pen HeaderGridLinesPen = null;

        public GridLineOptions()
        {

        }
    }

    #region GridLinesVisibilityEnum

    //Upside down, binary jenga
    [Serializable]
    [Flags]
    public enum GridLinesVisibilityEnum
    {
        AllWithHeader = 31,
        All = 7,
        VerticalHeader = 16,
        HorizontalHeader = 8,
        Both = 6,
        Vertical = 4,
        Horizontal = 2,
        Border = 1,
        None = 0
    }
    #endregion

    /// <summary>
    /// Thanx to
    /// https://www.codeproject.com/Tips/1039691/WPF-Grid-Control-with-Solid-GridLines
    /// Vishal.KumarSingh
    /// 
    /// Modified by Christian Bendl
    ///     - Added Border
    ///     - Simplified drawing
    /// </summary>
    public class CustomGridWithLines : Grid
    {

        #region Properties
        public bool ShowCustomGridLines
        {
            get { return (bool)GetValue(ShowCustomGridLinesProperty); }
            set { SetValue(ShowCustomGridLinesProperty, value); }
        }

        public static readonly DependencyProperty ShowCustomGridLinesProperty =
            DependencyProperty.Register("ShowCustomGridLines", typeof(bool),
            typeof(CustomGridWithLines), new UIPropertyMetadata(false));


        public GridLineOptions GridLineSettings
        {
            get { return (GridLineOptions)GetValue(GridLineSettingsProperty); }
            set { SetValue(GridLineSettingsProperty, value); }
        }

        public static readonly DependencyProperty GridLineSettingsProperty =
            DependencyProperty.Register("GridLineSettings",
        typeof(GridLineOptions), typeof(CustomGridWithLines),
        new UIPropertyMetadata(new GridLineOptions()));

        #endregion

        protected override void OnRender(DrawingContext dc)
        {
            //first draw content
            base.OnRender(dc);

            //then the actual lines or they wont be visible
            if (GridLineSettings.Visibility != GridLinesVisibilityEnum.None)
            {

                if (ShowCustomGridLines)
                {
                    if (GridLineSettings.Visibility.HasFlag(GridLinesVisibilityEnum.Horizontal))
                    {
                        for (int r = 0; r < RowDefinitions.Count; r++)
                        {
                            //skip first
                            if (r == 0) continue;

                            Pen useThisPen = (GridLineSettings.HorizontalGridLinesPen is null) ? GridLineSettings.GridLinesPen : GridLineSettings.HorizontalGridLinesPen;

                            if (r == 1)
                            {
                                if (GridLineSettings.Visibility.HasFlag(GridLinesVisibilityEnum.HorizontalHeader)) continue;
                            }
                            dc.DrawLine(useThisPen,
                                new Point(0, RowDefinitions[r].Offset),
                                new Point(ActualWidth, RowDefinitions[r].Offset)
                            );
                        }
                    }

                    if (GridLineSettings.Visibility.HasFlag(GridLinesVisibilityEnum.Vertical))
                    {
                        for(int v=0;v<ColumnDefinitions.Count;v++)
                        {
                            //skip first
                            if (v == 0) continue;

                            Pen useThisPen = (GridLineSettings.VerticalGridLinesPen is null) ? GridLineSettings.GridLinesPen : GridLineSettings.VerticalGridLinesPen;

                            if (v == 1)
                            {
                                if (GridLineSettings.Visibility.HasFlag(GridLinesVisibilityEnum.VerticalHeader)) continue;
                            }
                            dc.DrawLine(useThisPen,
                                new Point(ColumnDefinitions[v].Offset, 0),
                                new Point(ColumnDefinitions[v].Offset, ActualHeight)
                            );
                        }
                    }

                    //vertical header line
                    if (GridLineSettings.Visibility.HasFlag(GridLinesVisibilityEnum.VerticalHeader))
                    {
                        Pen useThisPen = (GridLineSettings.HeaderGridLinesPen is null) ? GridLineSettings.GridLinesPen : GridLineSettings.HeaderGridLinesPen;
                        dc.DrawLine(useThisPen,
                            new Point(ColumnDefinitions[1].Offset, 0),
                            new Point(ColumnDefinitions[1].Offset, ActualHeight)
                        );
                    }

                    //horizontal header line
                    if (GridLineSettings.Visibility.HasFlag(GridLinesVisibilityEnum.HorizontalHeader))
                    {
                        Pen useThisPen = (GridLineSettings.HeaderGridLinesPen is null) ? GridLineSettings.GridLinesPen : GridLineSettings.HeaderGridLinesPen;
                        dc.DrawLine(useThisPen,
                            new Point(0, RowDefinitions[1].Offset),
                            new Point(ActualWidth, RowDefinitions[1].Offset)
                            );
                    }

                    //outer border last
                    if (GridLineSettings.Visibility.HasFlag(GridLinesVisibilityEnum.Border))
                    {
                        dc.DrawRectangle(Brushes.Transparent,
                            GridLineSettings.GridLinesPen,
                            new Rect(0, 0, ActualWidth, ActualHeight)
                        );
                    }

                }
            }
        }
        static CustomGridWithLines()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomGridWithLines),
            new FrameworkPropertyMetadata(typeof(CustomGridWithLines)));
        }
    }
}