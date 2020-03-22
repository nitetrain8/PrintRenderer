using PrintRenderer.TableRenderer;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;

namespace PrintRenderer
{

    /// <summary>
    /// Simple grid-based printer thingy.
    /// </summary>
    public partial class SimpleGridPrinter
    {
        /// <summary>
        /// The print document.
        /// </summary>
        public PrintDocument Document;

        /// <summary>
        /// Add a renderer. 
        /// </summary>
        /// <param name="renderer"></param>
        public void AddRow(Renderer renderer)
        {
            Renderers.Add(renderer);
        }

        /// <summary>
        /// Create a new SimpleGridPrinter.
        /// </summary>
        public SimpleGridPrinter()
        {
            _Init(null);
        }

        /// <summary>
        /// Create a SimpleGridPrinter using the indicated printer.
        /// </summary>
        /// <param name="printer_name">Name of the printer to use.</param>
        public SimpleGridPrinter(string printer_name)
        {
            _Init(printer_name);
        }

        /// <summary>
        /// Begin printing. 
        /// </summary>
        public void Print()
        {
            Document.Print();
        }
    }

    // private & internal methods
    public partial class SimpleGridPrinter
    {

        private List<Renderer> Renderers;
        private int CurrentIndex;

        private void _Init(string printer_name)
        {
            Document = new PrintDocument
            {
                PrintController = new StandardPrintController()
            };
            Document.PrintPage += OnPagePrint;
            Renderers = new List<Renderer>();
            CurrentIndex = 0;
            _ChoosePrinter(printer_name);
        }

        private void OnPagePrint(object sender, PrintPageEventArgs ev)
        {
            if (CurrentIndex >= Renderers.Count)
            {
                return;
            }
            RenderResult more = Render(ev.Graphics, ev.MarginBounds);
            ev.HasMorePages = more == RenderResult.Incomplete;
        }

        public RenderResult Render(Graphics g, Rectangle bbox)
        {
            Renderer r;
            RenderResult result;

            // Page rendering can always proceed as long as the first
            // renderer can begin rendering on the page, even if rendering
            // is incomplete. If it can't, then rendering cannot proceed. 
            _CheckCanRenderOnPage(g, ref bbox);

            // loop forever until end-of-page or no more renderers available. 
            do
            {
                r = Renderers[CurrentIndex];
                result = r.Render(g, ref bbox);
                if (result == RenderResult.Incomplete)
                    return RenderResult.Incomplete;

                CurrentIndex += 1;

                bbox.Height -= r.LastRenderArea.Height;
                bbox.Y += r.LastRenderArea.Height;
            } while (CurrentIndex < Renderers.Count);

            return RenderResult.Done;
        }

        private void _CheckCanRenderOnPage(Graphics g, ref Rectangle page_area)
        {
            Renderer r = Renderers[CurrentIndex];
            bool can_render = r.CanBeginRender(g, ref page_area);
            if (!can_render)
                throw new Exceptions.DoesNotFitOnPageException($"{r.ToString()}");
        }

        private void _ChoosePrinter(string printer_name)
        {
            if (string.IsNullOrWhiteSpace(printer_name))
            {
                return; // default printer
            }
            var lower_name = printer_name.ToLower();
            foreach (string name in PrinterSettings.InstalledPrinters)
            {
                if (name.ToLower() == lower_name)
                {
                    Document.PrinterSettings.PrinterName = name;
                    return;
                }
            }
            throw new Exceptions.PrintRendererException($"Failed to find printer {printer_name}");
        }
    }
}
