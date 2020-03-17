using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Runtime.Serialization;

namespace PrintRenderer
{
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
        /// Uh oh...
        /// </summary>
        Fail
    }

    public interface IRenderer
    {
        RenderResult Render(Graphics g, ref Rectangle bbox);
        bool CanBeginRender(Graphics g, ref Rectangle bbox);
        Rectangle LastRenderArea { get; }
    }
}
