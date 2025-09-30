using System.Net.WebSockets;

namespace WebSockets.NotificationConnectionManager
{
    public interface INotificationConnectionManager
    {
        void AddSocket(Guid connectionId, WebSocket socket, int userId);

        WebSocket RemoveSocket(Guid connectionId, int userId);

        IEnumerable<WebSocket> GetSocketsByUserId(int userId);

        public int SocketCount { get; }
    }
}
