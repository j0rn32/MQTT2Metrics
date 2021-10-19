using static TopicUtil;

public class TestTopicUtil
{
    [Fact]
    public void Should_Get_TopicFilter()
    {
        Assert.Equal("home/+/temperature", GetTopicFilter("home/<sensor>/temperature"));
        Assert.Equal("home/+/temperature", GetTopicFilter("home/+/temperature"));
    }

    [Fact]
    public void Should_Match_NoWildcard()
    {
        Assert.Equal(
            (true, "SensorId", "B89D1E"),
            IsMatch("shellies/shellyht-<SensorId>/sensor/temperature", "shellies/shellyht-B89D1E/sensor/temperature")
        );
    }

    [Fact]
    public void Should_Match_Wildcard()
    {
        Assert.Equal(
            (true, "SensorId", "B89D1E"),
            IsMatch("+/shellyht-<SensorId>/+/temperature", "shellies/shellyht-B89D1E/sensor/temperature")
        );
    }

    [Fact]
    public void Should_Match_DoubleWildcard()
    {
        Assert.Equal(
            (true, null, null),
            IsMatch("+/#", "shellies/shellyht-B89D1E/sensor/temperature")
        );
        Assert.Equal(
            (true, "SensorId", "B89D1E"),
            IsMatch("#/shellyht-<SensorId>/+/temperature", "shellies/shellyht-B89D1E/sensor/temperature")
        );
        Assert.Equal(
            (true, "SensorId", "B89D1E"),
            IsMatch("#/shellyht-<SensorId>/+/+", "shellies/shellyht-B89D1E/sensor/temperature")
        );
    }

    [Fact]
    public void Should_Match_PathWildcard()
    {
        Assert.Equal(
            (true, "SensorId", "B89D1E"),
            IsMatch("#/shellyht-<SensorId>/#/temperature", "shellies/shellyht-B89D1E/sensor/temperature")
        );
        
        Assert.Equal(
            (true, "SensorId", "B89D1E"),
            IsMatch("#/shellyht-<SensorId>/#", "shellies/shellyht-B89D1E/sensor/temperature")
        );

        Assert.Equal(
            (true, null, null),
            IsMatch("#", "shellies/shellyht-B89D1E/sensor/temperature")
        );
    }
}