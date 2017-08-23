using System;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Policy_Tracker.Utilities
{
    public static class HtmlViewRenderer
    {
        public class FakeView : IView
        {
            #region IView Members

            public void Render(ViewContext viewContext, TextWriter writer)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        public static string RenderViewToString(Controller controller, string viewName, object viewData)
        {
            var renderedView = new StringBuilder();
            using (var responseWriter = new StringWriter(renderedView))
            {
                var fakeResponse = new HttpResponse(responseWriter);
                var fakeContext = new HttpContext(HttpContext.Current.Request, fakeResponse);
                var fakeControllerContext = new ControllerContext(new HttpContextWrapper(fakeContext), controller.ControllerContext.RouteData, controller.ControllerContext.Controller);

                var oldContext = HttpContext.Current;
                HttpContext.Current = fakeContext;

                using (var viewPage = new ViewPage())
                {
                    var html = new HtmlHelper(CreateViewContext(responseWriter, fakeControllerContext), viewPage);
                    html.RenderPartial(viewName, viewData);
                    HttpContext.Current = oldContext;
                }
            }

            return renderedView.ToString();
        }

        private static ViewContext CreateViewContext(TextWriter responseWriter, ControllerContext fakeControllerContext)
        {
            return new ViewContext(fakeControllerContext, new FakeView(), new ViewDataDictionary(), new TempDataDictionary(), responseWriter);
        }
    }

    

    public static class PDFHelper
    {
        public static MemoryStream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(s);
                writer.Flush();
                stream.Position = 0;
            }
            return stream;
        }
    }
    /// <summary>
    /// MVC action result that generates the file content using a delegate that writes the content directly to the output stream.
    /// </summary>
    public class FileGeneratingResult : FileResult
    {
        /// <summary>
        /// The delegate that will generate the file content.
        /// </summary>
        private readonly Action<System.IO.Stream> content;

        private readonly bool bufferOutput;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileGeneratingResult" /> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="content">Delegate with Stream parameter. This is the stream to which content should be written.</param>
        /// <param name="bufferOutput">use output buffering. Set to false for large files to prevent OutOfMemoryException.</param>
        public FileGeneratingResult(string fileName, string contentType, Action<System.IO.Stream> content, bool bufferOutput = true)
            : base(contentType)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            this.content = content;
            this.bufferOutput = bufferOutput;
            FileDownloadName = fileName;
        }

        /// <summary>
        /// Writes the file to the response.
        /// </summary>
        /// <param name="response">The response object.</param>
        protected override void WriteFile(System.Web.HttpResponseBase response)
        {
            response.Buffer = bufferOutput;
            content(response.OutputStream);
        }
    }
}