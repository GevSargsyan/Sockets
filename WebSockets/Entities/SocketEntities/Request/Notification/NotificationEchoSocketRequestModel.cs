using MessagePack;

namespace BEWebBackOfficeEntities.SocketEntities.RequestModel.RiskNotification
{
    [MessagePackObject(false)]
    public class NotificationEchoSocketRequestModel : NotificationBaseSocketRequestModel
    {
        public NotificationEchoSocketRequestModel() => RiskNotificationSocketRequestType = NotificationSocketRequestType.Echo;

    }
}
