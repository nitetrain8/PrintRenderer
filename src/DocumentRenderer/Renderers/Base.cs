using System;
using System.Collections.Generic;
using System.Drawing;

namespace PrintRenderer
{

    /// <summary>
    /// Indicates the result of a render operation.
    /// </summary>
    public enum RenderStatus
    {
        /// <summary>
        /// Render successful, rendering complete.
        /// </summary>
        Done,

        /// <summary>
        /// Render successful but out of room in provided 
        /// BBox. Call Render() again to add remaining content 
        /// to a new BBox. 
        /// </summary>
        Incomplete,

        /// <summary>
        /// No render operation has occurred. 
        /// </summary>
        None
    }

    /// <summary>
    /// Interface for all renderers.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// Perform the rendering operation. 
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="bbox">BBox to render content in.</param>
        /// <returns>Result of the rendering operation.</returns>
        RenderStatus Render(Graphics g, ref Rectangle bbox);

        /// <summary>
        /// Indicates whether rendering can begin.
        /// </summary>
        /// <param name="g">Graphics object.</param>
        /// <param name="bbox">BBox to check.</param>
        /// <returns>True if the rendering operation can be at least partially completed.</returns>
        bool CanBeginRender(Graphics g, ref Rectangle bbox);

        /// <summary>
        /// Area for the last render operation.
        /// </summary>
        RenderArea LastRenderArea { get; }

        /// <summary>
        /// The result of the last render operation.
        /// </summary>
        RenderStatus LastResult { get; }
    }

    /// <summary>
    /// A class version of Rectangle, because of reasons. 
    /// </summary>
    public class RenderArea
    {
        private Rectangle rect;

        /// <summary>
        /// Rectangle.X (X coordinate of top left corner)
        /// </summary>
        public int X
        {
            get => rect.X;
            set => rect.X = value;
        }

        /// <summary>
        /// Rectangle.Y
        /// </summary>
        public int Y
        {
            get => rect.Y;
            set => rect.Y = value;
        }

        /// <summary>
        /// X coordinate of right edge of rectangle.
        /// </summary>
        public int Right
        {
            get => rect.X + rect.Width;
        }

        /// <summary>
        /// Rectangle.Width;
        /// </summary>
        public int Width
        {
            get => rect.Width;
            set => rect.Width = value;
        }

        /// <summary>
        /// Rectangle.Height
        /// </summary>
        public int Height
        {
            get => rect.Height;
            set => rect.Height = value;
        }
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
        /// Line Width.
        /// </summary>
        public int Width;

        /// <summary>
        /// Whether to show the border. 
        /// </summary>
        public bool Show;
        
        /// <summary>
        /// Create a new border segment
        /// </summary>
        /// <param name="b">Brush</param>
        /// <param name="w">Line width</param>
        /// <param name="show">Whether the border should be shown.</param>
        public BorderSegment(Brush b, int w, bool show)
        {
            Brush = b;
            Width = w;
            Show = show;
        }

        /// <summary>
        /// Create a new null border segment.
        /// </summary>
        /// <returns></returns>
        public static BorderSegment None()
        {
            return new BorderSegment(Brushes.Black, 0, false);
        }
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

        /// <summary>
        /// New borders object. 
        /// </summary>
        public Borders()
        {
            Top = BorderSegment.None();
            Left = BorderSegment.None();
            Right = BorderSegment.None();
            Bottom = BorderSegment.None();
        }
    }

    /// <summary>
    /// Dimensions of edges of a box, for margin or padding.
    /// </summary>
    public class EdgeSizes
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
        public EdgeSizes(int left = 0, int right = 0, int top = 0, int bottom = 0)
        {
            Top = top;
            Left = left;
            Right = right;
            Bottom = bottom;
        }
    }

    /// <summary>
    /// Base element class.
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// Margins around the outside of this element.
        /// </summary>
        EdgeSizes Margin { get; }

        /// <summary>
        /// Internal padding for this element.
        /// </summary>
        EdgeSizes Padding { get; }

        /// <summary>
        /// Borders for this element.
        /// </summary>
        Borders Borders { get; }

        /// <summary>
        /// Get or set the content alignment.
        /// </summary>
        Alignment Alignment { get; set; }

        /// <summary>
        /// Width of this element, if it can be determined. Otherwise, 0.
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// Height of this element, if it can be determined. Otherwise, 0.
        /// </summary>
        int Height { get; set; }
    }

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
    /// Misc helper methods
    /// </summary>
    internal static class RenderMethods
    {
        /// <summary>
        /// Calculate the X coordinate to draw the provided line 
        /// based on the current Alignment setting. 
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
            throw new NotImplementedException($"Alignment: {alignment.ToString()}");
        }

        /// <summary>
        /// Adjusts the bbox for the provided margin and padding. 
        /// </summary>
        /// <param name="bbox">Bbox to be modified.</param>
        /// <param name="margin">Margins</param>
        /// <param name="padding">Padding</param>
        /// <param name="width">Total width consumed by margin and padding.</param>
        /// <param name="height">Total height consumed by margin and padding.</param>
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
    /// An element with built in render operations.
    /// </summary>
    public abstract class RenderableElement : IElement, IRenderer
    {
        
        /// <summary>
        /// Create a new RenderableElement
        /// </summary>
        public RenderableElement()
        {
            LastRenderArea = new RenderArea();
            LastResult = RenderStatus.None;
            Margin = new EdgeSizes();
            Padding = new EdgeSizes();
            Borders = new Borders();
            Alignment = Alignment.Left;
        }
        
        /// <summary>
        /// BBox of the last successful render operation.
        /// </summary>
        public RenderArea LastRenderArea { get; protected internal set; }

        /// <summary>
        /// Result of the last render operation.
        /// </summary>
        public RenderStatus LastResult { get; protected internal set; }

        /// <summary>
        /// Width of this element.
        /// </summary>
        virtual public int Width { get; set; }

        /// <summary>
        /// Height of this element
        /// </summary>
        virtual public int Height { get; set; }

        /// <summary>
        /// Margin for this element.
        /// </summary>
        public EdgeSizes Margin { get; set; }

        /// <summary>
        /// Internal padding for this element.
        /// </summary>
        public EdgeSizes Padding { get; set; }

        /// <summary>
        /// Borders for this element
        /// </summary>
        public Borders Borders { get; set; }

        /// <summary>
        /// Get or set the content alignment
        /// </summary>
        public Alignment Alignment { get; set; }

        /// <summary>
        /// Throws NotImplementedException
        /// </summary>
        /// <param name="g"></param>
        /// <param name="bbox"></param>
        /// <returns></returns>
        virtual public bool CanBeginRender(Graphics g, ref Rectangle bbox)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Throws NotImplementedException
        /// </summary>
        /// <param name="g"></param>
        /// <param name="bbox"></param>
        /// <returns></returns>
        virtual public RenderStatus Render(Graphics g, ref Rectangle bbox)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents a list of renderers
    /// </summary>
    public class RendererCollection : List<RenderableElement>
    {
    }

    /// <summary>
    /// A renderer that contains a collection of subrenderers.
    /// </summary>
    public abstract class ContainerRenderer : RenderableElement
    {
        /// <summary>
        /// The collection of renderers. 
        /// </summary>
        protected RendererCollection Renderers { get; set; }

        /// <summary>
        /// Create a new COntainerRenderer
        /// </summary>
        public ContainerRenderer() : base()
        {
            Renderers = new RendererCollection();
        }
    }

    /// <summary>
    /// A renderer that holds a collection 
    /// </summary>
    public class VerticalLayoutRenderer : ContainerRenderer
    {

        /// <summary>
        /// Current renderer index. The index advances when a 
        /// subrenderer is fully rendered. 
        /// </summary>
        protected int CurrentIndex;

        /// <summary>
        /// Initialize the VerticalLayoutRenderer
        /// </summary>
        public VerticalLayoutRenderer() : base()
        {
            CurrentIndex = 0;
        }

        /// <summary>
        /// For a vertical layout of objects, the render operation can be at 
        /// least partially completed as long as the next object in the list can
        /// be at least partially rendered, including through repeated incomplete
        /// render operations. 
        /// </summary>
        /// <param name="g"></param>
        /// <param name="bbox"></param>
        /// <returns>True if the render operation can be at least partially completed.</returns>
        public override bool CanBeginRender(Graphics g, ref Rectangle bbox)
        {
            RenderableElement r = Renderers[CurrentIndex];
            return r.CanBeginRender(g, ref bbox);
        }

        /// <summary>
        /// Render the collection of objects held by this layout renderer. 
        /// Objects will be rendered vertically in the provided bounding box.
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="bbox">Bounding box</param>
        /// <returns></returns>
        public override RenderStatus Render(Graphics g, ref Rectangle bbox)
        {
            RenderableElement r;
            RenderStatus result;
            Rectangle available_bbox = bbox;

            // loop forever until end-of-page or no more renderers available. 
            do
            {
                r = Renderers[CurrentIndex];
                result = r.Render(g, ref available_bbox);
                if (result == RenderStatus.Incomplete)
                {
                    return RenderStatus.Incomplete;
                }

                CurrentIndex += 1;

                available_bbox.Height -= r.LastRenderArea.Height;
                available_bbox.Y += r.LastRenderArea.Height;
            } while (CurrentIndex < Renderers.Count);

            return RenderStatus.Done;
        }
    }

    /// <summary>
    /// Lays out a collection of subrenderers horizontally, e.g. table row
    /// </summary>
    public class HorizontalLayoutRenderer : ContainerRenderer
    {
        /// <summary>
        /// Create a new HorizontalLayoutRenderer.
        /// </summary>
        public HorizontalLayoutRenderer() : base()
        {
        }

        /// <summary>
        /// True if any of the not-yet-fully-rendered subelements can begin rendering.
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="bbox">Bbox</param>
        /// <returns>True if rendering can begin.</returns>
        public override bool CanBeginRender(Graphics g, ref Rectangle bbox)
        {
            var available_bbox = bbox;
            for (int i = 0; i < Renderers.Count; ++i)
            {
                var r = Renderers[i];
                available_bbox.Width = r.Width;
                if (r.LastResult != RenderStatus.Done)
                {
                    if (!r.CanBeginRender(g, ref available_bbox))
                        return false;
                }
                available_bbox.X += r.Width;
                available_bbox.Width -= r.Width;
            }
            return true;
        }

        /// <summary>
        /// Render
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="bbox">Bbox</param>
        /// <returns>Result of the render operation.</returns>
        public override RenderStatus Render(Graphics g, ref Rectangle bbox)
        {
            Rectangle available_bbox = bbox;
            bool more = false;
            RenderStatus result;

            RenderMethods.AdjustBBoxForPaddingAndMargin(ref available_bbox, Margin, Padding, out int width, out int height);
            RenderMethods.AdjustBBoxForAlignment(ref available_bbox, Width, Alignment);

            RenderableElement element;
            int n = Renderers.Count;

            for (int i = 0; i < n; ++i)
            {
                element = Renderers[i];
                // Only render if the cell needs to render more data.

                if (element.LastResult != RenderStatus.Done)
                {
                    result = element.Render(g, ref available_bbox);
                    height = Math.Max(element.LastRenderArea.Height, height);
                }
                else
                {
                    result = RenderStatus.Done;
                }

                // Move the available bbox over by the cell's width, 
                // which may be different than the last render area. 
                available_bbox.X += element.Width;
                width += element.Width;
                more = more || result == RenderStatus.Incomplete;
            }

            LastRenderArea.X = bbox.X;
            LastRenderArea.Y = bbox.Y;
            LastRenderArea.Width = width;
            LastRenderArea.Height = height;
            LastResult = more ? RenderStatus.Incomplete : RenderStatus.Done;
            return LastResult;
        }
    }
}
