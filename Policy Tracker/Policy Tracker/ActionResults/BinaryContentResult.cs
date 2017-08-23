using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PolicyTracker.ActionResults
{
    /// <summary>
    /// An ActionResult used to send binary data to the browser.
    /// </summary>
    public class BinaryContentResult : ActionResult
    {
        private readonly string contentType;
        private readonly byte[] contentBytes;

        public BinaryContentResult(byte[] contentBytes, string contentType)
        {
            this.contentBytes = contentBytes;
            this.contentType = contentType;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var response = context.HttpContext.Response;
            response.Clear();
            response.Cache.SetCacheability(HttpCacheability.Public);
            response.ContentType = this.contentType;

            using (var stream = new MemoryStream(this.contentBytes))
            {
                stream.WriteTo(response.OutputStream);
                stream.Flush();
            }
        }
    }
}