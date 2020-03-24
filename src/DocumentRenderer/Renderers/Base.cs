using System;
using System.Collections.Generic;
using System.Drawing;
using System.Collections.ObjectModel;


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
    /// Result of a render operation.
    /// </summary>
    public class RenderResult
    {
        /// <summary>
        /// Result of the last render operation.
        /// </summary>
        public RenderStatus Status;

        /// <summary>
        /// BBox of the rendered area. 
        /// </summary>
        public Rectangle RenderArea;

        /// <summary>
        /// Creates a new, empty render result
        /// </summary>
        public RenderResult()
        {
            RenderArea = Rectangle.Empty;
            Status = RenderStatus.None;
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
    /// Interface for all renderers.
    /// </summary>
    public interface IRenderer
    {
        /// <summary>
        /// Perform the rendering operation. 
        /// </summary>
        /// <param name="g">Graphics object</param>
        /// <param name="bbox">BBox to render content in.</param>
        /// <param name="result">RenderResult object to hold last result.</param>
        /// <returns>Result of the rendering operation.</returns>
        void Render(Graphics g, ref Rectangle bbox, ref RenderResult result);

        ///// <summary>
        ///// Called when rendering begins.
        ///// </summary>
        //void BeginRendering();

        ///// <summary>
        ///// Called when rendering ends.
        ///// </summary>
        //void EndRendering();

        ///// <summary>
        ///// Indicates whether rendering operations are occurring. 
        ///// </summary>
        //bool IsRendering { get; }

        /// <summary>
        /// Indicates whether rendering can begin.
        /// </summary>
        /// <param name="g">Graphics object.</param>
        /// <param name="bbox">BBox to check.</param>
        /// <returns>True if the rendering operation can be at least partially completed.</returns>
        bool CanBeginRender(Graphics g, ref Rectangle bbox);
    }

    /// <summary>
    /// Represents a single border line (top, left, right, etc).
    /// </summary>
    public struct BorderSegment
    {
        /// <summary>
        /// Brush to use. 
        /// </summary>
        public Pen Pen;

        /// <summary>
        /// Line Width.
        /// </summary>
        public float Width;

        /// <summary>
        /// Whether to show the border. 
        /// </summary>
        public bool IsVisible;

        /// <summary>
        /// Create a new border segment
        /// </summary>
        /// <param name="b">Brush</param>
        /// <param name="w">Line width</param>
        /// <param name="show">Whether the border should be shown.</param>
        public BorderSegment(Pen b, float w, bool show)
        {
            Pen = b;
            Width = w;
            IsVisible = show;
        }

        /// <summary>
        /// Create a new null border segment.
        /// </summary>
        /// <returns></returns>
        public static BorderSegment None => new BorderSegment(Pens.Black, 0, false);

        /// <summary>
        /// Draws the border border according to the indicated line segment cordinates.
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="x1">First X coordinate</param>
        /// <param name="y1">First Y coordinate</param>
        /// <param name="x2">Second X coordinate</param>
        /// <param name="y2">Second Y coordinate</param>
        public void Draw(Graphics g, int x1, int y1, int x2, int y2)
        {
            if (IsVisible)
            {
                float w = Pen.Width;
                Pen.Width = Width;
                g.DrawLine(Pen, x1, y1, x2, y2);
                Pen.Width = w;
            }
        }
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
        public int Right => rect.X + rect.Width;

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
            Top = BorderSegment.None;
            Left = BorderSegment.None;
            Right = BorderSegment.None;
            Bottom = BorderSegment.None;
        }

        /// <summary>
        /// Draws the borders around the specified BBox.
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="bbox">Rectangle to draw borders on.</param>
        public void Draw(Graphics g, ref Rectangle bbox)
        {
            //   (x1,y1) ------ (x2, y1)
            //      |               |
            //      |               |
            //   (x1,y2) ------ (x2, y2)

            int x1 = bbox.X;
            int x2 = bbox.X + bbox.Width;
            int y1 = bbox.Y;
            int y2 = bbox.Y + bbox.Height;

            Top.Draw(g, x1, y1, x2, y1);
            Left.Draw(g, x1, y1, x1, y2);
            Right.Draw(g, x2, y1, x2, y2);
            Bottom.Draw(g, x1, y2, x2, y2);
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
        /// Total thickness of the side edges.
        /// </summary>
        public int HSize => Left + Right;

        /// <summary>
        /// Total thickness of the top and bottom edges.
        /// </summary>
        public int VSize => Top + Bottom;

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
    /// Misc helper methods
    /// </summary>
    internal static class RenderMethods
    {
        /// <summary>
        /// Adjustes the BBox for the given edge thickness (padding or margin). 
        /// </summary>
        /// <param name="bbox"></param>
        /// <param name="edges"></param>
        public static Rectangle AdjustedBBox(Rectangle bbox, EdgeSizes edges)
        {
            bbox.X += edges.Left;
            bbox.Y += edges.Top;
            bbox.Width -= edges.Left + edges.Right;
            bbox.Height -= edges.Top + edges.Bottom;
            return bbox;
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
        /// Create a new RenderableElement
        /// </summary>
        public RenderableElement()
        {
            Margin = new EdgeSizes();
            Padding = new EdgeSizes();
            Borders = new Borders();
            Alignment = Alignment.Left;
        }

        /// <summary>
        /// Throws NotImplementedException
        /// </summary>
        /// <param name="g"></param>
        /// <param name="bbox"></param>
        /// <returns></returns>
        abstract public bool CanBeginRender(Graphics g, ref Rectangle bbox);

        /// <summary>
        /// Throws NotImplementedException
        /// </summary>
        /// <param name="g"></param>
        /// <param name="bbox"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        abstract public void Render(Graphics g, ref Rectangle bbox, ref RenderResult result);
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
    abstract public class VerticalLayoutRenderer : ContainerRenderer
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
        /// <param name="result">Result of the last render operation</param>
        /// <returns></returns>
        public override void Render(Graphics g, ref Rectangle bbox, ref RenderResult result)
        {

            if (CurrentIndex >= Renderers.Count)
            {
                result.Status = RenderStatus.Done;
                result.RenderArea = Rectangle.Empty;
                return;
            }

            Rectangle border_bbox = RenderMethods.AdjustedBBox(bbox, Padding);
            Rectangle available_bbox = RenderMethods.AdjustedBBox(border_bbox, Margin);

            // loop forever until end-of-page or no more renderers available. 
            do
            {
                // If the previous call to Render() resulted in Incomplete,
                // then CurrentIndex will point to the correct renderer on
                // the first loop here.
                var r = Renderers[CurrentIndex];
                r.Render(g, ref available_bbox, ref result);
                available_bbox.Height -= result.RenderArea.Height;
                available_bbox.Y += result.RenderArea.Height;

                if (result.Status == RenderStatus.Incomplete)
                {
                    break;
                }

                CurrentIndex += 1;

            } while (CurrentIndex < Renderers.Count);

            Borders.Draw(g, ref border_bbox);

            // result.Status should match the last SubRenderer's status
            // i.e. we're incomplete if they were, otherwise
            // we're done too. 

            // result.RenderArea should have the original BBox's 
            // top left coordinates and width. The height is the
            // difference in original height with the remaining available height
            // plus the padding and margins widths removed earlier. 
            result.RenderArea.X = bbox.X;
            result.RenderArea.Y = bbox.Y;
            result.RenderArea.Width = bbox.Width;
            result.RenderArea.Height = bbox.Height - (available_bbox.Height + Padding.VSize + Margin.VSize);
        }
    }

    /// <summary>
    /// Lays out a collection of subrenderers horizontally, e.g. table row
    /// </summary>
    abstract public class HorizontalLayoutRenderer : ContainerRenderer
    {
        /// <summary>
        /// Create a new HorizontalLayoutRenderer.
        /// </summary>
        public HorizontalLayoutRenderer() : base()
        {
        }

        /// <summary>
        /// Result of the last child operation.
        /// </summary>
        protected RenderResult[] ChildResults;

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

                RenderStatus last_result;
                if (ChildResults == null)
                    last_result = RenderStatus.None;
                else
                    last_result = ChildResults[i].Status;

                if (last_result != RenderStatus.Done)
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
        /// Lazy initialization of the child results array. 
        /// Done here so subclasses and consumers can transparently
        /// add elements directly to the Renderers collection
        /// without needing to worry about updating the internal list.
        /// </summary>
        protected void CheckChildResultArray()
        {

            if (ChildResults == null)
            {
                ChildResults = new RenderResult[Renderers.Count];
                for (int i = 0; i < ChildResults.Length; ++i)
                {
                    ChildResults[i] = new RenderResult();
                } 
            }
        }

        /// <summary>
        /// Render
        /// </summary>
        /// <param name="g">Graphics</param>
        /// <param name="bbox">Bbox</param>
        /// <param name="result">Result of the render operation.</param>
        public override void Render(Graphics g, ref Rectangle bbox, ref RenderResult result)
        {
            if (Renderers.Count == 0)
            {
                result.Status = RenderStatus.Done;
                result.RenderArea = Rectangle.Empty;
                return;
            }
            CheckChildResultArray();
            RenderStatus overall_status = RenderStatus.Done;
            var border_bbox = RenderMethods.AdjustedBBox(bbox, Padding);
            var available_bbox = RenderMethods.AdjustedBBox(border_bbox, Margin);
            RenderMethods.AdjustBBoxForAlignment(ref available_bbox, Width, Alignment);

            RenderableElement element;
            RenderResult last_result;
            int n = Renderers.Count;

            int height = 0;

            for (int i = 0; i < n; ++i)
            {
                element = Renderers[i];
                last_result = ChildResults[i];

                if (last_result.Status != RenderStatus.Done)
                {
                    element.Render(g, ref available_bbox, ref last_result);
                    if (last_result.Status == RenderStatus.Incomplete)
                    {
                        overall_status = RenderStatus.Incomplete;
                    }

                    height = Math.Max(last_result.RenderArea.Height, height);
                }

                // Move the available bbox over by the element's width.
                // if the renderer misreports its with from the last
                // render operation, that's its fault, not ours. 
                available_bbox.X += last_result.RenderArea.Width;
            }

            // The horizontal layout renderer consumes its full width.
            // Status is determined by whether it contains *any* child 
            // elements that still need to be rendered, as some child
            // elements may break across a page while others might fit. 
            result.Status = overall_status;
            result.RenderArea.X = bbox.X;
            result.RenderArea.Y = bbox.Y;
            result.RenderArea.Width = bbox.Width;
            result.RenderArea.Height = height + Padding.VSize + Margin.VSize;
        }
    }
}
