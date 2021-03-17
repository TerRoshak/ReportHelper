using System;
using System.IO;
using System.Linq;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Xps;

namespace CB.Helpers.ReportHelper
{
    /// <summary>
    /// Document generation Helpers and Tools
    /// </summary>
    public static class DocHelper
    {

        public class FontSettings
        {
            public FontFamily Family = new FontFamily("Calibri");
            public double Size = 8.0;
            public FontStyle Style = FontStyles.Normal;
            public FontWeight Weight = FontWeights.Normal;
            public FontStretch Stretch = FontStretches.Normal;
            public Brush Foreground = Brushes.Black;
            public Brush Background = Brushes.Transparent;
        }

        #region General Helper

        /// <summary>
        /// Converts mm to Dibs (96 dpi)
        /// </summary>
        /// <param name="MM">mm</param>
        /// <returns>Dibs</returns>
        public static Double MM2Dibs(Double MM)
        {
            double defaultDibsPerInch = 96;
            double defaultMMPerInch = 25.4;

            return ((MM / defaultMMPerInch) * defaultDibsPerInch);
        }

        /// <summary>
        /// For placing images you want to get the optimal pixelresolution for a certain size
        /// This method calculates the Pixel Size which would be the optimum regarding the default printers' resolution
        /// </summary>
        /// <param name="MM">mm size of image you want to put on a canvas</param>
        /// <returns>pixel size needed to optimally use the printers resolution</returns>
        public static Size MM2PixelsDefaultDPI(Size MM)
        {
            PageResolution pr = GetDefaultPrinterQueue().UserPrintTicket.PageResolution;
            return new Size((int)((MM.Width / 25.4) * (double)pr.X),(int)( (MM.Height / 25.4) * (double)pr.Y));
        }

        #endregion

        /// <summary>
        /// Centers a FrameworkElement on a canvas
        /// The Framework element should be a child of the canvas
        /// Make sure both Element and On have Width and Height properties !
        /// </summary>
        /// <param name="element">Element to be centered</param>
        /// <param name="On">Canvas from which element is a child of</param>
        /// <param name="Horizontal">Horizontal centering</param>
        /// <param name="Vertical">Vertical centering</param>
        public static void CenterObject(FrameworkElement element, Canvas On, Boolean Horizontal = true, Boolean Vertical = true)
        {

            if (Horizontal)
            {
                double left = (On.Width - element.Width) / 2;
                Canvas.SetLeft(element, left);
            }

            if (Vertical)
            {
                double top = (On.Height - element.Height) / 2;
                Canvas.SetTop(element, top);
            }
        }

        /// <summary>
        /// Puts element in a Border
        /// Make sure element has Width and Height !
        /// </summary>
        /// <param name="element">Element to be bordered</param>
        /// <param name="BorderColor">Color or the Border</param>
        /// <param name="BorderThickness">Thickness of the Borderline</param>
        /// <returns>The new border element</returns>
        public static Border BorderThis(FrameworkElement element, Brush BorderColor, Thickness BorderThickness)
        {
            Border nBorder = new Border();
            if ((element.Width != 0) && (!double.IsNaN(element.Width)))
            {
                nBorder.Width = element.Width + (BorderThickness.Left + BorderThickness.Right);
            }
            if ((element.Height != 0) && (!double.IsNaN(element.Height)))
            {
                nBorder.Height = element.Height + (BorderThickness.Top + BorderThickness.Bottom);
            }
            nBorder.BorderThickness = BorderThickness;
            nBorder.BorderBrush = BorderColor;

            ((IAddChild)nBorder).AddChild(element);
            return nBorder;
        }

        public static TextBlock NewText(String txt, FontSettings FontSetting = null, Thickness? Padding = null)
        {
            TextBlock result = new TextBlock();
            result.TextAlignment = TextAlignment.Center;
            result.Text = txt;

            FontSettings fs = new FontSettings();
            if (!(FontSetting is null)) fs = FontSetting;

            result.FontFamily = fs.Family;
            result.FontSize = fs.Size;
            result.FontStyle = fs.Style;
            result.FontStretch = fs.Stretch;
            result.FontWeight = fs.Weight;
            result.Foreground = fs.Foreground;
            result.Background = fs.Background;
            
            result.Padding = (Padding is null) ? new Thickness(0) : (Thickness)Padding;

            result.Measure(new Size(9999999999, 9999999999)); //srsly chris .. i'm disappointed in myself.
            result.Width = result.DesiredSize.Width;
            result.Height = result.DesiredSize.Height;

            return result;
        }

        /// <summary>
        /// Puts an Xaml file on a Canvas
        /// </summary>
        /// <typeparam name="T">Type of element in the xaml file</typeparam>
        /// <param name="Filename">Filename</param>
        /// <param name="On">Canvas on which the element should be put on</param>
        /// <param name="Width">Expected Width (optional if available in the root object)</param>
        /// <param name="Height">Expected Height (optional if available in the root object)</param>
        /// <param name="CenterHorizontal">Center object horizontally</param>
        /// <param name="CenterVertical">Center object vertically</param>
        /// <param name="Left">Overrides centering</param>
        /// <param name="Top">Overrides centering</param>
        /// <param name="Right">Overrides centering</param>
        /// <param name="Bottom">Overrides centering</param>
        /// <returns>Object of type T which was put on the canvas</returns>
        public static T PutXamlOn<T>(String Filename, Canvas On, double? Width = null, double? Height = null, Boolean CenterHorizontal = false, Boolean CenterVertical = false, double? Left = null, double? Top = null, double? Right = null, double? Bottom = null) where T: FrameworkElement
        {
            T newXaml = LoadXAML<T>(Filename);

            PutElementOn(newXaml, On, Width, Height, CenterHorizontal, CenterVertical, Left, Top, Right, Bottom);

            return newXaml;
        }

        /// <summary>
        /// Puts an element on a canvas
        /// </summary>
        /// <param name="element">The element which you want to put on the canvas</param>
        /// <param name="On">Canvas on which the element should be put on</param>
        /// <param name="Width">Expected Width (optional if available in the root object)</param>
        /// <param name="Height">Expected Height (optional if available in the root object)</param>
        /// <param name="CenterHorizontal">Center object horizontally</param>
        /// <param name="CenterVertical">Center object vertically</param>
        /// <param name="Left">Overrides centering</param>
        /// <param name="Top">Overrides centering</param>
        /// <param name="Right">Overrides centering</param>
        /// <param name="Bottom">Overrides centering</param>
        public static void PutElementOn(FrameworkElement element, Canvas On, double? Width = null, double? Height = null, Boolean CenterHorizontal = false, Boolean CenterVertical = false, double? Left = null, double? Top = null, double? Right = null, double? Bottom = null)
        {
            On.Children.Add(element);

            if (!(Width is null)) element.Width = (double)Width;
            if (!(Height is null)) element.Height = (double)Height;

            if (CenterHorizontal || CenterVertical)
            {
                CenterObject(element, On, CenterHorizontal, CenterVertical);
            }

            if (!(Left is null)) Canvas.SetLeft(element, (double)Left);
            if (!(Top is null)) Canvas.SetTop(element, (double)Top);
            if (!(Right is null)) Canvas.SetRight(element, (double)Right);
            if (!(Bottom is null)) Canvas.SetBottom(element, (double)Bottom);

            element.Measure(new Size(On.Width, On.Height));
        }

        /// <summary>
        /// Loads a xaml file and returns the FrameworkElement
        /// </summary>
        /// <typeparam name="T">Classtype of root element</typeparam>
        /// <param name="XamlFile">Filename</param>
        /// <returns>The FrameworkElement or default if not loadable</returns>
        public static T LoadXAML<T>(String XamlFile)
        {
            try
            {
                StreamReader mysr = new StreamReader(XamlFile);
                T rootObject = (T)XamlReader.Load(mysr.BaseStream);
                //ButtoninXAML = LogicalTreeHelper.FindLogicalNode(rootObject, "button1") as Button;
                //ButtoninXAML.Click += new RoutedEventHandler(Button_Click);
                return rootObject;
            }
            catch (FileNotFoundException)
            {
                //MessageBox.Show(ex.Message.ToString());
            }

            return default;
        }

        #region Printer related

        public static Boolean PrintFixedDocumentOnDefaultPrinter(FixedDocument doc)
        {
            PrintQueue pq = GetDefaultPrinterQueue();
            if (pq.IsXpsDevice)
            {
                XpsDocumentWriter wr = PrintQueue.CreateXpsDocumentWriter(pq);
                wr.Write(doc.DocumentPaginator);
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Returns the systems default printer queue with preset PageMediaSize
        /// </summary>
        /// <param name="pname">PageMediaSize you want to verify exists on the printer</param>
        /// <returns>PrinterQueue or null if pname was not available on the default printer</returns>
        public static PrintQueue GetDefaultPrinterQueue(PageMediaSizeName pname = PageMediaSizeName.ISOA4)
        {
            LocalPrintServer ls = new LocalPrintServer();
            //PrintQueueCollection pqc = ls.GetPrintQueues();
            PrintQueue defaultPrinter = ls.DefaultPrintQueue;

            PrintCapabilities pc = defaultPrinter.GetPrintCapabilities();
            var result = pc.PageMediaSizeCapability.Where(x => x.PageMediaSizeName == pname);
            if (result.Count() > 0)
            {
                PageMediaSize pms = result.First();
                PrintTicket newTicket = new PrintTicket() { PageMediaSize = pms };
                System.Printing.ValidationResult vr = defaultPrinter.MergeAndValidatePrintTicket(defaultPrinter.UserPrintTicket, newTicket);
                if (vr.ValidatedPrintTicket.PageMediaSize.PageMediaSizeName == pname)
                {
                    defaultPrinter.UserPrintTicket = vr.ValidatedPrintTicket;
                    defaultPrinter.Commit();
                    return defaultPrinter;
                }
                else return null;
            }
            else return null;
        }

        /// <summary>
        /// Returns the PrintableArea of the systems default printer for a PageMediaSize of ISOA4
        /// </summary>
        /// <returns>s.o.</returns>
        public static Size GetDefaultPrinterPrintSizeA4()
        {
            PrintQueue pq = GetDefaultPrinterQueue(); //gets default a4 ticket

            if (pq != null)
            {
                PrintCapabilities pc = pq.GetPrintCapabilities();
                if (pc != null)
                {
                    if ((pc.OrientedPageMediaWidth != null) && (pc.OrientedPageMediaHeight != null))
                    {
                        return new Size((double)pc.OrientedPageMediaWidth, (double)pc.OrientedPageMediaHeight);
                    }
                    else return Size.Empty;
                }
                else return Size.Empty;
            }
            else return Size.Empty;
        }

        #endregion
    }
}
