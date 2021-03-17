using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.IO;
using System.Xml;
using System.Windows.Xps.Packaging;
using System.IO.Packaging;
using System.Windows.Xps;
using System.Windows.Media;
using System.Threading.Tasks;

namespace CB.Helpers.ReportHelper
{
    /// <summary>
    /// Handles FixedDocument stuff for easier report generation
    /// </summary>
    public class DocumentEngine
    {

        private FixedDocument _doc;
        /// <summary>
        /// The document created
        /// </summary>
        public FixedDocument Document { get { return _doc; } }

        //private PageResolution _pageResolution = null;

        private Boolean _watermark = true;
        private String _watermarkXaml;

        private Thickness _printmargin;

           /*
            * FixedDocument
            * PageContent
            * FixedPage
            * Canvas
            * n UIElements
            */

        /// <summary>
        /// Creates a new DocumentEngine
        /// </summary>
        /// <param name="Printmargin">
        /// Printing Clipborder, usually the document gets the default printers A4 printable area as size,
        /// but to have a nicer and more homogeneous print look you can set a wider printmargin (negative bleedarea, cutline)
        /// </param>
        /// <param name="WatermarkPages">
        /// If true, the pages will have a randomly generated watermark in the background
        /// This is currently using the file watermark.xaml
        /// </param>
        public DocumentEngine(Thickness Printmargin, Boolean WatermarkPages = true)
        {
            _watermark = WatermarkPages;
            _printmargin = Printmargin;
            _doc = new FixedDocument();

            Size A4SizeFromDefaultPrinter = DocHelper.GetDefaultPrinterPrintSizeA4();

            if (A4SizeFromDefaultPrinter.IsEmpty) _doc.DocumentPaginator.PageSize = new Size(793.92, 1122.24); //default size for A4
            else _doc.DocumentPaginator.PageSize = A4SizeFromDefaultPrinter;

           
        }

        /// <summary>
        /// Save document as XPS file
        /// </summary>
        /// <param name="Filename"></param>
        public void SaveXPS(String Filename)
        {
            XpsDocument xps = new XpsDocument(Filename, FileAccess.Write, CompressionOption.Maximum);

            XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(xps);
            writer.Write(_doc);

            xps.Close();
        }

        /// <summary>
        /// NEEDS MICROSOFT PDF PRINTER TO BE INSTALLED (it's like an office thing)
        /// </summary>
        /// <param name="Filename">should be a FULL path..</param>
        public void SavePDF(String Filename)
        {
            // Convert FixedDocument to XPS file in memory
            var ms = new MemoryStream();
            var package = Package.Open(ms, FileMode.Create);
            var doc = new XpsDocument(package);
            var writer = XpsDocument.CreateXpsDocumentWriter(doc);
            writer.Write(_doc.DocumentPaginator);
            doc.Close();
            package.Close();

            // Get XPS file bytes
            var bytes = ms.ToArray();
            ms.Dispose();

            // Print to PDF
            PdfFilePrinter.PrintXpsToPdf(bytes, Filename, "PdfReport");
        }

        public async Task<Boolean> PrintDocumentOnDefaultPrinter()
        {
              return await Task<Boolean>.Run(() =>
              {
                  return DocHelper.PrintFixedDocumentOnDefaultPrinter(_doc);
              });
        }

        /// <summary>
        /// Not usable with custom controls which use classes as multi value properties
        /// Should be able to serialize a fixeddocument to a file which could be used as a page // template
        /// </summary>
        /// <param name="Filename"></param>
        public void SaveXAML(String Filename)
        {
            throw new NotImplementedException("XAML serialization with custom user controls not possible atm !");
            FileStream f = new FileStream(Filename, FileMode.Create, FileAccess.Write);
            XamlWriter.Save(_doc, f);
            f.Flush();
            f.Close();
        }

        //Dont try to understand it.. it will take days :)
        /// <summary>
        /// Adds a page to the document
        /// </summary>
        /// <param name="Margin">General page margin</param>
        /// <returns>The canvas of the page you can put your content into</returns>
        public Canvas AddPage(Thickness Margin)
        {
            /*
             * Document
             *      0..n PageContent
             *              1 FixedPage
             *                  1 printMarginContent : manages nicely cut off borders for better looks when using backgrounds, also is used for the watermark !!!
             *                      [1 Border : Debug view]
             *                      1 content : used for the actual content, can have an additional margin for the content, cannot be used for backgrounds (margin!)
             *                      
             *                      content is returned
             *                      if background is important we need to refactor this
             */

            Size fpSize = _doc.DocumentPaginator.PageSize;

            //Always remember to set the size of the FixedPage to the PageSize -> otherwise Borders !
            FixedPage fp = new FixedPage();
            fp.Width = fpSize.Width;
            fp.Height = fpSize.Height;
            //Set margin for not Bleeding any Content ! (not needed but nicer than cutting it off by the printer (looks more homogeneous)
            fp.Margin = _printmargin;

            Size pmSize = new Size(fpSize.Width - (_printmargin.Right + _printmargin.Left), fpSize.Height - (_printmargin.Bottom + _printmargin.Top));
            
            Canvas printMarginContent = new Canvas();
            //content.Margin = _printmargin;
            printMarginContent.Width = pmSize.Width;
            printMarginContent.Height = pmSize.Height;

            fp.Children.Add(printMarginContent);

            //content.ClipToBounds = true;
            //may be using BorderThis static method in the future (was not implemented back when this was written :D
            Boolean DebugBorder = false;

            Size bcSize;
            Border innerXaml = null;

            if (DebugBorder)
            {

                innerXaml = new Border();
                innerXaml.BorderThickness = new Thickness(1);
                innerXaml.BorderBrush = Brushes.Black;

                ((IAddChild)printMarginContent).AddChild(innerXaml);

                bcSize = new Size(pmSize.Width - 2, pmSize.Height - 2);

            } else
            {
                bcSize = pmSize;
            }

            Size contentSize = new Size(bcSize.Width - (Margin.Left + Margin.Right), bcSize.Height - (Margin.Top + Margin.Bottom));

            Canvas content = new Canvas();
            content.Margin = Margin;
            content.Width = contentSize.Width;
            content.Height = contentSize.Height;

            if (DebugBorder) ((IAddChild)innerXaml).AddChild(content);
            else ((IAddChild)printMarginContent).AddChild(content);

            //watermarking
            if (_watermark)
            {
                if (_watermarkXaml is null)
                {
                    CreateWatermark(printMarginContent, "watermark.xaml", 1, 4, 0.05);
                }
                Watermark(printMarginContent);
            }

            //create PageContent put FixedPage in it
            PageContent pc = new PageContent();
            pc.Width = fp.Width;
            pc.Height = fp.Height;
            ((IAddChild)pc).AddChild(fp);

            //Add PageContent to Document
            _doc.Pages.Add(pc);

            return content;
        }

        /// <summary>
        /// Creates a 3 Part grid on a Canvas and returns those as Canvas
        /// </summary>
        /// <param name="On">Target Canvas</param>
        /// <param name="HeaderHeight">Height of the Header</param>
        /// <param name="FooterHeight">Height of the Footer</param>
        /// <param name="HeaderCanvas">Header Canvas</param>
        /// <param name="ContentCanvas">Content Canvas</param>
        /// <param name="FooterCanvas">Footer Canvas</param>
        /// <param name="GridLines">optional Gridline Options for the grid created</param>
        public void CreateHeaderFooter(Canvas On, GridLength HeaderHeight, GridLength FooterHeight, out Canvas HeaderCanvas, out Canvas ContentCanvas, out Canvas FooterCanvas, GridLineOptions GridLines = null)
        {
            double ContentHeight = On.Height - HeaderHeight.Value - FooterHeight.Value;

            Grid nGrid = GridHelper.CreateGrid(On.Width, On.Height, new GridLength[] { new GridLength(On.Width) },
                new GridLength[] { HeaderHeight, new GridLength(), FooterHeight },
                Brushes.Transparent, GridLines);

            HeaderCanvas = new Canvas();
            HeaderCanvas.Width = On.Width;
            HeaderCanvas.Height = HeaderHeight.Value;
            GridHelper.PutInGrid(HeaderCanvas, nGrid, 0, 0);

            ContentCanvas = new Canvas();
            ContentCanvas.Width = On.Width;
            ContentCanvas.Height = ContentHeight; //Size of Grids are not calculated before actual rendering - thus actualheight = 0
            GridHelper.PutInGrid(ContentCanvas, nGrid, 0, 1);

            FooterCanvas = new Canvas();
            FooterCanvas.Width = On.Width;
            FooterCanvas.Height = FooterHeight.Value;
            GridHelper.PutInGrid(FooterCanvas, nGrid, 0, 2);

            DocHelper.PutElementOn(nGrid, On);
        }

        //Creates a random watermark canvas which is used by the watermark method
        private void CreateWatermark(Canvas On, String Filename, int numX = 5, int numY = 10, double opacity = 1.0, double angle = 20.0)
        {
            Random rand = new Random();

            Canvas watermarkCanvas = new Canvas();
            watermarkCanvas.Width = On.Width;
            watermarkCanvas.Height = On.Height;

            if (numX == 1) numX++;
            if (numY == 1) numY++;

            double WWidth = watermarkCanvas.Width / (numX-1);
            double WHeight = watermarkCanvas.Height / (numY-1);

            double shifty = rand.NextDouble();
            for (int y = 0; y <= numY; y++)
            {
                double shiftx = rand.NextDouble();
                for (int x = 0; x <= numX; x++)
                {
                    Viewbox watermark = DocHelper.LoadXAML<Viewbox>(Filename);
                    watermark.Width = WWidth * 0.9;
                    watermark.Height = WHeight;
                    watermark.Opacity = opacity;
                    watermarkCanvas.Children.Add(watermark);
                    //watermark.RenderTransform = new RotateTransform(angle, WWidth / 2, WHeight / 2);
                    Canvas.SetLeft(watermark, (x * WWidth) - ((WWidth)*shiftx));
                    Canvas.SetTop(watermark, (y * WHeight) - ((WHeight)*shifty));
                }
            }

            _watermarkXaml = XamlWriter.Save(watermarkCanvas);
        }

        //watermarks a Canvas
        private void Watermark(Canvas On)
        {
            Canvas watermark = XamlReader.Load(XmlReader.Create(new StringReader(_watermarkXaml))) as Canvas;
            Canvas.SetZIndex(watermark, -100); //background
            ((IAddChild)On).AddChild(watermark);
        }


    }
}
