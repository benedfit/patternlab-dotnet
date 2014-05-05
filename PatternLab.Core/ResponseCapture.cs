using System;
using System.IO;
using System.Web;

namespace PatternLab.Core
{
    /// <summary>
    /// Class for capturing the contents of a HTTP response
    /// </summary>
    public class ResponseCapture : IDisposable
    {
        private StringWriter _localWriter;
        private readonly TextWriter _originalWriter;
        private readonly HttpResponseBase _response;

        /// <summary>
        /// Capture the contents of a HTTP response
        /// </summary>
        /// <param name="response">The HTTP response</param>
        public ResponseCapture(HttpResponseBase response)
        {
            _response = response;
            _originalWriter = response.Output;
            _localWriter = new StringWriter();

            response.Output = _localWriter;
        }

        /// <summary>
        /// Gets the contents of a HTTP response as a string
        /// </summary>
        /// <returns>The contents of a HHTP response as a string</returns>
        public override string ToString()
        {
            _localWriter.Flush();
            return _localWriter.ToString();
        }

        /// <summary>
        /// Dispose of the contents of the HTTP response
        /// </summary>
        public void Dispose()
        {
            if (_localWriter == null) return;

            // Dispose of local assets and return contents
            _localWriter.Dispose();
            _localWriter = null;
            _response.Output = _originalWriter;
        }
    }
}