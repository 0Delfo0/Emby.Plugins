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

    public class BaseResponsePagedResponse : BaseResponse, IPagedResponse
    {
        public string user { get; set; }
        public int page { get; set; }
        public int perPage { get; set; }
        public int totalPages { get; set; }
        public int total { get; set; }

        public bool IsLastPage()
        {
            return page.Equals(totalPages);
        }
    }

    public interface IPagedResponse
    {
        string user { get; set; }
        int page { get; set; }
        int perPage { get; set; }
        int totalPages { get; set; }
        int total { get; set; }
        bool IsLastPage();
    }
}