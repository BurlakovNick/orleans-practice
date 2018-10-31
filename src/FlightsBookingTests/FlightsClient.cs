using System;
using System.Net;
using System.Threading.Tasks;
using FlightsBooking.Dto;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Deserializers;
using ISerializer = RestSharp.Serializers.ISerializer;

namespace FlightsBookingTests
{
    public class FlightsClient
    {
        private readonly bool needLogging;
        private readonly RestClient client;

        public FlightsClient(bool needLogging = false)
        {
            this.needLogging = needLogging;
            client = new RestClient("http://localhost:52362/api/flights");

            //note: https://bytefish.de/blog/restsharp_custom_json_serializer/
            client.AddHandler("application/json", Serializer.Instance);
            client.AddHandler("text/json", Serializer.Instance);
            client.AddHandler("text/x-json", Serializer.Instance);
            client.AddHandler("text/javascript", Serializer.Instance);
            client.AddHandler("*+json", Serializer.Instance);
        }

        public Task<FlightDto> GetFlight(Guid flightId)
        {
            var request = new RestRequest($"{flightId}", Method.GET, DataFormat.Json);
            return ExecuteAsync<FlightDto>(request);
        }

        public Task<Result> HoldSeatAsync(Guid flightId, string seatId, string userId, int? holdSeconds = null)
        {
            var request = new RestRequest($"{flightId}/seat/{seatId}/hold", Method.POST, DataFormat.Json);
            request.AddQueryParameter("userId", userId);
            if (holdSeconds.HasValue)
            {
                request.AddQueryParameter("holdSeconds", holdSeconds.Value.ToString());
            }
            return ExecuteAsync<Result>(request);
        }

        public Task<Result> BuySeatAsync(Guid flightId, string seatId, string userId)
        {
            var request = new RestRequest($"{flightId}/seat/{seatId}/buy", Method.POST, DataFormat.Json);
            request.AddQueryParameter("userId", userId);
            return ExecuteAsync<Result>(request);
        }

        private async Task<T> ExecuteAsync<T>(RestRequest request)
        {
            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = Serializer.Instance;

            Log($"Executing {request.Resource}");
            if (request.Parameters.Count > 0)
            {
                Log($"Params: {JsonConvert.SerializeObject(request.Parameters)}");
            }

            var response = await client.ExecuteTaskAsync<T>(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Status code is {response.StatusCode}");
            }

            if (response.ResponseStatus != ResponseStatus.Completed)
            {
                throw new InvalidOperationException($"Response status is {response.ResponseStatus}", response.ErrorException);
            }

            Log($"Executed {request.Resource}");
            Log($"Result: {response.Content}");
            
            return response.Data;
        }

        private async Task<IRestResponse> ExecuteAsync(RestRequest request)
        {
            request.RequestFormat = DataFormat.Json;
            request.JsonSerializer = Serializer.Instance;

            Log($"Executing {request.Resource}");

            var response = await client.ExecuteTaskAsync(request);
            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Status code is {response.StatusCode}");
            }

            Log($"Executed {request.Resource}");

            return response;
        }

        private void Log(string message)
        {
            if (needLogging)
            {
                Console.WriteLine(message);
            }
        }

        private class Serializer : ISerializer, IDeserializer
        {
            private static readonly Serializer SerializerSingleton = new Serializer();
            public static readonly Serializer Instance = SerializerSingleton;

            private Serializer()
            {
                ContentType = "application/json";
            }

            public string Serialize(object obj)
            {
                return JsonConvert.SerializeObject(obj, Formatting.None);
            }

            public T Deserialize<T>(IRestResponse response)
            {
                return JsonConvert.DeserializeObject<T>(response.Content);
            }

            public string ContentType { get; set; }

            public string RootElement { get; set; }
            public string Namespace { get; set; }
            public string DateFormat { get; set; }
        }
    }
}