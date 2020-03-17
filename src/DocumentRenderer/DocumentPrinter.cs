using PrintRenderer.TableRenderer;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;

namespace PrintRenderer
{

    // public API
    public partial class SimpleGridPrinter
    {
        public PrintDocument Document;

        public void AddRow(IRenderer renderer)
        {
            _Renderers.Add(renderer);
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

    // private & internal methods
    public partial class SimpleGridPrinter
    {

        private List<IRenderer> _Renderers;
        private int _CurrentIndex;

        private void _Init(string printer_name)
        {
            Document = new PrintDocument
            {
                PrintController = new StandardPrintController()
            };
            Document.PrintPage += OnPagePrint;
            _Renderers = new List<IRenderer>();
            _CurrentIndex = 0;
        }

        private void Document_PrintPage(object sender, PrintPageEventArgs ev)
        {
            bool more = RenderPage(ev.Graphics, ev.MarginBounds);
            ev.HasMorePages = more;
        }

        private void _ThrowIfCannotRender(IRenderer r, Graphics g, ref Rectangle page_area)
        {
            if (!r.CanBeginRender(g, ref page_area))
            {
                throw new Exceptions.DoesNotFitOnPageException($"{r.ToString()} too large. Required: {page_area.ToString()}");
            }
        }

        private void OnPagePrint(object sender, PrintPageEventArgs ev)
        {
            if (_CurrentIndex >= _Renderers.Count)
            {
                return;
            }
            bool more = RenderPage(ev.Graphics, ev.MarginBounds);
            ev.HasMorePages = more;
        }

        private bool RenderPage(Graphics g, Rectangle page_area)
        {
            IRenderer r;
            RenderResult result;
            Rectangle bbox = page_area; // copy-by-value

            // Page rendering can always proceed as long as the first
            // renderer can begin rendering on the page, even if rendering
            // is complete. If it can't, then rendering cannot proceed. 
            _CheckCanRenderOnPage(g, ref page_area);

            // loop forever until end-of-page or no more renderers available. 
            do
            {
                r = _Renderers[_CurrentIndex];
                result = r.Render(g, ref bbox);

                switch (result)
                {
                    case RenderResult.Done:
                        _CurrentIndex += 1;
                        break;

                    case RenderResult.Incomplete:
                        // end of page.
                        return true;

                    case RenderResult.Fail:
                        throw new Exceptions.PrintRendererException($"Rendering failed: {r.ToString()}");
                }

                bbox.Height -= r.LastRenderArea.Height;
                bbox.Y += r.LastRenderArea.Height;
            } while (_CurrentIndex < _Renderers.Count);

            return false;
        }

        private void _CheckCanRenderOnPage(Graphics g, ref Rectangle page_area)
        {
            IRenderer r = _Renderers[_CurrentIndex];
            bool can_render = r.CanBeginRender(g, ref page_area);
            if (!can_render)
                throw new Exceptions.DoesNotFitOnPageException($"{r.ToString()}");
        }

        //private void RenderPage(Graphics g, Rectangle bbox)
        //{
        //    RowRenderer r = null;
        //    Rectangle used_bbox = bbox;
        //    RenderResult result = new RenderResult();

        //    r = _Rows.GetNext();
        //    if (r == null)
        //    {
        //        return; // nothing to render
        //    }

        //    // the render loop will normally loop forever if 
        //    // a cell cannot be rendered because it won't fit,
        //    // e.g. font size too large. By testing to see if the
        //    // first row can fit on the page, we can bail if 
        //    // we detect that the cell can't be rendered.
        //    // This only needs to be checked at the start, 
        //    // because an un-renderable row will fall through
        //    // and always appear as the first row on the next
        //    // loop. 

        //    if (!r.CanBeginRender(g, ref bbox))
        //    {
        //        throw new PrintSizeException("Can't print: Cell cannot fit on page (probably font too large)");
        //    }

        //    do
        //    {
        //        r.Render(g, ref used_bbox, ref result);
        //        if (r.MoreContentAvailable)
        //        {
        //            break;
        //        }
        //        used_bbox.Y += result.BBox.Height;
        //        used_bbox.Height -= result.BBox.Height;
        //    } while ((r = _Rows.GetNext()) != null);
        //}

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
            throw new Exceptions.PrintRendererException($"Failed to find printer {printer_name}");
        }
    }
}
