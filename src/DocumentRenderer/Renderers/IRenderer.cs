using System;
using System.Collections.Generic;
using System.Drawing;

namespace PrintRenderer
{

    /// <summary>
    /// Indicates the result of a render operation.
    /// </summary>
    public enum RenderResult
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
        RenderResult Render(Graphics g, ref Rectangle bbox);

        /// <summary>
        /// Indicates whether rendering can begin.
        /// </summary>
        /// <param name="g">Graphics object.</param>
        /// <param name="bbox">BBox to check.</param>
        /// <returns>True if the rendering operation can be at least partially completed.</returns>
        bool CanBeginRender(Graphics g, ref Rectangle bbox);
    }

    /// <summary>
    /// Base renderer class.
    /// </summary>
    abstract public class Renderer
    {
        /// <summary>
        /// Area for the last render operation.
        /// </summary>
        public Rectangle LastRenderArea;

        /// <summary>
        /// The result of the last render operation.
        /// </summary>
        protected internal RenderResult LastResult;

        /// <summary>
        /// Creates a new renderer
        /// </summary>
        public Renderer()
        {
            LastResult = RenderResult.None;
        }

        /// <summary>
        /// Whether the renderer can begin.
        /// </summary>
        /// <param name="g">Graphics.</param>
        /// <param name="bbox">Bounding box to render in.</param>
        /// <returns>True if the render operation can be completed or partially completed.</returns>
        abstract public bool CanBeginRender(Graphics g, ref Rectangle bbox);

        /// <summary>
        /// Render any content and/or children. 
        /// </summary>
        /// <param name="g">Graphics object.</param>
        /// <param name="bbox">Bounding box to render inside.</param>
        /// <returns>Result of the render operation.</returns>
        abstract internal protected RenderResult Render(Graphics g, ref Rectangle bbox);
    }
}
//    /// <summary>
//    /// Represents a list of renderers
//    /// </summary>
//    public class RendererCollection<T> : List<T> where T: Renderer
//    {
//    }

//    /// <summary>
//    /// A renderer that contains a collection of subrenderers.
//    /// </summary>
//    public abstract class ContainerRenderer : Renderer
//    {
//        /// <summary>
//        /// The collection of renderers. 
//        /// </summary>
//        protected RendererCollection<Renderer> Renderers;

//        /// <summary>
//        /// Initialize the ContainerRenderer.
//        /// </summary>
//        protected ContainerRenderer() : base()
//        {
//            Renderers = new RendererCollection<Renderer>();
//        }
//    }

//    /// <summary>
//    /// A renderer that holds a collection 
//    /// </summary>
//    public class VerticalLayoutRenderer : ContainerRenderer
//    {

//        /// <summary>
//        /// 
//        /// </summary>
//        protected int CurrentIndex;

//        /// <summary>
//        /// Initialize the VerticalLayoutRenderer
//        /// </summary>
//        public VerticalLayoutRenderer() : base()
//        {
//            CurrentIndex = 0;
//        }

//        /// <summary>
//        /// For a vertical layout of objects, the render operation can be at 
//        /// least partially completed as long as the next object in the list can
//        /// be at least partially rendered, including through repeated incomplete
//        /// render operations. 
//        /// </summary>
//        /// <param name="g"></param>
//        /// <param name="bbox"></param>
//        /// <returns>True if the render operation can be at least partially completed.</returns>
//        public override bool CanBeginRender(Graphics g, ref Rectangle bbox)
//        {
//            Renderer r = Renderers[CurrentIndex];
//            return r.CanBeginRender(g, ref bbox);
//        }

//        /// <summary>
//        /// Render the collection of objects held by this layout renderer. 
//        /// Objects will be rendered vertically in the provided bounding box.
//        /// </summary>
//        /// <param name="g">Graphics object</param>
//        /// <param name="bbox">Bounding box</param>
//        /// <returns></returns>
//        internal protected override RenderResult Render(Graphics g, ref Rectangle bbox)
//        {
//            Renderer r;
//            RenderResult result;
//            Rectangle available_bbox = bbox;

//            // loop forever until end-of-page or no more renderers available. 
//            do
//            {
//                r = Renderers[CurrentIndex];
//                result = r.Render(g, ref available_bbox);
//                if (result == RenderResult.Incomplete)
//                {
//                    return RenderResult.Incomplete;
//                }

//                CurrentIndex += 1;

//                available_bbox.Height -= r.LastRenderArea.Height;
//                available_bbox.Y += r.LastRenderArea.Height;
//            } while (CurrentIndex < Renderers.Count);

//            return RenderResult.Done;
//        }
//    }

//    public class HasWidthRenderer : Renderer
//    {
//        public override bool CanBeginRender(Graphics g, ref Rectangle bbox)
//        {
//            throw new NotImplementedException();
//        }

//        protected internal override RenderResult Render(Graphics g, ref Rectangle bbox)
//        {
//            throw new NotImplementedException();
//        }
//        virtual public int Width { get; set; }
//    }

//    public class HorizontalLayoutRenderer : ContainerRenderer
//    {
//        public HorizontalLayoutRenderer()
//        {
//            //Renderers = new RendererCollection<HasWidthRenderer>();
//        }

//        public override bool CanBeginRender(Graphics g, ref Rectangle bbox)
//        {
//            var available_bbox = bbox;
//            for (int i = 0; i < Renderers.Count; ++i)
//            {
//                var r = Renderers[i];
//                available_bbox.Width = r.W
//                if (Renderers[i].LastResult != RenderResult.Done)
//                {
//                    if (!Renderers[i].CanBeginRender(g, ref available_bbox))
//                        return false;
//                }
//            }
//            return true;

//        }

//        protected internal override RenderResult Render(Graphics g, ref Rectangle bbox)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
