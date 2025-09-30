using MessagePack;
using System.Text.Json.Serialization;

namespace BEWebBackOfficeEntities.SocketEntities.ResponseModel.WSResponse
{
    [MessagePackObject(false)]
    public class WSErrorResponse : IBaseWSResponseModel
    {
        [Key(0)]
        [JsonPropertyName("ErrorMessage")]
        public string ErrorMessage { get; set; }

        [Key(1)]
        [JsonPropertyName("ErrorCode")]
        public int ErrorCode { get; set; }

        [Key(2)]
        [JsonPropertyName("RequestType")]
        public int RequestType { get; set; }

        [Key(3)]
        [JsonPropertyName("ValidatonErrors")]
        public Dictionary<string, string[]> ValidatonErrors { get; set; }

    }
}
