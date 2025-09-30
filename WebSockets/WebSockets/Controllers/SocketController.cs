using BEWebBackOfficeApi.Helpers;
using BEWebBackOfficeEntities.Enums;
using BEWebBackOfficeEntities.SocketEntities.RequestModel.RiskNotification;
using BEWebBackOfficeEntities.SocketEntities.ResponseModel.WSResponse;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using WebSockets.Helpers;
using WebSockets.NotificationConnectionManager;

namespace WebSockets.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SocketController : ControllerBase
    {
        private readonly INotificationConnectionManager _connectionManager;
        private readonly ILogger<SocketController> _logger;

        public SocketController(INotificationConnectionManager connectionManager, ILogger<SocketController> logger)
        {
            _connectionManager = connectionManager;
            _logger = logger;
        }


        [HttpGet]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {

                //var token = JwtHelper.GetToken(HttpContext);
                //if (string.IsNullOrEmpty(token))
                //{
                //    HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //    return;
                //}

                //var principal = JwtHelper.ValidateJwt(
                //    token,
                //    secretKey: ConfigManager.AppConfig.JWT.SigningKey,
                //    validAudiences: ConfigManager.AppConfig.JWT.Audience
                //);

                //if (principal == null)
                //{
                //    HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //    return;
                //}

                //var userIdStr = JwtHelper.GetClaimValue(principal, "currentUserId");
                //if (!int.TryParse(userIdStr, out var userId))
                //{
                //    HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                //    return;
                //}

                var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                await Accept(webSocket, 0);//userId
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task SendMessageAsync(WebSocket socket, byte[] serializedItem, Guid connectionId, int userId)
        {
            try
            {
                if (socket != null && socket.State == WebSocketState.Open)
                {
                    var timeout = 2000;
                    var cToken = new CancellationTokenSource(timeout).Token;

                    await socket.SendAsync(
                        new ArraySegment<byte>(serializedItem, 0, serializedItem.Length),
                        WebSocketMessageType.Text,
                        endOfMessage: true,
                        cToken
                    ).ConfigureAwait(false);
                }
                else
                {
                    await CloseSocket(connectionId, userId);
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, $"WebSocketException occurred during message sending for connectionId: {connectionId}");
                await CloseSocket(connectionId, userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occurred during message sending for connectionId: {connectionId}");
                await CloseSocket(connectionId, userId);
            }
        }

        private async Task Accept(WebSocket webSocket, int userId)
        {
            using (MemoryStream ms = new MemoryStream())
            {

                var connectionId = Guid.NewGuid();
                try
                {
                    _connectionManager.AddSocket(connectionId, webSocket, userId);

                    while (webSocket.State == WebSocketState.Open)
                    {
                        WebSocketReceiveResult result = null;
                        ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[4 * 1024]);

                        do
                        {
                            try
                            {
                                result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"WebSocket receive failed");
                                throw;
                            }

                            ms.Write(buffer.Array ?? new byte[0], buffer.Offset, result.Count);
                        }
                        while (!result.EndOfMessage);

                        ms.Position = 0;

                        switch (result.MessageType)
                        {
                            case WebSocketMessageType.Close:
                                await CloseSocket(connectionId, userId);
                                return;

                            case WebSocketMessageType.Text:
                                try
                                {
                                    NotificationBaseSocketRequestModel? request = SerializationHelper<NotificationBaseSocketRequestModel>.Deserializer(MimeObjectType.MessagePack, ms);

                                    if (request is not null)
                                    {
                                        var serializedObject = await ProcessCommandAsync(request, connectionId);

                                        if (serializedObject is not null)
                                        {
                                            var response = SerializationHelper<NotificationBaseSocketRequestModel>.Serializer(MimeObjectType.MessagePack, serializedObject);

                                            if (response is not null && webSocket.State == WebSocketState.Open)
                                            {
                                                try
                                                {
                                                    _ = SendMessageAsync(webSocket, response, connectionId, userId);
                                                }
                                                catch (WebSocketException ex)
                                                {
                                                    _logger.LogError(ex, $"WebSocketException_Failed_to_send_message, state - {webSocket.State}");
                                                }
                                                catch (Exception ex)
                                                {
                                                    _logger.LogError(ex, $"Exception_Failed_to_send_message, state - {webSocket.State}");
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error processing WebSocket message.");
                                }
                                break;
                        }

                        ms.Seek(0, SeekOrigin.Begin);
                        ms.SetLength(0);
                        ms.Position = 0;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"NotificationWS_Exception - {ex.Message}");
                }
                finally
                {
                    await CloseSocket(connectionId, userId);
                }
            }
        }

        private async Task CloseSocket(Guid guid, int userId)
        {
            try
            {
                var webSocket = _connectionManager.RemoveSocket(guid, userId);

                if (webSocket != null)
                {
                    if (webSocket.State == WebSocketState.Open || webSocket.State == WebSocketState.CloseReceived)
                    {
                        try
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by the WebSocketManager", CancellationToken.None).ConfigureAwait(false);
                        }
                        catch
                        {
                            webSocket.Abort();
                        }
                        _logger.LogError($"WebSocket_closed_with_state {webSocket.State}, totalConnectins - {_connectionManager.SocketCount}.");
                    }
                    else
                    {
                        _logger.LogError($"WebSocket_closed_with_state {webSocket.State}, totalConnectins - {_connectionManager.SocketCount}.");

                        webSocket.Abort();
                    }

                    webSocket.Dispose();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"WebSocket_CloseSocket {ex.Message}.");
            }
        }

        private async Task<IBaseWSResponseModel?> ProcessCommandAsync(NotificationBaseSocketRequestModel requestModel, Guid connectionId)
        {
            try
            {
                IBaseWSResponseModel? responseModel = null;
                switch (requestModel)
                {
                    case NotificationEchoSocketRequestModel echoSocketRequestModel:
                        responseModel = new WSEchoResponse();
                        return responseModel;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occurred on {nameof(ProcessCommandAsync)}");
                return new WSErrorResponse
                {
                    ErrorCode = (int)WSResponseResultCode.ExceptionOccurred,
                    ErrorMessage = ex.Message,
                };
            }

            return new WSSuccessResponse()
            {
                ResultCode = (int)WSResponseResultCode.Success
            };
        }
    }
}
