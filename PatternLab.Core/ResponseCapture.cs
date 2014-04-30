using System;
using System.IO;
using System.Web;

namespace PatternLab.Core
{
    public class ResponseCapture : IDisposable
    {
        private StringWriter _localWriter;
        private readonly TextWriter _originalWriter;
        private readonly HttpResponseBase _response;

        public ResponseCapture(HttpResponseBase response)
        {
            _response = response;
            _originalWriter = response.Output;
            _localWriter = new StringWriter();

            response.Output = _localWriter;
        }

        public override string ToString()
        {
            _localWriter.Flush();
            return _localWriter.ToString();
        }

        public void Dispose()
        {
            if (_localWriter == null) return;

            _localWriter.Dispose();
            _localWriter = null;
            _response.Output = _originalWriter;
        }
    }
}