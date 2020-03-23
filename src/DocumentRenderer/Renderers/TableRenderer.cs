using System;
using System.Drawing;


namespace PrintRenderer.TableRenderer
{

    internal class InternalUtil
    {
        public static StringFormat StringFormat = StringFormat.GenericTypographic;
        public static Font DefaultFont = new Font("Consolas", 10);
        public static Brush DefaultBrush = Brushes.Black;

    }

    /// <summary>
    /// Base class for the raw content.
    /// </summary>
    abstract public class Content : RenderableElement
    {
    }

    /// <summary>
    /// Text content container. 
    /// </summary>
    public partial class TextContent : Content
    {
        /// <summary>
        /// Font for rendered text.
        /// </summary>
        public Font Font;

        /// <summary>
        /// Brush for text (text color).
        /// </summary>
        public Brush Brush;

        /// <summary>
        /// Get or set the text rendered.
        /// </summary>
        public string Text
        {
            get => Reader.Get();
            set
            {
                LastResult = RenderStatus.None;
                Reader.Set(value);
            }
        }

        /// <summary>
        /// Create a new TextContent with the given text and font.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="font">Font to use.</param>
        /// <param name="brush">Brush</param>
        /// <param name="alignment">Text alignment</param>
        public TextContent(string text = "", Font font = null, Brush brush = null, Alignment alignment = Alignment.Left)
        {
            Font = font ?? InternalUtil.DefaultFont;
            Brush = brush ?? InternalUtil.DefaultBrush;
            Reader = new PrintStringReader(text);
            Alignment = alignment;
        }

        /// <summary>
        /// Sent the text and/or font for this TextContent.
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="font">Font</param>
        public void SetText(string text = "", Font font = null)
        {
            Reader.Set(text);
            if (font != null)
            {
                Font = font;
            }
        }

        /// <summary>
        /// Render the text in the given bbox.
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="bbox">Bounding box.</param>
        /// <returns>Result of the render operation.</returns>
        override public RenderStatus Render(Graphics g, ref Rectangle bbox)
        {
            g.PageUnit = GraphicsUnit.Display;
            var char_width = _StringWidth(g, "a");
            var font_height = Font.GetHeight(g);
            var line_width = (int)(bbox.Width / char_width);
            return _InternalRender(g, ref bbox, char_width, font_height, line_width);
        }

        /// <summary>
        /// Indicates whether the text block can begin rendering in the provided 
        /// BBox.
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="bbox">Bounding box.</param>
        /// <returns>True if the cell can be partially rendered, else false.</returns>
        override public bool CanBeginRender(Graphics g, ref Rectangle bbox)
        {
            //SizeF size = g.MeasureString("a", Font, 10000, _StringFormat);
            var min_height = Font.GetHeight(g);
            var min_width = _StringWidth(g, "a");
            return (min_height <= bbox.Height) && (min_width <= bbox.Width);
        }
    }

    public partial class TextContent : Content
    {
        private PrintStringReader Reader;

        /// <summary>
        /// Whether text content has been fully rendered.
        /// </summary>
        protected internal bool Complete => Reader.EOF;

        private float _StringWidth(Graphics g, string text)
        {
            return g.MeasureString(text, Font, 10000, InternalUtil.StringFormat).Width;
        }

        private RenderStatus _InternalRender(Graphics g, ref Rectangle bbox, float char_width, float font_height, int max_line)
        {
            float y = bbox.Y;
            var remaining = (int)(bbox.Height / font_height);

            string line;
            float x;
            float line_width;

            for (; remaining > 0 && !Reader.EOF; --remaining)
            {
                line = Reader.Read(max_line);
                line_width = _StringWidth(g, line);
                x = RenderMethods.CalcXPosition(line_width, ref bbox, Alignment);
                g.DrawString(line, Font, Brush, x, y, InternalUtil.StringFormat);
                y += font_height;
            }

            // calculate the rendered area
            LastRenderArea.X = bbox.X;
            LastRenderArea.Y = bbox.Y;
            LastRenderArea.Width = (int)Math.Ceiling(char_width * max_line);
            LastRenderArea.Height = (int)Math.Ceiling(y) - bbox.Y;
            LastResult = Reader.EOF ? RenderStatus.Done : RenderStatus.Incomplete;
            return LastResult;
        }
    }

    /// <summary>
    /// Basic cell
    /// </summary>
    public abstract class Cell : RenderableElement
    {
        /// <summary>
        /// The content for this Cell
        /// </summary>
        public Content Content { get; set; }

        /// <summary>
        /// Create a new cell.
        /// </summary>
        public Cell()
        { }
    }

    /// <summary>
    /// Cell that holds text.
    /// </summary>
    public class TextCell : Cell
    {

        /// <summary>
        /// Sets the text content alignment
        /// </summary>
        public Alignment ContentAlignment
        {
            get => Content.Alignment;
            set => Content.Alignment = value;
        }

        /// <summary>
        /// Create a new Cell that holds text content. 
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="font">Font</param>
        /// <param name="alignment">How to align text (left, right, center).</param>
        /// <param name="width">Cell width.</param>
        public TextCell(string text, Font font = null,
                                Alignment alignment = Alignment.Left, int width = 10)
        {
            _Init(text, font ?? SystemFonts.DefaultFont, alignment, width);
        }

        private void _Init(string text, Font font, Alignment alignment, int width)
        {
            Content = new TextContent(text, font, Brushes.Black, alignment);
            ContentAlignment = alignment;
            Width = width;
        }

        /// <summary>
        /// Set the text string.
        /// </summary>
        /// <param name="text">Text string.</param>
        public void SetText(string text)
        {
            (Content as TextContent).SetText(text);
        }

        /// <summary>
        /// Set the text string and alignment.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="alignment">Text alignment</param>
        public void SetText(string text, Alignment alignment)
        {
            (Content as TextContent).SetText(text);
            ContentAlignment = alignment;
        }

        /// <summary>
        /// Set the text string and font. 
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="font">Font.</param>
        public void SetText(string text, Font font)
        {
            (Content as TextContent).SetText(text, font);
        }

        /// <summary>
        /// Indicates whether the current cell can begin rendering in the provided 
        /// BBox.
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="bbox">Bounding box.</param>
        /// <returns>True if the cell can be partially rendered, else false.</returns>
        public override bool CanBeginRender(Graphics g, ref Rectangle bbox)
        {
            return Content.CanBeginRender(g, ref bbox);
        }

        /// <summary>
        /// Render the cell in the provided bounding box.
        /// </summary>
        /// <param name="g">Graphics object.</param>
        /// <param name="bbox">Allowed bounding box.</param>
        /// <returns>Result of the render operation.</returns>
        public override RenderStatus Render(Graphics g, ref Rectangle bbox)
        {
            var allowed_bbox = bbox;
            allowed_bbox.Width = Width;
            LastResult = Content.Render(g, ref allowed_bbox);
            LastRenderArea = Content.LastRenderArea;
            return LastResult;
        }
    }

    public partial class Row : HorizontalLayoutRenderer
    {
        /// <summary>
        /// Collection of cells to render for this row
        /// </summary>
        public RendererCollection Cells => Renderers;

        /// <summary>
        /// Create a new row.
        /// </summary>
        public Row() : base()
        {
        }

        /// <summary>
        /// Get the width of this row. 
        /// </summary>
        public override int Width
        {
            get => _GetWidth();
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Add a cell.
        /// </summary>
        /// <param name="cell">cell to add.</param>
        public void AddCell(Cell cell)
        {
            Cells.Add(cell);
        }

        /// <summary>
        /// Add a TextCell in place.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="font"></param>
        /// <param name="alignment"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public TextCell AddTextCell(string text, Font font, Alignment alignment = Alignment.Left, int width = 10)
        {
            var r = new TextCell(text, font, alignment, width);
            AddCell(r);
            return r;
        }
    }

    /// <summary>
    /// Private, protected, and internal methods.
    /// </summary>
    public partial class Row : HorizontalLayoutRenderer
    {

        private protected int _GetWidth()
        {
            var w = 0;
            var n = Cells.Count;
            for (var i = 0; i < n; ++i)
            {
                w += Cells[i].Width;
            }

            return w;
        }
    }

    /// <summary>
    /// A row that uses callbacks, virtual members, or ienumerators to 
    /// continuously render data. 
    /// </summary>
    public abstract class IterRowRenderer : Row
    {

        /// <summary>
        /// New Row Renderer
        /// </summary>
        protected IterRowRenderer() : base()
        {
        }

        /// <summary>
        /// Render. 
        /// </summary>
        /// <param name="g">Graphics object.</param>
        /// <param name="bbox">Bbox.</param>
        /// <returns>Result of the render operation.</returns>
        public override RenderStatus Render(Graphics g, ref Rectangle bbox)
        {
            var available_bbox = bbox;
            while (true)
            {
                if (LastResult == RenderStatus.Done || LastResult == RenderStatus.None)
                {
                    ResetCellResults();
                    if (!UpdateRow())
                    {
                        LastResult = RenderStatus.Done;
                        return RenderStatus.Done;
                    }
                }

                // LastRenderArea set by base.Render()
                LastResult = base.Render(g, ref available_bbox);

                if (LastResult == RenderStatus.Incomplete)
                {
                    return LastResult;
                }

                available_bbox.Y += LastRenderArea.Height;
                available_bbox.Height -= LastRenderArea.Height;
            }
        }

        /// <summary>
        /// Once a row has been fully rendered, the LastResult of each cell must
        /// be reset to None so that `base.Render()` knows that the cell needs to
        /// be rendered.
        /// </summary>
        private void ResetCellResults()
        {
            for (var i = 0; i < Renderers.Count; ++i)
            {
                Renderers[i].LastResult = RenderStatus.None;
            }
        }

        /// <summary>
        /// Called to update the row object.
        /// </summary>
        /// <returns>True if the row was updated and data should be rendered. False if there is no more data to render.</returns>
        public virtual bool UpdateRow()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Uses a callback to update the row. 
    /// </summary>
    public class CallbackRowRenderer : IterRowRenderer
    {
        /// <summary>
        /// The update row callback;
        /// </summary>
        public Func<CallbackRowRenderer, bool> UpdateRowCallback { get; set; }

        /// <summary>
        /// Create a new CallbackRowRenderer
        /// </summary>
        /// <param name="UpdateRow">callback</param>
        public CallbackRowRenderer(Func<CallbackRowRenderer, bool> UpdateRow = null)
        {
            UpdateRowCallback = UpdateRow;
        }

        /// <summary>
        /// Updates the row by calling the user-provided callback.
        /// </summary>
        /// <returns>Result of the update operation.</returns>
        public override bool UpdateRow()
        {
            if (UpdateRowCallback != null)
            {
                return UpdateRowCallback(this);
            }

            return false;
        }
    }

    ///// <summary>
    ///// Uses an Action(T) to update the row object until there is no more data left. 
    ///// </summary>
    //public class CallbackRowRenderer : RowRenderer
    //{
    //    /// <summary>
    //    /// Function called with the current RowRender object
    //    /// to set the data for the next row to be rendered. 
    //    /// Must return `true` to continue rendering, or `false`
    //    /// if rendering is complete. 
    //    /// </summary>
    //    private Func<CallbackRowRenderer, bool> UpdateRow;
    //    private bool _GetMoreData;

    //    /// <summary>
    //    /// Create a new row.
    //    /// </summary>
    //    /// <param name="UpdateRowCallback">Update row callback.</param>
    //    public CallbackRowRenderer(Func<CallbackRowRenderer, bool> UpdateRowCallback) : base()
    //    {
    //        UpdateRow = UpdateRowCallback;
    //        _GetMoreData = true;
    //    }

    //    /// <summary>
    //    /// Render.
    //    /// </summary>
    //    /// <param name="g">Graphics object.</param>
    //    /// <param name="bbox">Bounding box.</param>
    //    /// <returns>Result of the rendering operation.</returns>
    //    public override RenderResult Render(Graphics g, ref Rectangle bbox)
    //    {
    //        RenderResult result;
    //        Rectangle available_bbox = bbox;

    //        while (true)
    //        {
    //            if (_GetMoreData)
    //            {
    //                // currently results in an extra page being printed if the 
    //                // callback renderer is the last renderer, and the last row 
    //                // is the last to fit on a page. 
    //                bool have_data = UpdateRow(this);
    //                if (!have_data)
    //                {
    //                    return RenderResult.Done;
    //                }
    //            }

    //            // LastRenderArea set by base.Render()
    //            result = base.Render(g, ref available_bbox);

    //            if (result == RenderResult.Incomplete)
    //            {
    //                _GetMoreData = false;
    //                return RenderResult.Incomplete;
    //            }
    //            else
    //            {
    //                _GetMoreData = true;
    //            }
    //            available_bbox.Y += LastRenderArea.Height;
    //            available_bbox.Height -= LastRenderArea.Height;
    //        }
    //    }
    //}
}