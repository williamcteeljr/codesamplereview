namespace WebAPI.Controllers.SendPostingNotice
{
    internal class HttpResponseMessage
    {
        private object oK;

        public HttpResponseMessage(object oK)
        {
            this.oK = oK;
        }
    }
}