using MessagePack;
using System.Text.Json.Serialization;

namespace BEWebBackOfficeEntities.SocketEntities.ResponseModel.WSResponse
{
    [MessagePackObject(false)]
    public class WSSuccessResponse : IBaseWSResponseModel
    {
        [Key(0)]
        [JsonPropertyName("ResultCode")]
        public int ResultCode { get; set; }

        [Key(1)]
        [JsonPropertyName("RequestType")]
        public int RequestType { get; set; }

    }
}
