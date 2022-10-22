using System.Text.Json.Serialization;
using UnityEngine;
using SocketIOClient;
using Unity.VisualScripting;
using System.Threading;
using SocketIOClient.Transport;


public class BaseData
{
    [JsonPropertyName("uniqueId")]
    public string UniqueId { get; set; }

    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    [JsonPropertyName("profilePictureUrl")]
    public string ProfilePictureUrl { get; set; }
}
public class LikeData : BaseData { }
public class GiftData : BaseData
{
    [JsonPropertyName("diamondCount")]
    public int DiamondCount { get; set; }

    [JsonPropertyName("giftName")]
    public string GiftName { get; set; }

    [JsonPropertyName("giftPictureUrl")]
    public string GiftPictureUrl { get; set; }

    [JsonPropertyName("repeatCount")]
    public int RepeatCount { get; set; }

    [JsonPropertyName("repeatEnd")]
    public bool RepeatEnd { get; set; }
}

public class TiktokSocket : MonoBehaviour
{
    SocketIO client;

    [SerializeField]
    private string liveUser;

    [SerializeField]
    private GameObject CoinManager;

    SynchronizationContext context;

    private void RunMainThread(SendOrPostCallback callback)
    {
        context.Post(callback, null);
    }

    private void LikeHandler(LikeData data)
    {
        RunMainThread((_) =>
        {
            Debug.Log("LikeHandler");
            Debug.LogWarning(CoinManager.name);
            CustomEvent.Trigger(CoinManager, "ShotCoin");
        });
    }

    // Start is called before the first frame update
    async void Start()
    {
        // Main Thread Context
        context = SynchronizationContext.Current;

        client = new SocketIO("ws://localhost:3400", new SocketIOOptions
        {
            Reconnection = false,
            Transport = TransportProtocol.WebSocket,
        });
        Debug.Log("socket start");

        client.On("chat", (response) =>
        {
            Debug.Log(response);
        });
        client.On("like", (response) =>
        {
            Debug.Log(response);
            var data = response.GetValue<LikeData>();
            // Debug.Log(data.Nickname);
            LikeHandler(data);
        });
        client.On("gift", (response) =>
        {
            Debug.Log(response);
            // var data = response.GetValue<GiftData>();
            // Debug.Log(response);
            // Debug.Log(data.GiftName);
        });


        client.OnConnected += async (sender, e) =>
        {

            Debug.Log("hello socket!");
            // LikeHandler(new LikeData { });


            // var username = "romanioli3";
            // var username = "yamada_nu";
            // var username = "joemsaaaaa";
            await client.EmitAsync("enter", new { username = liveUser });
            Debug.Log("emit enter ");
        };
        client.OnError += (sender, e) =>
        {
            Debug.LogError(e);
        };
        client.OnReconnectAttempt += (sender, e) =>
        {
            Debug.Log("attempt reconnect websocket");
            Debug.Log(client.Options.ReconnectionAttempts);
        };

        await client.ConnectAsync();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private async void OnDestroy()
    {
        Debug.Log("destroy");
        if (client != null)
        {
            Debug.Log("disconnect");
            await client.DisconnectAsync();
            client.Dispose();
            client = null;
        }
    }

}
