public static class TopicUtil
{
    public static string GetTopicFilter(string subscription)
    {
        var parts = subscription.Split('/');
        for (int i = 0; i < parts.Length; i++)
            if (parts[i].Contains('<'))
                parts[i] = "+";
        return string.Join('/', parts);
    }

    public static (bool isMatch, string sensor, string sensorValue) IsMatch(string subscription, string topic)
    {
        (bool match, string sensor, string sensorValue) MatchPartWithSensor(string path, string pathSensor)
        {
            var sensor = pathSensor.Split(new char[] { '<', '>' }).Skip(1).First();

            var firstPart = pathSensor.Split(new char[] { '<', '>' }).First();
            var lastPart = pathSensor.Split(new char[] { '<', '>' }).Last();

            if (!string.IsNullOrWhiteSpace(firstPart) && !path.StartsWith(firstPart)) return (false, null, null);
            if (!string.IsNullOrWhiteSpace(lastPart) && !path.EndsWith(lastPart)) return (false, null, null);

            var sensorValue = path;
            if (!string.IsNullOrWhiteSpace(firstPart)) sensorValue = sensorValue.Replace(firstPart, "");
            if (!string.IsNullOrWhiteSpace(lastPart)) sensorValue = sensorValue.Replace(lastPart, "");
            return (true, sensor, sensorValue);
        }

        var topicParts = topic.Split('/');
        var parts = subscription.Split('/');
        string sensor = null;
        string sensorValue = null;
        var topicI = 0;
        for (int i = 0; i < parts.Length; i++)
        {
            if (topicI > topicParts.Length)
                return (false, null, null);

            if (topicParts[i] == parts[topicI]) // Same = continue
            {
                topicI++;
                continue;
            }

            if (parts[i] == "+") // Single wildcard = continue
            {
                topicI++;
                continue;
            }

            if (parts[i] == "#") // Multi-wildcard, loop until next 
            {
                if (i == parts.Length - 1) // Last is wildcard, success!
                    return (true, sensor, sensorValue);
                var next = parts[i + 1];
                while (true)
                {
                    topicI++;
                    if (topicI == topicParts.Length) return (false, null, null); // overflow, no match
                    if (topicParts[topicI] == next) break;
                    var checkFilter = MatchPartWithSensor(topicParts[topicI], next);
                    if (checkFilter.match)
                    {
                        sensor = checkFilter.sensor;
                        sensorValue = checkFilter.sensorValue;
                        break;
                    }
                }

                continue;
            }

            if (parts[i].Contains("<"))
            {
                var checkFilter = MatchPartWithSensor(topicParts[topicI], parts[i]);
                topicI++;
                if (checkFilter.match)
                {
                    sensor = checkFilter.sensor;
                    sensorValue = checkFilter.sensorValue;
                    continue;
                }
            }

            return (false, null, null);
        }

        return (true, sensor, sensorValue);
    }
}