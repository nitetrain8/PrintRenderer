using System;
using System.Drawing;

namespace PrintRenderer.TableRenderer
{


    public enum TextAlignments
    {
        Left,
        Right,
        Center
    }

    public class ColumnTextRenderer : IRenderer, IDisposable
    {
        public Font Font { get; set; }
        public PrintStringReader Reader;
        public bool MoreContentAvailable => !Reader.EOF;

        private int _Height;
        public int Height
        {
            get
            {
                if (_Height == -1)
                {
                    throw new RenderSizeUnavailableException("Column not yet rendered");
                }

                return _Height;
            }
            private set => _Height = value;
        }
        public int Width = 0;

        private readonly StringFormat _StringFormat = StringFormat.GenericTypographic;
        public Brush Brush;
        public TextAlignments TextAlignment;

        public ColumnTextRenderer(string text, Font font=null, 
                                TextAlignments alignment=TextAlignments.Left, int width=10)
        {
            _Init(text, font ?? SystemFonts.DefaultFont, alignment, width);
        }

        private void _Init(string text, Font font, TextAlignments alignment, int width)
        {
            Font = font;
            Reader = new PrintStringReader(text);
            TextAlignment = alignment;
            _Height = -1;
            Brush = Brushes.Black;
            Width = width;
        }

        public bool CanBeginRender(Graphics g, Rectangle bbox)
        {
            float min_height = Font.GetHeight(g);
            float min_width = _StringWidth(g, "a");
            return (min_height <= bbox.Height) && (min_width <= bbox.Width);
        }

        private float _StringWidth(Graphics g, string text)
        {
            return g.MeasureString(text, Font, 10000, _StringFormat).Width;
        }

        public void Render(Graphics g, Rectangle bbox)
        {
            g.PageUnit = GraphicsUnit.Display;
            float char_width = _StringWidth(g, "a");
            float font_height = Font.GetHeight(g);
            int line_width = (int)(bbox.Width / char_width);
            if (line_width <= 0 || font_height > bbox.Height)
            {
                throw new PrintRendererException("BBox too small: can't print characters");
            }
            _InternalRender(g, bbox, char_width, font_height, line_width);
        }

        private void _InternalRender(Graphics g, Rectangle bbox, float char_width, float font_height, int max_line)
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

            Height = (int)Math.Ceiling(y - bbox.Y);
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
                    int middle = (bbox.Right - bbox.Left) / 2;
                    return middle - width / 2;
                default:
                    break;
            }
            throw new NotImplementedException($"TextPosition: {alignment.ToString()}");
        }
        #region IDisposable Support
        private bool _disposed = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                }
                _disposed = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    public class RowRenderer : IRenderer
    {
        public bool MoreContentAvailable
        {
            get
            {
                foreach (var r in Columns.GetAll())
                {
                    if (r.MoreContentAvailable)
                        return true;
                }
                return false;
            }
        }

        public int Height
        {
            get
            {
                int mh = 0;
                foreach (IRenderer r in Columns.GetAll())
                {
                    if (r.Height > mh)
                    {
                        mh = r.Height;
                    }
                }

                return mh;
            }
        }

        public RendererStack<ColumnTextRenderer> Columns;

        public RowRenderer()
        {
            Columns = new RendererStack<ColumnTextRenderer>();
        }

        public void AddColumn(ColumnTextRenderer column)
        {
            Columns.Add(column);
        }

        public ColumnTextRenderer AddTextColumn(string text, Font font)
        {
            ColumnTextRenderer r = new ColumnTextRenderer(text, font);
            Columns.Add(r);
            return r;
        }

        public bool CanBeginRender(Graphics g, Rectangle bbox)
        {
            foreach (IRenderer r in Columns.GetRemaining())
            {
                if (r.CanBeginRender(g, bbox))
                {
                    return true;
                }
            }
            return false;
        }

        public void Render(Graphics g, Rectangle bbox)
        {
            var used_bbox = new Rectangle(bbox.X, bbox.Y, bbox.Width, bbox.Height);
            foreach (ColumnTextRenderer r in Columns.GetAll())
            {
                r.Render(g, used_bbox);
                used_bbox.X += r.Width;
                used_bbox.Width -= r.Width;
            }
        }
    }
}
