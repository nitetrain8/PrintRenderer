﻿using PrintRenderer.TableRenderer;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;

namespace PrintRenderer
{

    /// <summary>
    /// Simple grid-based printer thingy.
    /// </summary>
    public partial class SimpleDocumentRenderer
    {
        /// <summary>
        /// The print document.
        /// </summary>
        public PrintDocument Document;

        /// <summary>
        /// Add a renderer. 
        /// </summary>
        /// <param name="renderer"></param>
        public void AddRow(RenderableElement renderer)
        {
            Renderers.Add(renderer);
        }

        /// <summary>
        /// Create a new SimpleGridPrinter.
        /// </summary>
        public SimpleDocumentRenderer()
        {
            _Init(null);
        }

        /// <summary>
        /// Create a SimpleGridPrinter using the indicated printer.
        /// </summary>
        /// <param name="printer_name">Name of the printer to use.</param>
        public SimpleDocumentRenderer(string printer_name)
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
    public partial class SimpleDocumentRenderer : VerticalLayoutRenderer
    {

        private void _Init(string printer_name)
        {
            Document = new PrintDocument
            {
                PrintController = new StandardPrintController()
            };
            Document.PrintPage += OnPagePrint;
            _ChoosePrinter(printer_name);
        }

        private void OnPagePrint(object sender, PrintPageEventArgs ev)
        {
            if (CurrentIndex >= Renderers.Count)
            {
                ev.HasMorePages = false;
                return;
            }
            Rectangle bbox = ev.MarginBounds;
            RenderResult result = new RenderResult();
            Render(ev.Graphics, ref bbox, ref result);

            // If rendering is incomplete, and the only thing rendered
            // on the page was the padding and margin, then rendering failed
            // and we have to bail. 
            if (result.Status == RenderStatus.Incomplete)
            {
                var current_renderer = Renderers[CurrentIndex];
                var h = current_renderer.Margin.VSize + current_renderer.Margin.HSize;
                if (result.RenderArea.Height == h)
                    throw new Exceptions.DoesNotFitOnPageException("Document rendering failed");
            }

            ev.HasMorePages = result.Status == RenderStatus.Incomplete;
        }

        //private void _CheckCanRenderOnPage(Graphics g, ref Rectangle page_area)
        //{
        //    RenderableElement r = Renderers[CurrentIndex];
        //    bool can_render = r.CanBeginRender(g, ref page_area);
        //    if (!can_render)
        //        throw new Exceptions.DoesNotFitOnPageException($"{r.ToString()}");
        //}

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
