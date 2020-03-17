using System;

namespace PrintRenderer.Exceptions
{
    /// <summary>
    /// Base exception for print renderer exceptions. 
    /// </summary>
    public class PrintRendererException : Exception
    {
        public PrintRendererException(string message) : base(message) { }
    }

    /// <summary>
    /// Base exception for render failures
    /// </summary>
    public class RenderFailedException : PrintRendererException
    {
        public RenderFailedException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// Unable to render element: size exceeds maximum page bounds. 
    /// </summary>
    public class DoesNotFitOnPageException : RenderFailedException
    {
        public DoesNotFitOnPageException(string message) : base(message){}
    }
}