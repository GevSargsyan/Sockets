using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace WebSockets.NotificationConnectionManager
{
    public class NotificationConnectionManager : INotificationConnectionManager
    {
        private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new();

        private readonly ConcurrentDictionary<int, ConcurrentBag<Guid>> _userSockets = new();

        public void AddSocket(Guid connectionId, WebSocket socket, int userId)
        {
            _sockets.TryAdd(connectionId, socket);

            _userSockets.AddOrUpdate(
            userId,
            _ => new ConcurrentBag<Guid> { connectionId },
            (_, bag) =>
            {
                bag.Add(connectionId);
                return bag;
            });

        }

        public WebSocket RemoveSocket(Guid connectionId, int userId)
        {
            _sockets.TryRemove(connectionId, out var socket);
            if (_userSockets.TryGetValue(userId, out var connections))
            {
                connections.TryTake(out connectionId);
                if (_userSockets[userId].Count == 0)
                {
                    _userSockets.TryRemove(userId, out _);
                }
            }
            return socket;
        }

        public IEnumerable<WebSocket> GetSocketsByUserId(int userId)
        {
            if (_userSockets.TryGetValue(userId, out var connectionIds))
            {
                foreach (var id in connectionIds)
                {
                    if (_sockets.TryGetValue(id, out var socket))
                        yield return socket;
                }
            }
        }

        public int SocketCount { get => _sockets.Count; }

    }
}
