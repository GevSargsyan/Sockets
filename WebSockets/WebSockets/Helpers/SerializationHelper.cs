using BEWebBackOfficeApi.Settings;
using BEWebBackOfficeEntities.Enums;
using BEWebBackOfficeEntities.SocketEntities.RequestModel.RiskNotification;
using BEWebBackOfficeEntities.SocketEntities.ResponseModel.WSResponse;
using MessagePack;
using System.Buffers;
using System.Text;
using System.Text.Json;

namespace BEWebBackOfficeApi.Helpers
{
    public static class SerializationHelper<T> where T : class
    {
        private static readonly JsonSerializerOptions _jsonSerializerOptions;

        static SerializationHelper()
        {
            _jsonSerializerOptions = typeof(T) switch
            {
                Type t when t == typeof(NotificationBaseSocketRequestModel) => new JsonSerializerOptions
                {
                    TypeInfoResolver = new PolymorphicNotificationTypeResolver(),
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                },
                _ => throw new InvalidOperationException($"Unsupported type {typeof(T).Name}")
            };
        }

        public static T? Deserializer(MimeObjectType exchangeObjectType, Span<byte> combinedBytes)
        {
            try
            {
                if (!combinedBytes.IsEmpty)
                    return exchangeObjectType switch
                    {
                        MimeObjectType.Json => JsonSerializer.Deserialize<T>(combinedBytes, _jsonSerializerOptions),
                        MimeObjectType.MessagePack => MessagePackSerializer.Deserialize<T>(new ReadOnlySequence<byte>(combinedBytes.ToArray())),
                        _ => null,
                    };
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }


        public static T? Deserializer(MimeObjectType exchangeObjectType, MemoryStream memoryStream)
        {
            try
            {
                if (memoryStream.Length > 0)
                {
                    memoryStream.Position = 0;

                    return exchangeObjectType switch
                    {
                        MimeObjectType.Json => JsonSerializer.Deserialize<T>(memoryStream, _jsonSerializerOptions),
                        MimeObjectType.MessagePack => MessagePackSerializer.Deserialize<T>(memoryStream),
                        _ => null,
                    };
                }
            }
            catch (Exception)
            {
                return null;
            }

            return null;
        }


        public static byte[]? Serializer(MimeObjectType exchangeObjectType, IBaseWSResponseModel model)
        {
            try
            {
                return exchangeObjectType switch
                {
                    MimeObjectType.Json => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(model, _jsonSerializerOptions)),
                    MimeObjectType.MessagePack => MessagePackSerializer.Serialize(model),
                    _ => null,
                };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetJson(IBaseWSResponseModel model)
        {
            try
            {
                return JsonSerializer.Serialize(model, _jsonSerializerOptions);

            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
