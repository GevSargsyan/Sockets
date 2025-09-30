using MessagePack;
using System.Text.Json.Serialization;

namespace BEWebBackOfficeEntities.SocketEntities.ResponseModel.WSResponse
{
    [MessagePackObject(false)]
    public class WSEchoResponse : IBaseWSResponseModel
    {
        [Key(0)]
        [JsonPropertyName("RequestType")]
        public int RequestType { get; set; }
    }
}
