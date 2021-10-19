var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IMessageHandler, MessageHandler>();
builder.Services.AddHostedService<MQTTClient>();
builder.Services.AddSingleton<MQTTConfig>(builder.Configuration.GetSection("MQTTClient").Get<MQTTConfig>());
builder.Services.AddSingleton<IMetrics, Metrics>();

var app = builder.Build();
app.UseMetricServer();
await app.RunAsync();