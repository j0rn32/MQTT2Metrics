public class MQTTConfig
{
    public string Server { get; set; }
    public int Port { get; set; } = 1883;
    public Subscription[] Subscriptions { get; set; }

    public class Subscription
    {
        public string Topic { get; set; }
        public string Name { get; set; }
    }
}