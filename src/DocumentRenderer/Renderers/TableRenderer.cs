using System;
using System.Collections;
using System.Collections.Generic;
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
        /// Font for rendered text. Must be monospaced, or the rendering will be corrupted. 
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
            set => Reader.Set(value);
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
        /// <param name="result">Result of the render operation.</param>
        override public void Render(Graphics g, ref Rectangle bbox, ref RenderResult result)
        {
            g.PageUnit = GraphicsUnit.Display;
            var char_width = _StringWidth(g, "a");
            var font_height = Font.GetHeight(g);
            var line_width = (int)(bbox.Width / char_width);
            _InternalRender(g, ref bbox, char_width, font_height, line_width, ref result);
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
            SizeF size = g.MeasureString("a", Font, 10000, InternalUtil.StringFormat);
            //var min_height = Font.GetHeight(g);
            //var min_width = _StringWidth(g, "a");
            return (size.Height <= bbox.Height) && (size.Width <= bbox.Width);
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

        /// <summary>
        /// Calculate the X coordinate to draw the provided line 
        /// based on the current Alignment setting. 
        /// </summary>
        /// <param name="width">Line width.</param>
        /// <param name="bbox">Bounding box.</param>
        /// <param name="alignment">Text alignment.</param>
        /// <returns></returns>
        private float CalcXPosition(float width, ref Rectangle bbox, Alignment alignment)
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
            throw new NotImplementedException($"Alignment: {alignment.ToString()}");
        }

        private void _InternalRender(Graphics g, ref Rectangle bbox, float char_width, float font_height, int max_line, ref RenderResult result)
        {
            float y = bbox.Y;
            var remaining = (int)(bbox.Height / font_height);

            string line;
            float x;
            float line_width;
            for (; remaining > 0 && !Reader.EOF; --remaining)
            {
                line = Reader.Read(max_line);
                //line_width = _StringWidth(g, line);
                line_width = line.Length * char_width;  // only monospaced fonts supported!!
                x = CalcXPosition(line_width, ref bbox, Alignment);
                g.DrawString(line, Font, Brush, x, y, InternalUtil.StringFormat);
                y += font_height;
            }

            // calculate the rendered area
            result.Status = Reader.EOF ? RenderStatus.Done : RenderStatus.Incomplete;
            result.RenderArea.X = bbox.X;
            result.RenderArea.Y = bbox.Y;
            result.RenderArea.Width = (int)Math.Ceiling(char_width * max_line);
            result.RenderArea.Height = (int)Math.Ceiling(y) - bbox.Y;
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
        /// <param name="result">Result of the render operation.</param>
        public override void Render(Graphics g, ref Rectangle bbox, ref RenderResult result)
        {
            var original = bbox;
            original.Width = Width;
            var border_bbox = RenderMethods.AdjustedBBox(original, Padding);
            var allowed_bbox = RenderMethods.AdjustedBBox(border_bbox, Margin);
            Content.Render(g, ref allowed_bbox, ref result);

            Borders.Draw(g, ref border_bbox);

            // result.Status inherits the content's rendering result status.
            // bounding box is the original box with height shrunk to match
            // the height actually used. 
            result.RenderArea.X = original.X;
            result.RenderArea.Y = original.Y;
            result.RenderArea.Width = original.Width;
            result.RenderArea.Height += Padding.VSize + Margin.VSize;
        }
    }

    /// <summary>
    /// Represents a readonly collection of cells.
    /// </summary>
    public class CellsCollection : IReadOnlyList<Cell>
    {
        private RendererCollection Cells;

        /// <summary>
        /// Create a new cells collection.
        /// </summary>
        /// <param name="cells"></param>
        public CellsCollection(RendererCollection cells)
        {
            Cells = cells;
        }

        /// <summary>
        /// Gets the cell.
        /// </summary>
        /// <param name="index">Index</param>
        /// <returns>Cell</returns>
        public Cell this[int index] => Cells[index] as Cell;

        /// <summary>
        /// Gets the number of cells.
        /// </summary>
        public int Count => Cells.Count;

        /// <summary>
        /// Gets the enumerator. 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Cell> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return Cells[i] as Cell;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public partial class Row : HorizontalLayoutRenderer
    {

        /// <summary>
        /// Create a new row.
        /// </summary>
        public Row() : base()
        {
            Cells = new CellsCollection(Renderers);
        }

        /// <summary>
        /// Set the widths of cells in the row to the widths in the array. 
        /// Throws an ArgumentException if the length of the array does not
        /// match the number of cells contained. 
        /// </summary>
        /// <param name="args"></param>
        public void SetWidths(params int[] args)
        {
            if (args.Length != Renderers.Count)
                throw new ArgumentException($"Number of widths provided does not match number of cells. ({args.Length} != {Renderers.Count})");
            for (int i = 0; i < args.Length; ++i)
                Renderers[i].Width = args[i];
        }

        /// <summary>
        /// Returns a read-only view of the cells. Note cells may be modified individually. 
        /// </summary>
        public CellsCollection Cells { get; private set; }

        /// <summary>
        /// Returns the cell at the given index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Cell GetCell(int index)
        {
            return Renderers[index] as Cell;
        }

        /// <summary>
        /// Returns the number of cells in this row. 
        /// </summary>
        public int CellCount => Renderers.Count;

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
            Renderers.Add(cell);
        }

        /// <summary>
        /// Removes all cells from the row.
        /// </summary>
        public void ClearCells()
        {
            ChildResults = null;
            Renderers.Clear();
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
            var n = Renderers.Count;
            for (var i = 0; i < n; ++i)
            {
                w += Renderers[i].Width;
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
            LastStatus = RenderStatus.None;
        }

        /// <summary>
        /// Last render operation status. Needed for the
        /// iter row renderer to track whether or not to
        /// call its Update() method. 
        /// </summary>
        protected RenderStatus LastStatus;

        /// <summary>
        /// Render. 
        /// </summary>
        /// <param name="g">Graphics object.</param>
        /// <param name="bbox">Bbox.</param>
        /// <param name="result">Result of the render operation.</param>
        public override void Render(Graphics g, ref Rectangle bbox, ref RenderResult result)
        {
            // must be called before ResetCellResults(),
            // otherwise ChildResults will be null.
            CheckChildResultArray();

            var available_bbox = bbox;
            while (true)
            {
                if (LastStatus == RenderStatus.Done || LastStatus == RenderStatus.None)
                {
                    ResetCellResults();
                    if (!UpdateRow())
                    {
                        LastStatus = RenderStatus.Done;
                        break;
                    }
                }

                base.Render(g, ref available_bbox, ref result);

                if (result.Status == RenderStatus.Incomplete)
                {
                    break;
                }

                available_bbox.Y += result.RenderArea.Height;
                available_bbox.Height -= result.RenderArea.Height;
            }
            // result data is already set by base.Render()
        }

        /// <summary>
        /// Once a row has been fully rendered, the LastResult of each cell must
        /// be reset to None so that `base.Render()` knows that the cell needs to
        /// be rendered.
        /// </summary>
        private void ResetCellResults()
        {
            for (var i = 0; i < ChildResults.Length; ++i)
            {
                ChildResults[i].Status = RenderStatus.None;
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
}