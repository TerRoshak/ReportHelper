using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CB.Helpers.ReportHelper
{
    /// <summary>
    /// Helper methods for grid creation and manipulation
    /// </summary>
    public static class GridHelper
    {

        /// <summary>
        /// Creates a grid with equally sized columns and rows
        /// </summary>
        /// <param name="GridWidth"></param>
        /// <param name="GridHeight"></param>
        /// <param name="GridColumns"></param>
        /// <param name="GridRows"></param>
        /// <param name="Background"></param>
        /// <returns></returns>
        public static Grid CreateEquiSizedGrid(double GridWidth, double GridHeight, int GridColumns, int GridRows, Brush Background, CustomGridWithLines.GridLineOptions GridLines = null)
        {
            double CellWidth = GridWidth / GridColumns;
            double CellHeight = GridHeight / GridRows;

            GridLength[] columns = (new GridLength[GridColumns]);
            for (int i = 0; i < columns.Length; i++) columns[i] = new GridLength(CellWidth);
            GridLength[] rows = (new GridLength[GridRows]);
            for (int i = 0; i < rows.Length; i++) rows[i] = new GridLength(CellHeight);

            return CreateGrid(GridWidth, GridHeight, columns, rows, Background, GridLines);
        }

        /// <summary>
        /// Creates a grid with the corresponding GridLength values
        /// </summary>
        /// <param name="GridWidth"></param>
        /// <param name="GridHeight"></param>
        /// <param name="Columns"></param>
        /// <param name="Rows"></param>
        /// <param name="Background"></param>
        /// <returns></returns>
        public static Grid CreateGrid(double GridWidth, double GridHeight, GridLength[] Columns, GridLength[] Rows, Brush Background, CustomGridWithLines.GridLineOptions GridLines = null)
        {
            Grid nGrid;

            if (GridLines is null)
            {
                nGrid = new Grid();
            } else
            {
                nGrid = new CustomGridWithLines();
                (nGrid as CustomGridWithLines).ShowCustomGridLines = true;
                (nGrid as CustomGridWithLines).GridLineSettings = GridLines;
            }

            nGrid.Width = GridWidth;
            nGrid.Height = GridHeight;
            nGrid.Background = Background;

            for (int columns = 0; columns < Columns.Length; columns++)
            {
                ColumnDefinition cd = new ColumnDefinition();
                cd.Width = Columns[columns];
                nGrid.ColumnDefinitions.Add(cd);
            }
            for (int rows = 0; rows < Rows.Length; rows++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = Rows[rows];
                nGrid.RowDefinitions.Add(rd);
            }

            return nGrid;
        }

        /// <summary>
        /// Puts an FrameworkElement in a grid on the corresponding row and volumn
        /// </summary>
        /// <param name="element"></param>
        /// <param name="g"></param>
        /// <param name="Column">ZERO based</param>
        /// <param name="Row">ZERO based</param>
        public static void PutInGrid(FrameworkElement element, Grid g, int Column, int Row)
        {
            Grid.SetColumn(element, Column);
            Grid.SetRow(element, Row);
            g.Children.Add(element);
        }


        public enum FillMode
        {
            /// <summary>
            /// Meander from bottom Left
            /// ...
            /// 9,8,7,6,5
            /// 0,1,2,3,4
            /// </summary>
            MeanderBottomLeft,
            /// <summary>
            /// Linear from TopLeft to Bottom Right
            /// 0,1,2,3,4
            /// 5,6,7,8,9
            /// ...
            /// </summary>
            Linear
        }

        /// <summary>
        /// Takes a grids layout and uses FillAction to get a Canvas which gets
        /// put in the Corresponding grid cell.
        /// Meander (Mäander) starts at a Corner then goes trough the row turns
        /// and returns in the next row
        /// 
        /// At the moment it only starts in the bottom left corner !
        /// ---->!
        /// <-----
        /// *---->
        /// </summary>
        /// <param name="g">Grid to meander through</param>
        /// <param name="FillAction">A method which gets a 0 based cell index and return the canvas which gets put in the Gridcell</param>
        public static void FillGrid(Grid g, FillMode Mode, Func<int, FrameworkElement> FillAction)
        {
            int GridRows = g.RowDefinitions.Count;
            int GridColumns = g.ColumnDefinitions.Count;

            //meander placing starting bottom left
            for (int n = 0; n < GridRows * GridColumns; n++)
            {
                int row = 0;
                int column = 0;

                switch (Mode)
                {
                    case FillMode.MeanderBottomLeft:
                        row = GridRows - ((int)Math.Floor((double)(n / GridColumns))) - 1;
                        column = (row % 2 == 0) ?   //remember column/rows are 0..n-1
                                //Odd
                                (n % GridColumns) :
                                //Even
                                GridColumns - (n % GridColumns) - 1;
                        break;
                    case FillMode.Linear:
                        row = (int)Math.Floor((double)(n / GridColumns));
                        column = n % GridColumns;
                        break;
                }


                PutInGrid(FillAction(n), g, column, row);
                //bordered.Width = Helpers.MM2Dibs(25);
                //bordered.Height = Helpers.MM2Dibs(32);
            }
        }

    }
}
