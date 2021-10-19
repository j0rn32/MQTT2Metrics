public interface IMessageHandler
{
    Task HandleMessage(string topic, string message);
}

public class MessageHandler : IMessageHandler
{
    private readonly ILogger<MessageHandler> Log;
    private readonly MQTTConfig.Subscription[] Subscriptions;
    private readonly IMetrics Metrics;

    public MessageHandler(MQTTConfig config, IMetrics metrics, ILogger<MessageHandler> log)
    {
        Metrics = metrics;
        Log = log;
        Subscriptions = config.Subscriptions;
    }

    public Task HandleMessage(string topic, string message)
    {
        foreach (var s in Subscriptions)
        {
            var (isMatch, sensor, sensorValue) = TopicUtil.IsMatch(s.Topic, topic);
            if (!isMatch) continue;
            Log.LogInformation("{topic} matched with {subscriptionTopic}. {sensor} = {value}", topic, s.Topic, sensor, sensorValue);
            if (!double.TryParse(message, out var value)) continue;
            Metrics.Value(s.Name, value, (sensor, sensorValue), ("topic", topic));
        }

        return Task.CompletedTask;
    }
}