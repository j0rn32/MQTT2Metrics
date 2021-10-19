# MQTT2Metrics
Connects to MQTT server and listens for messages.  Exposes Prometheus metrics.

.Net6 Minimal API.

## Running
```dotnet run src/MQTT2Metrics.csproj```

## Docker
```
docker build -t mqtt2metrics .
docker run -d --network=home --restart unless-stopped --name=mqtt2metrics -t mqtt2metrics
```

## Config
appsettings.json:
```
{
  "MQTTClient": {
    "Server" : "192.168.1.101",
    "Port" : 1883,
    "Subscriptions" : [
      {
          "Topic" : "shellies/shellyht-<SensorId>/sensor/temperature",
          "Name" : "Temperature"
      },
      {
        "Topic" : "shellies/shellyht-<SensorId>/sensor/humidity",
        "Name" : "Humidity"
      }
    ]
  }
}
```

## Metrics output
```
# TYPE Temperature gauge
Temperature{SensorId="F38FBF",topic="shellies/shellyht-F38FBF/sensor/temperature"} 23
Temperature{SensorId="B89D1E",topic="shellies/shellyht-B89D1E/sensor/temperature"} 16.25
# HELP Humidity 
# TYPE Humidity gauge
Humidity{SensorId="B89D1E",topic="shellies/shellyht-B89D1E/sensor/humidity"} 50.5
Humidity{SensorId="F38FBF",topic="shellies/shellyht-F38FBF/sensor/humidity"} 48
```
