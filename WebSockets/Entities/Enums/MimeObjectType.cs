using System.ComponentModel;

namespace BEWebBackOfficeEntities.Enums
{
    public enum MimeObjectType
    {
        None = 0,

        [Description("application/json")]
        Json = 1,

        [Description("application/x-msgpack")]
        MessagePack = 2
    }
}
