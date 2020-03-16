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
    public class PrintRendererException : Exception
    {
        public PrintRendererException(string message) : base(message)
        {
        }

        public PrintRendererException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PrintRendererException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    public class PrintSizeException : PrintRendererException
    {
        public PrintSizeException(string message) : base(message)
        {
        }
    }

    public class RenderSizeUnavailableException : PrintRendererException
    {
        public RenderSizeUnavailableException(string message) : base(message)
        {
        }
    }

    public interface IRenderer
    {
        void Render(Graphics g, Rectangle bbox);
        bool MoreContentAvailable { get; }
        bool CanBeginRender(Graphics g, Rectangle bbox);
        int Height { get; }
    }
}
