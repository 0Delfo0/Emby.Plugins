namespace Lastfm.Api.Model.Responses
{
    public class BaseResponse
    {
        public string message { get; set; }
        public int errorCode { get; set; }

        public bool IsError()
        {
            return errorCode > 0;
        }
    }
}