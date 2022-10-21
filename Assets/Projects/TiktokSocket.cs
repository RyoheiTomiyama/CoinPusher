using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using UnityEngine;
using SocketIOClient;
using Unity.VisualScripting;
using UnityEngine.Events;
using System.Threading;

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
            Debug.Log("Trigger ScoreUpp");
            Debug.LogWarning(CoinManager.name);
            CustomEvent.Trigger(CoinManager, "ShotCoin");
        });
    }

    // Start is called before the first frame update
    async void Start()

    {
        // Main Thread Context
        context = SynchronizationContext.Current;

        client = new SocketIO("ws://localhost:3400");
        Debug.Log("socket start");
        client.OnConnected += async (sender, e) =>
        {
            Debug.Log("hello socket!");
            Debug.Log("hello sockcet!");
            LikeHandler(new LikeData { });


            // var username = "romanioli3";
            // var username = "yamada_nu";
            // var username = "joemsaaaaa";
            await client.EmitAsync("enter", new { username = liveUser });
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

        client.On("chat", (response) =>
        {
            Debug.Log(response);
        });
        client.On("like", (response) =>
        {
            var data = response.GetValue<LikeData>();
            Debug.Log(data.Nickname);
            LikeHandler(data);
        });
        client.On("gift", (response) =>
        {
            var data = response.GetValue<GiftData>();
            Debug.Log(response);
            Debug.Log(data.GiftName);
        });

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
            // client.Dispose();
        }
    }

}
