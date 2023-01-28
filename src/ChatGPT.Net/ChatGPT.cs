using ChatGPT.Net.DTO;
using ChatGPT.Net.Session;
using Newtonsoft.Json;
using SocketIOClient;

namespace ChatGPT.Net;

public class ChatGpt
{
    public List<ChatGptMessage> Messages { get; set; } = new();
    public bool UseCache { get; set; } = true;
    public bool SaveCache { get; set; } = true;
    private bool Ready { get; set; } = false;
    private string BypassNode { get; set; }
    private SocketIO Socket { get; set; }
    private string SessionId { get; set; }
    private List<ChatGptClient> ChatGptClients { get; set; } = new();

    public ChatGpt(ChatGptConfig config = null)
    {
        config ??= new ChatGptConfig();
        SessionId = Guid.NewGuid().ToString();
        UseCache = config.UseCache;
        SaveCache = config.SaveCache;
        BypassNode = config.BypassNode;
        if (SaveCache)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await WaitForReady();
                    var json = JsonConvert.SerializeObject(Messages, Formatting.Indented);
                    await File.WriteAllTextAsync("cache.json", json);
                    await Task.Delay(30000);
                }
            });
        }

        Task.Run(Init);
    }

    public async Task WaitForReady()
    {
        while (!Ready) await Task.Delay(25);
    }

    public bool GetReadyState()
    {
        return Ready;
    }

    public SocketIO GetSocketConnection()
    {
        return Socket;
    }

    private async Task Init()
    {
        Ready = false;
        var firstConnection = true;
        Socket = new SocketIO(BypassNode, new SocketIOOptions
        {
            Reconnection = false,
            Query = new []
            {
                new KeyValuePair<string, string>("client", "csharp"),
                new KeyValuePair<string, string>("version", "1.1.5"),
                new KeyValuePair<string, string>("versionCode", "115"),
                new KeyValuePair<string, string>("signature", SessionId)
            }
        });

        Socket.OnConnected += (sender, e) =>
        {
            if(firstConnection) Console.WriteLine("Connected to the Bypass Node!");
            firstConnection = false;
            Ready = true;
        };
        
        Socket.OnReconnected += (sender, e) =>
        {
            Console.WriteLine("Reconnected to the Bypass Node!");
            Ready = true;
        };
        
        Socket.OnError += (sender, e) =>
        {
            Console.WriteLine($"Error: {e}");
        };

        Socket.OnReconnectAttempt += (sender, e) =>
        {
            Console.WriteLine($"Reconnecting...");
        };
        
        Socket.OnReconnectError += (sender, e) =>
        {
            Console.WriteLine($"Reconnection Error: {e}");
        };

        Socket.OnReconnectFailed += (sender, e) =>
        {
            Console.WriteLine($"Reconnection Failed!");
        };

        Socket.OnDisconnected += async (sender, e) =>
        {
            if(!Ready) return;
            Ready = false;
            Console.WriteLine("Disconnected from the Bypass Node! Reconnecting...");
            tryAgain:
            try
            {
                await Socket.ConnectAsync();
            }
            catch (Exception ex)
            {
                Ready = false;
                Console.WriteLine($"CacheException caught: {ex.Message}");
                Console.WriteLine("Retrying in 10 second...");
                await Task.Delay(5000);
                goto tryAgain;
            }
        };

        Socket.On("serverMessage", Console.WriteLine);

        while (true)
        {
            try
            {
                await Socket.ConnectAsync();
                break;
            }
            catch (Exception e)
            {
                Ready = false;
                Console.WriteLine($"CacheException caught: {e.Message}");
                Console.WriteLine("Retrying in 10 second...");
                await Task.Delay(5000);
            }
        }
    }
    
    public async Task<ChatGptClient> CreateClient(ChatGptClientConfig config)
    {
        var chatGptClient = new ChatGptClient(config, GetReadyState, GetSocketConnection);
        if (string.IsNullOrWhiteSpace(config.SessionToken))
        {
            throw new Exception("You need to provide either a session token or an account.");
        }

        if (!string.IsNullOrWhiteSpace(config.SessionToken))
        {
            await chatGptClient.RefreshAccessToken();
        }

        ChatGptClients.Add(chatGptClient);
        return chatGptClient;
    }
}