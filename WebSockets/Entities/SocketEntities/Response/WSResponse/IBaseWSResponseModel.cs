using MessagePack;
using System.Text.Json.Serialization;

namespace BEWebBackOfficeEntities.SocketEntities.ResponseModel.WSResponse
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(WSResponseResultCode))]
    [JsonDerivedType(typeof(WSErrorResponse), typeDiscriminator: (int)WSResponseResultCode.ExceptionOccurred)]
    [JsonDerivedType(typeof(WSSuccessResponse), typeDiscriminator: (int)WSResponseResultCode.Success)]
    [JsonDerivedType(typeof(WSEchoResponse), typeDiscriminator: (int)WSResponseResultCode.Echo)]

    [Union((int)WSResponseResultCode.ExceptionOccurred, typeof(WSErrorResponse))]
    [Union((int)WSResponseResultCode.Success, typeof(WSSuccessResponse))]
    [Union((int)WSResponseResultCode.Echo, typeof(WSEchoResponse))]
    public interface IBaseWSResponseModel
    {
        int RequestType { get; set; }
    }
}
