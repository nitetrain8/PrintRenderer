using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Printing;
using PrintRenderer.TableRenderer;

namespace PrintRenderer
{

    // public API
    public partial class SimpleGridPrinter
    {
        public PrintDocument Document;

        public void AddRow(RowRenderer renderer)
        {
            _Rows.Add(renderer);
        }

        public SimpleGridPrinter()
        {
            _Init(null);
        }

        public SimpleGridPrinter(string printer_name)
        {
            _Init(printer_name);
        }

        public void Print()
        {
            Document.Print();
        }
    }

    // private methods
    public partial class SimpleGridPrinter
    {
        private RendererStack<RowRenderer> _Rows;

        private void _Init(string printer_name)
        {
            Document = new PrintDocument
            {
                PrintController = new StandardPrintController()
            };
            Document.PrintPage += Document_PrintPage;
            _Rows = new RendererStack<RowRenderer>();
        }

        private void Document_PrintPage(object sender, PrintPageEventArgs ev)
        {
            Render(ev.Graphics, ev.MarginBounds);
            ev.HasMorePages = _Rows.MoreContentAvailable;
        }

        private void Render(Graphics g, Rectangle bbox)
        {
            RowRenderer r = null;
            Rectangle used_bbox = bbox;
            RenderResult result = new RenderResult();

            r = _Rows.GetNext();
            if (r == null)
                return; // nothing to render

            // the render loop will normally loop forever if 
            // a cell cannot be rendered because it won't fit,
            // e.g. font size too large. By testing to see if the
            // first row can fit on the page, we can bail if 
            // we detect that the cell can't be rendered.
            // This only needs to be checked at the start, 
            // because an un-renderable row will fall through
            // and always appear as the first row on the next
            // loop. 

            if (!r.CanBeginRender(g, ref bbox))
                throw new PrintSizeException("Can't print: Cell cannot fit on page (probably font too large)");
            
            do
            {
                r.Render(g, ref used_bbox, ref result);
                if (r.MoreContentAvailable)
                {
                    break;
                }
                used_bbox.Y += result.BBox.Height;
                used_bbox.Height -= result.BBox.Height;
            } while ((r = _Rows.GetNext()) != null);
        }

        private void _ChoosePrinter(string printer_name)
        {
            if (string.IsNullOrWhiteSpace(printer_name))
            {
                return; // default printer
            }

            // verify printer exists
            foreach (string name in PrinterSettings.InstalledPrinters)
            {
                if (name == printer_name)
                {
                    Document.PrinterSettings.PrinterName = name;
                    return;
                }
            }
            throw new PrintRendererException($"Failed to find printer {printer_name}");
        }
    }
}
