using System;
using System.Collections.Generic;
using System.Drawing;


namespace PrintRenderer.TableRenderer
{

    /// <summary>
    /// Content alignment enum.
    /// </summary>
    public enum Alignment
    {
        /// <summary>
        /// Left aligned.
        /// </summary>
        Left,

        /// <summary>
        /// Right aligned.
        /// </summary>
        Right,

        /// <summary>
        /// Center aligned
        /// </summary>
        Center
    }

    /// <summary>
    /// Represents a single border line (top, left, right, etc).
    /// </summary>
    public struct BorderSegment
    {
        /// <summary>
        /// Brush to use. 
        /// </summary>
        public Brush Brush;

        /// <summary>
        /// Line thickness.
        /// </summary>
        public int Thickness;

        /// <summary>
        /// Whether to show the border. 
        /// </summary>
        public bool Show;
    }

    /// <summary>
    /// Represents the borders of a bounding box.
    /// </summary>
    public class Borders
    {
        /// <summary>
        /// Top border segment.
        /// </summary>
        public BorderSegment Top;

        /// <summary>
        /// Left border segment.
        /// </summary>
        public BorderSegment Left;

        /// <summary>
        /// Right border segment.
        /// </summary>
        public BorderSegment Right;

        /// <summary>
        /// Bottom border segment.
        /// </summary>
        public BorderSegment Bottom;
    }

    /// <summary>
    /// Dimensions of edges of a box, for margin or padding.
    /// </summary>
    public struct EdgeSizes
    {
        /// <summary>
        /// Top edge thickness.
        /// </summary>
        public int Top;

        /// <summary>
        /// Bottom edge thickness.
        /// </summary>
        public int Left;

        /// <summary>
        /// Right edge thickness. 
        /// </summary>
        public int Right;

        /// <summary>
        /// Bottom edge thickness.
        /// </summary>
        public int Bottom;
        
        /// <summary>
        /// Initialize a new EdgeSizes structure
        /// </summary>
        /// <param name="left">Left edge.</param>
        /// <param name="right">Right edge.</param>
        /// <param name="top">Top edge.</param>
        /// <param name="bottom">Bottom edge.</param>
        public EdgeSizes(int left=0, int right=0, int top=0, int bottom=0)
        {
            Top = top;
            Left = left;
            Right = right;
            Bottom = bottom;
        }
    }

    internal class InternalUtil
    {
        public static StringFormat StringFormat = StringFormat.GenericTypographic;
        public static Font DefaultFont = new Font("Consolas", 10);
        public static Brush DefaultBrush = Brushes.Black;

        /// <summary>
        /// Calculate the X coordinate to draw the provided line 
        /// based on the current TextPosition setting. 
        /// </summary>
        /// <param name="width">Line width.</param>
        /// <param name="bbox">Bounding box.</param>
        /// <param name="alignment">Text alignment.</param>
        /// <returns></returns>
        public static float CalcXPosition(float width, ref Rectangle bbox, Alignment alignment)
        {
            switch (alignment)
            {
                case Alignment.Left:
                    return bbox.Left;
                case Alignment.Right:
                    return bbox.Right - width;
                case Alignment.Center:
                    int middle = (bbox.Right + bbox.Left) / 2;
                    return middle - width / 2;
                default:
                    break;
            }
            // just in case anything changes
            throw new NotImplementedException($"TextPosition: {alignment.ToString()}");
        }

        public static void AdjustBBoxForPaddingAndMargin(ref Rectangle bbox, EdgeSizes margin, EdgeSizes padding, out int width, out int height)
        {
            // padding and margin are the same from a mathematical point of view
            // when adjusting the bbox.
            int left = margin.Left + padding.Left;
            int right = margin.Right + padding.Right;
            int top = margin.Top + padding.Top;
            int bottom = margin.Bottom + padding.Bottom;

            height = top + bottom;
            width = left + right;

            bbox.X += left;
            bbox.Width = bbox.Width - width;
            bbox.Y += top;
            bbox.Height = bbox.Height - height;
        }

        /// <summary>
        /// Adjusts the X and Width fields of the provided bbox
        /// to satisfy the provided alignment.
        /// </summary>
        /// <param name="bbox">Available bbox</param>
        /// <param name="w">Width of the container</param>
        /// <param name="alignment">Alignment</param>
        static public void AdjustBBoxForAlignment(ref Rectangle bbox, int w, Alignment alignment)
        {
            int diff;
            switch (alignment)
            {
                case Alignment.Left:
                    // default
                    break;
                case Alignment.Right:
                    // move everything to the right so the last cell
                    // is touching the BBox right edge. 
                    diff = bbox.Width - w;
                    bbox.X += diff;
                    bbox.Width -= diff;
                    break;
                case Alignment.Center:
                    diff = bbox.Width - w;
                    bbox.X += diff / 2;
                    bbox.Width -= diff / 2;
                    break;
            }
        }
    }

    /// <summary>
    /// Base class for the raw content.
    /// </summary>
    public class Content
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
        public string Text { get => Reader.Get(); set => Reader.Set(value); }

        /// <summary>
        /// Alignment of text (left, right, center).
        /// </summary>
        public Alignment Alignment;

        /// <summary>
        /// Represents the last render area. 
        /// </summary>
        public Rectangle LastRenderArea;

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
        public RenderResult Render(Graphics g, Rectangle bbox)
        {
            g.PageUnit = GraphicsUnit.Display;
            float char_width = _StringWidth(g, "a");
            float font_height = Font.GetHeight(g);
            int line_width = (int)(bbox.Width / char_width);
            return _InternalRender(g, ref bbox, char_width, font_height, line_width);
        }

        /// <summary>
        /// Indicates whether the text block can begin rendering in the provided 
        /// BBox.
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="bbox">Bounding box.</param>
        /// <returns>True if the cell can be partially rendered, else false.</returns>
        public bool CanBeginRender(Graphics g, ref Rectangle bbox)
        {
            //SizeF size = g.MeasureString("a", Font, 10000, _StringFormat);
            float min_height = Font.GetHeight(g);
            float min_width = _StringWidth(g, "a");
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



        private RenderResult _InternalRender(Graphics g, ref Rectangle bbox, float char_width, float font_height, int max_line)
        {
            float y = bbox.Y;
            int remaining = (int)(bbox.Height / font_height);

            string line;
            float x;
            float line_width;

            for (; remaining > 0 && !Reader.EOF; --remaining)
            {
                line = Reader.Read(max_line);
                line_width = _StringWidth(g, line);
                x = InternalUtil.CalcXPosition(line_width, ref bbox, Alignment);
                g.DrawString(line, Font, Brush, x, y, InternalUtil.StringFormat);
                y += font_height;
            }

            // calculate the rendered area
            LastRenderArea.X = bbox.X;
            LastRenderArea.Y = bbox.Y;
            LastRenderArea.Width = (int)Math.Ceiling(char_width * max_line);
            LastRenderArea.Height = (int)Math.Ceiling(y) - bbox.Y;

            return Reader.EOF ? RenderResult.Done : RenderResult.Incomplete;
        }
    }

    /// <summary>
    /// Basic cell
    /// </summary>
    public abstract class Cell : Renderer
    {
        /// <summary>
        /// Borders for this cell.
        /// </summary>
        public Borders Borders;

        /// <summary>
        /// Content Alignment.
        /// </summary>
        public virtual Alignment ContentAlignment { get; set; }

        /// <summary>
        /// Create a new cell.
        /// </summary>
        public Cell()
        {

        }

        /// <summary>
        /// Width of this cell.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Determine whether more content is available.
        /// </summary>
        public virtual bool MoreContentAvailable => throw new NotImplementedException();
    }

    /// <summary>
    /// Cell that holds text.
    /// </summary>
    public class TextCell : Cell
    {
        /// <summary>
        /// The actual text content object.
        /// </summary>
        public TextContent TextContent;

        /// <summary>
        /// Indicates whether more content is available.
        /// </summary>
        public override bool MoreContentAvailable => !TextContent.Complete;

        /// <summary>
        /// Sets the text content alignment
        /// </summary>
        public override Alignment ContentAlignment
        {
            get => TextContent.Alignment;
            set => TextContent.Alignment = value;
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
            TextContent = new TextContent(text, font, Brushes.Black, alignment);
            ContentAlignment = alignment;
            Width = width;
        }

        /// <summary>
        /// Set the text string.
        /// </summary>
        /// <param name="text">Text string.</param>
        public void SetText(string text)
        {
            TextContent.SetText(text);
        }

        /// <summary>
        /// Set the text string and alignment.
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="alignment">Text alignment</param>
        public void SetText(string text, Alignment alignment)
        {
            TextContent.SetText(text);
            ContentAlignment = alignment;
        }

        /// <summary>
        /// Set the text string and font. 
        /// </summary>
        /// <param name="text">Text string.</param>
        /// <param name="font">Font.</param>
        public void SetText(string text, Font font)
        {
            TextContent.SetText(text, font);
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
            return TextContent.CanBeginRender(g, ref bbox);
        }

        /// <summary>
        /// Render the cell in the provided bounding box.
        /// </summary>
        /// <param name="g">Graphics object.</param>
        /// <param name="bbox">Allowed bounding box.</param>
        /// <returns>Result of the render operation.</returns>
        protected internal override RenderResult Render(Graphics g, ref Rectangle bbox)
        {
            var allowed_bbox = bbox;
            allowed_bbox.Width = Width;
            RenderResult result = TextContent.Render(g, allowed_bbox);
            LastRenderArea = TextContent.LastRenderArea;
            return result;
        }
    }


    public partial class RowRenderer : Renderer
    {
        /// <summary>
        /// Collection of cells to render for this row
        /// </summary>
        public List<Cell> Cells;

        /// <summary>
        /// Content Alignment
        /// </summary>
        public Alignment Alignment;

        /// <summary>
        /// Create a new row.
        /// </summary>
        public RowRenderer()
        {
            Cells = new List<Cell>();
        }

        /// <summary>
        /// Get or set the Margin for this row.
        /// </summary>
        public EdgeSizes Margin;

        /// <summary>
        /// Get or set the row padding.
        /// </summary>
        public EdgeSizes Padding;

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
            TextCell r = new TextCell(text, font, alignment, width);
            AddCell(r);
            return r;
        }

        /// <summary>
        /// Indicate whether the row can be partially rendered. 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="bbox"></param>
        /// <returns></returns>
        public override bool CanBeginRender(Graphics g, ref Rectangle bbox)
        {
            Rectangle cell_bbox = bbox;
            Cell r;
            int n = Cells.Count;
            for (int i = 0; i < n; ++i)
            {
                r = Cells[i];
                cell_bbox.Width = r.Width;
                if (!r.CanBeginRender(g, ref cell_bbox))
                {
                    return false;
                }
                cell_bbox.X += r.Width;
            }
            return true;
        }

        /// <summary>
        /// Render the row.
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="bbox">Bounding box.</param>
        /// <returns>Result of the rendering operation.</returns>
        protected internal override RenderResult Render(Graphics g, ref Rectangle bbox)
        {
            Rectangle available_bbox = bbox;
            bool more = false;
            RenderResult result;

            InternalUtil.AdjustBBoxForPaddingAndMargin(ref available_bbox, Margin, Padding, out int width, out int height);
            InternalUtil.AdjustBBoxForAlignment(ref available_bbox, _GetWidth(), Alignment);

            Cell cell;
            int n = Cells.Count;

            for (int i = 0; i < n; ++i)
            {
                cell = Cells[i];
                // Only render if the cell needs to render more data.

                if (cell.MoreContentAvailable)
                {
                    result = cell.Render(g, ref available_bbox);
                }
                else
                {
                    result = RenderResult.Done;
                }

                // Move the available bbox over by the cell's width, 
                // which may be different than the last render area. 
                available_bbox.X += cell.Width;
                width += cell.Width;
                height = Math.Max(cell.LastRenderArea.Height, height);
                more = more || result == RenderResult.Incomplete;
            }

            LastRenderArea.X = bbox.X;
            LastRenderArea.Y = bbox.Y;
            LastRenderArea.Width = width;
            LastRenderArea.Height = height;

            return more ? RenderResult.Incomplete : RenderResult.Done;
        }
    }

    /// <summary>
    /// Private, protected, and internal methods.
    /// </summary>
    public partial class RowRenderer : Renderer
    {

        private protected int _GetWidth()
        {
            int w = 0;
            int n = Cells.Count;
            for (int i = 0; i < n; ++i)
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
    abstract public class IterRowRenderer : RowRenderer
    {
        /// <summary>
        /// Result of the last render operation.
        /// </summary>
        protected RenderResult LastResult;

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
        protected internal override RenderResult Render(Graphics g, ref Rectangle bbox)
        {
            var available_bbox = bbox;
            while (true)
            {
                if (LastResult == RenderResult.Done)
                {
                    if (!UpdateRow())
                        return RenderResult.Done;
                }

                // LastRenderArea set by base.Render()
                LastResult = base.Render(g, ref available_bbox);

                if (LastResult == RenderResult.Incomplete)
                {
                    return LastResult;
                }

                available_bbox.Y += LastRenderArea.Height;
                available_bbox.Height -= LastRenderArea.Height;
            }
        }

        /// <summary>
        /// Called to update the row object.
        /// </summary>
        /// <returns>True if the row was updated and data should be rendered. False if there is no more data to render.</returns>
        virtual public bool UpdateRow()
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
        public CallbackRowRenderer(Func<CallbackRowRenderer, bool> UpdateRow=null)
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
                return UpdateRowCallback(this);
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