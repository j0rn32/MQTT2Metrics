using System.Linq;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;

public class MQTTClient : BackgroundService
{
    private readonly ILogger<MQTTClient> Log;
    private IMqttClient MqttClient;
    private readonly string MqttServer = "mosquitto";
    private readonly int MqttPort = 1883;

    private readonly IMessageHandler Handler;
    private readonly MqttTopicFilter[] Topics;

    public MQTTClient(IMessageHandler handler, MQTTConfig config, ILogger<MQTTClient> log)
    {
        MqttServer = config.Server;
        MqttPort = config.Port;
        Handler = handler;
        Log = log;
        Topics =
            config.Subscriptions.Select(s =>
                    new MqttTopicFilterBuilder().WithTopic(TopicUtil.GetTopicFilter(s.Topic)).Build())
                .ToArray();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new MqttFactory();
        MqttClient = factory.CreateMqttClient();

        await Reconnect();
        MqttClient.UseDisconnectedHandler(Reconnect);
    }

    private async Task Reconnect(MqttClientDisconnectedEventArgs args = null)
    {
        Log.LogInformation("MQTT reconnecting to {server}:{port} because {reason}", MqttServer, MqttPort, args?.Reason.ToString() ?? "nothing");
        if (args != null)
            await Task.Delay(TimeSpan.FromSeconds(5));

        var options = new MqttClientOptionsBuilder()
            .WithClientId("MQTT2Metrics2")
            .WithTcpServer(MqttServer, MqttPort)
            .WithCleanSession()
            .Build();

        MqttClient.UseApplicationMessageReceivedHandler(OnMessageReceived);
        MqttClient.UseConnectedHandler(OnConnected);
        await MqttClient.ConnectAsync(options);
    }

    private async Task OnConnected(MqttClientConnectedEventArgs args)
    {
        Log.LogInformation("MQTT connected, subscribing to {@topics}", Topics.Select(t => t.Topic));
        await MqttClient.SubscribeAsync(Topics);
    }

    private async Task OnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
    {
        try
        {
            var topic = args.ApplicationMessage.Topic;
            var payload = Encoding.ASCII.GetString(args.ApplicationMessage.Payload);

            Log.LogInformation($"MQTT message received {{topic}} : {payload}", topic, payload);
            await Handler.HandleMessage(topic, payload);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to process message {topic} : {payload}", args.ApplicationMessage.Topic, args.ApplicationMessage.Payload);
        }
    }
}