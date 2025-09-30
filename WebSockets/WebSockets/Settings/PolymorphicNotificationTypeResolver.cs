using System.Text.Json.Serialization.Metadata;
using System.Text.Json.Serialization;
using System.Text.Json;
using BEWebBackOfficeEntities.SocketEntities.RequestModel.RiskNotification;

namespace BEWebBackOfficeApi.Settings
{
    public class PolymorphicNotificationTypeResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

            Type basePointType = typeof(NotificationBaseSocketRequestModel);
            if (jsonTypeInfo.Type == basePointType)
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = nameof(NotificationBaseSocketRequestModel.RiskNotificationSocketRequestType),

                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                    DerivedTypes =
                    {
                        new JsonDerivedType(typeof(NotificationEchoSocketRequestModel), (int)NotificationSocketRequestType.Echo)
                    }
                };

            }

            return jsonTypeInfo;
        }
    }
}
