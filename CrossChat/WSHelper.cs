using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using Il2Cpp;
using Il2CppSteamworks;
using MelonLoader;

namespace CrossChat
{
    public class ChatMessage
    {
        public uint Id { get; set; }
        public int Context { get; set; }
        public string Content { get; set; }
    }

    public static class WSHelper
    {
        private static ClientWebSocket _webSocket = null;
        private static CancellationTokenSource _receiveCts;

        private static readonly ConcurrentQueue<Action> _mainThreadQueue = new();

        public static void ProcessMainThreadQueue()
        {
            while (_mainThreadQueue.TryDequeue(out var action))
            {
                try { action(); }
                catch (Exception ex) { MelonLogger.Error($"Main Thread Action Error: {ex.Message}"); }
            }
        }

        public static async Task Disconnect()
        {
            if (_webSocket == null) return;

            try
            {
                _receiveCts?.Cancel();

                if (_webSocket.State == WebSocketState.Open ||
                    _webSocket.State == WebSocketState.CloseReceived ||
                    _webSocket.State == WebSocketState.CloseSent)
                {
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(2));
                    await _webSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Client Disconnecting", timeout.Token);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"WebSocket Disconnect Handled: {ex.Message}");
            }
            finally
            {
                _webSocket?.Dispose();
                _webSocket = null;
                _receiveCts?.Dispose();
                _receiveCts = null;
                MelonLogger.Msg("WebSocket Disposed.");
            }
        }

        public static async Task Connect()
        {
            if (_webSocket != null) await Disconnect();

            string username = SteamFriends.GetPersonaName();
            _webSocket = new ClientWebSocket();
            _receiveCts = new CancellationTokenSource();
            
            Uri serverUri = new($"ws://localhost:1768/ws?name={Uri.EscapeDataString(username)}");

            try
            {
                MelonLogger.Msg("Connecting to WebSocket...");
                await _webSocket.ConnectAsync(serverUri, CancellationToken.None);
                MelonLogger.Msg("WebSocket Connected!");

                _ = Task.Run(() => ReceiveMessages(_receiveCts.Token));
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"WebSocket Connection Error: {ex.Message}");
            }
        }

        private static async Task ReceiveMessages(CancellationToken token)
        {
            var buffer = new byte[1024 * 4];
            try
            {
                while (_webSocket != null && _webSocket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                    
                    if (result.MessageType == WebSocketMessageType.Close) break;

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    using JsonDocument doc = JsonDocument.Parse(message);
                    string user = doc.RootElement.GetProperty("Username").GetString() ?? "Unknown";
                    string content = doc.RootElement.GetProperty("Content").GetString() ?? "";

                    _mainThreadQueue.Enqueue(() =>
                    {
                        if (ChatManager.Instance)
                        {
                            ChatManager.Instance.ReceiveChatMessage(0, user, content, true);
                        }
                    });
                }
            }
            catch (OperationCanceledException)
            { }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    MelonLogger.Error($"Receive Error: {ex.Message}");
                }
            }
        }

        public static async Task SendMessage(int context, string content)
        {
            if (_webSocket != null && _webSocket.State == WebSocketState.Open)
            {
                try
                {
                    var payload = new ChatMessage
                    {
                        Context = context,
                        Content = content
                    };

                    string jsonString = JsonSerializer.Serialize(payload);
                    var bytes = Encoding.UTF8.GetBytes(jsonString);

                    await _webSocket.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                }
                catch (Exception ex)
                {
                    MelonLogger.Error($"Send Error: {ex.Message}");
                }
            }
        }
    }
}