using MessagePack;
using System.Text.Json.Serialization;

namespace BEWebBackOfficeEntities.SocketEntities.RequestModel.RiskNotification
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = nameof(RiskNotificationSocketRequestType))]
    [JsonDerivedType(typeof(NotificationEchoSocketRequestModel), typeDiscriminator: (int)NotificationSocketRequestType.Echo)]

    [MessagePackObject(false)]
    [Union((int)NotificationSocketRequestType.Echo, typeof(NotificationEchoSocketRequestModel))]
    public abstract class NotificationBaseSocketRequestModel
    {
        [Key(0)]
        [JsonPropertyName("RiskNotificationSocketRequestType")]
        public NotificationSocketRequestType RiskNotificationSocketRequestType { get; set; }
    }
}
