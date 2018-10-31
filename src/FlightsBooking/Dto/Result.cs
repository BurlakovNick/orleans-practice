using Newtonsoft.Json;

namespace FlightsBooking.Dto
{
    public class Result
    {
        [JsonProperty("success")]
        public bool IsSuccess { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        public static Result Success() =>
            new Result
            {
                IsSuccess = true,
                Message = string.Empty
            };

        public static Result Fail(string message) =>
            new Result
            {
                IsSuccess = false,
                Message = message
            };
    }
}