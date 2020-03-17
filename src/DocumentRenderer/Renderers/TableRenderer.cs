using System;
using System.Collections.Generic;
using System.Drawing;

namespace PrintRenderer.TableRenderer
{
    public enum TextAlignments
    {
        Left,
        Right,
        Center
    }

    public interface ICellRenderer : IRenderer
    {
        int Width { get; set; }
        bool RenderComplete { get; }
    }

    public class CellTextRenderer : ICellRenderer
    {
        public Font Font;
        public Brush Brush;
        public TextAlignments TextAlignment;

        public bool RenderComplete => Reader.EOF;
        public Rectangle LastRenderArea => _LastRenderArea;
        public int Width { get => _Width; set => _Width = value; }

        private Rectangle _LastRenderArea;
        private int _Width = 0;
        private PrintStringReader Reader;
        private readonly StringFormat _StringFormat = StringFormat.GenericTypographic;

        public CellTextRenderer(string text, Font font = null,
                                TextAlignments alignment = TextAlignments.Left, int width = 10)
        {
            _Init(text, font ?? SystemFonts.DefaultFont, alignment, width);
        }

        private void _Init(string text, Font font, TextAlignments alignment, int width)
        {
            Font = font;
            Reader = new PrintStringReader(text);
            TextAlignment = alignment;
            Brush = Brushes.Black;
            Width = width;
        }

        public void SetText(string text)
        {
            Reader.Set(text);
        }

        public void SetText(string text, TextAlignments alignment)
        {
            Reader.Set(text);
            TextAlignment = alignment;
        }

        public void SetText(string text, Font font)
        {
            Reader.Set(text);
            Font = font;
        }

        public bool CanBeginRender(Graphics g, ref Rectangle bbox)
        {
            //SizeF size = g.MeasureString("a", Font, 10000, _StringFormat);
            float min_height = Font.GetHeight(g);
            float min_width = _StringWidth(g, "a");
            return (min_height <= bbox.Height) && (min_width <= bbox.Width);
        }

        private float _StringWidth(Graphics g, string text)
        {
            return g.MeasureString(text, Font, 10000, _StringFormat).Width;
        }

        public RenderResult Render(Graphics g, ref Rectangle bbox)
        {
            g.PageUnit = GraphicsUnit.Display;
            float char_width = _StringWidth(g, "a");
            float font_height = Font.GetHeight(g);
            int line_width = (int)(bbox.Width / char_width);
            return _InternalRender(g, ref bbox, char_width, font_height, line_width);
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
                x = _CalcTextPosition(line_width, bbox, TextAlignment);
                g.DrawString(line, Font, Brush, x, y, _StringFormat);
                y += font_height;
            }
            _LastRenderArea.X = bbox.X;
            _LastRenderArea.Y = bbox.Y;
            _LastRenderArea.Width = Width;
            _LastRenderArea.Height = (int)Math.Ceiling(y) - bbox.Y;
            return Reader.EOF ? RenderResult.Done : RenderResult.Incomplete;
        }

        /// <summary>
        /// Calculate the X coordinate to draw the provided line 
        /// based on the current TextPosition setting. 
        /// </summary>
        /// <param name="width">Line width</param>
        /// <returns></returns>
        private float _CalcTextPosition(float width, Rectangle bbox, TextAlignments alignment)
        {
            switch (alignment)
            {
                case TextAlignments.Left:
                    return bbox.Left;
                case TextAlignments.Right:
                    return bbox.Right - width;
                case TextAlignments.Center:
                    int middle = (bbox.Right + bbox.Left) / 2;
                    return middle - width / 2;
                default:
                    break;
            }
            throw new NotImplementedException($"TextPosition: {alignment.ToString()}");
        }
    }

    public class RowRenderer : IRenderer
    {
        public List<ICellRenderer> Cells;
        public Rectangle LastRenderArea => _LastRenderArea;

        protected Rectangle _LastRenderArea;

        public RowRenderer()
        {
            Cells = new List<ICellRenderer>();
        }

        public void AddCell(CellTextRenderer column)
        {
            Cells.Add(column);
        }

        public CellTextRenderer AddTextColumn(string text, Font font)
        {
            CellTextRenderer r = new CellTextRenderer(text, font);
            AddCell(r);
            return r;
        }

        public bool CanBeginRender(Graphics g, ref Rectangle bbox)
        {
            foreach (ICellRenderer r in Cells)
            {
                if (!r.CanBeginRender(g, ref bbox))
                {
                    return false;
                }
            }
            return true;
        }

        public virtual RenderResult Render(Graphics g, ref Rectangle bbox)
        {
            Rectangle available_bbox = bbox;

            bool more = false;
            RenderResult result;
            int height = 0;
            int width = 0;

            foreach (ICellRenderer r in Cells)
            {
                // Set available width to the Column's assigned width
                // before rendering. After rendering, move the X coordinate
                // of the available BBox by the consumed width. 

                // Only render if the cell needs to render more data.

                available_bbox.Width = r.Width;
                if (!r.RenderComplete)
                {
                    result = r.Render(g, ref available_bbox);
                }
                else
                {
                    result = RenderResult.Done;
                }
                available_bbox.X += r.Width;

                width += r.Width;
                height = Math.Max(r.LastRenderArea.Height, height);

                more = more || result == RenderResult.Incomplete;
            }

            _LastRenderArea.X = bbox.X;
            _LastRenderArea.Y = bbox.Y;
            _LastRenderArea.Width = width;
            _LastRenderArea.Height = height;

            return more ? RenderResult.Incomplete : RenderResult.Done;
        }
    }

    public class CallbackRowRenderer : RowRenderer
    {
        /// <summary>
        /// Function called with the current RowRender object
        /// to set the data for the next row to be rendered. 
        /// Must return `true` to continue rendering, or `false`
        /// if rendering is complete. 
        /// </summary>
        private Func<CallbackRowRenderer, bool> UpdateRow;
        private bool _GetMoreData;
        public CallbackRowRenderer(Func<CallbackRowRenderer, bool> UpdateRowCallback) : base()
        {
            UpdateRow = UpdateRowCallback;
            _GetMoreData = true;
        }

        public override RenderResult Render(Graphics g, ref Rectangle bbox)
        {
            RenderResult result;
            var available_bbox = bbox;

            while (true)
            {
                if (_GetMoreData)
                {
                    bool have_data = UpdateRow(this);
                    if (!have_data)
                    {
                        return RenderResult.Done;
                    }
                }
                result = base.Render(g, ref available_bbox);
                if (result == RenderResult.Incomplete)
                {
                    _GetMoreData = false;
                    return RenderResult.Incomplete;
                }
                else
                {
                    _GetMoreData = true;
                }
                available_bbox.Y += _LastRenderArea.Height;
                available_bbox.Height -= _LastRenderArea.Height;
            }
        }
    }
}
