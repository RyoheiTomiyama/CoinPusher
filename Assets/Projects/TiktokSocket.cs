using System.Text.Json.Serialization;
using UnityEngine;
using SocketIOClient;
using Unity.VisualScripting;
using System.Threading;
using SocketIOClient.Transport;
using System.Collections.Generic;

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

    Dictionary<string, int> giftRepeaters;

    private void RunMainThread(SendOrPostCallback callback)
    {
        context.Post(callback, null);
    }

    private void ShotCoin(int num = 1)
    {
        foreach (var _ in new List<int>(num))
        {
            RunMainThread((_) =>
            {
                CustomEvent.Trigger(CoinManager, "ShotCoin");
            });
        }
    }

    private void LikeHandler(LikeData data)
    {
        Debug.Log("LikeHandler");
        ShotCoin();
    }
    private void GiftHandler(GiftData data)
    {
        Debug.Log("GiftHandler");
        var n = data.RepeatCount;

        if (giftRepeaters.ContainsKey(data.UniqueId))
        {
            // 前回通知との差分を計算
            var prevCount = giftRepeaters[data.UniqueId];
            n -= prevCount;
            // 連打が終わったら
            if (data.RepeatEnd)
            {
                giftRepeaters.Remove(data.UniqueId);
            }
            // 連打の途中なら
            else
            {
                giftRepeaters[data.UniqueId] = data.RepeatCount;
            }
        }
        // 連打の初回なら
        else if (!data.RepeatEnd)
        {
            giftRepeaters.Add(data.UniqueId, data.RepeatCount);
        }
        ShotCoin(data.DiamondCount * n);
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
            var data = response.GetValue<GiftData>();
            GiftHandler(data);
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
