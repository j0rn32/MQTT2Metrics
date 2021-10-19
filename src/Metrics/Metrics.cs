public class Metrics : IMetrics
{
    private readonly ConcurrentDictionary<string, IObserver> HistogramCache = new ConcurrentDictionary<string, IObserver>();
    private readonly ConcurrentDictionary<string, Counter> CounterCache = new ConcurrentDictionary<string, Counter>();
    private readonly ConcurrentDictionary<string, Gauge> ValueCache = new ConcurrentDictionary<string, Gauge>();
    private readonly ILogger<Metrics> Log;

    public Metrics(ILogger<Metrics> log)
    {
        Log = log;
    }

    private static string FixName(string name) => name.Replace(".", "_").Replace(' ', '_');

    #region Duration

    public IDisposable Duration(string name, string description = null, double[] buckets = null)
    {
        try
        {
            buckets ??= new[] { .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5, 7.5, 10 };
            name = FixName(name);
            if (!HistogramCache.TryGetValue(name, out var observer))
            {
                observer = Prometheus.Metrics.CreateHistogram(name, description ?? name, new HistogramConfiguration { Buckets = buckets });
                HistogramCache.AddOrUpdate(name, observer, (_, __) => observer);
            }

            if (observer == null)
                throw new NullReferenceException();

            return observer.NewTimer();
        }
        catch (Exception e)
        {
            Log?.LogError(e, "Failed on Metric duration! name={name}", name);
        }

        return new NothingDisposable();
    }

    public void Duration(string name, string description, int value, double[] buckets = null)
    {
        try
        {
            buckets ??= new[] { .005, .01, .025, .05, .075, .1, .25, .5, .75, 1, 2.5, 5, 7.5, 10 };
            name = FixName(name);
            if (!HistogramCache.TryGetValue(name, out var observer))
            {
                observer = Prometheus.Metrics.CreateHistogram(name, description ?? name, new HistogramConfiguration { Buckets = buckets });
                HistogramCache.AddOrUpdate(name, observer, (_, __) => observer);
            }

            observer.Observe(value);
        }
        catch (Exception e)
        {
            Log?.LogError(e, "Failed on Metric duration! name={name}, desc={description}, value={value}", name, description, value);
        }
    }

    #endregion

    #region Value

    public void Value(string name, double value, params (string labelName, string labelValue)[] instances)
    {
        try
        {
            name = FixName(name);
            if (!ValueCache.TryGetValue(name, out var gauge))
            {
                gauge = Prometheus.Metrics.CreateGauge(name, "", new GaugeConfiguration { SuppressInitialValue = true, LabelNames = instances.Select(i => FixName(i.labelName)).ToArray() });
                ValueCache.AddOrUpdate(name, gauge, (_, __) => gauge);
            }

            gauge.WithLabels(instances.Select(i => i.labelValue ?? "null").ToArray()).Set(value);
        }
        catch (Exception e)
        {
            Log?.LogError(e, "Failed on Metric value! name={name}, value={value}", name, value);
        }
    }

    public void Value(string name, double value)
    {
        try
        {
            name = FixName(name);
            if (!ValueCache.TryGetValue(name, out var gauge))
            {
                gauge = Prometheus.Metrics.CreateGauge(name, "", new GaugeConfiguration { SuppressInitialValue = true });
                ValueCache.AddOrUpdate(name, gauge, (_, __) => gauge);
            }

            gauge.Set(value);
        }
        catch (Exception e)
        {
            Log?.LogError(e, "Failed on Metric value! name={name}, value={value}", name, value);
        }
    }

    public void RemoveValue(string name)
    {
        try
        {
            if (!ValueCache.TryGetValue(name, out var gauge)) return;
            if (!(gauge is Gauge g)) return;
            if (!ValueCache.TryRemove(name, out _)) return;
            foreach (var labels in g.GetAllLabelValues())
                g.RemoveLabelled(labels);
            g.Unpublish();
        }
        catch (Exception e)
        {
            Log?.LogError(e, "Failed on Metric count! name={name}", name);
        }
    }

    #endregion

    #region Count

    public void Count(string name, string description = null, int countNumber = 1)
    {
        try
        {
            name = FixName(name);
            if (!CounterCache.TryGetValue(name, out var counter))
            {
                counter = Prometheus.Metrics.CreateCounter(name, description ?? name);
                CounterCache.AddOrUpdate(name, counter, (_, __) => counter);
            }

            counter.Inc(countNumber);
        }
        catch (Exception e)
        {
            Log?.LogError(e, "Failed on Metric count with single label! name={name}, desc={description}", name, description ?? "");
        }
    }

    public void Count(string name, string description = null, params (string labelName, string labelValue)[] instances)
        => Count(name, description, 1, instances);

    public void Count(string name, params (string labelName, string labelValue)[] instances)
        => Count(name, null, 1, instances);

    public void Count(string name, string description = null, int countNumber = 1, params (string labelName, string labelValue)[] instances)
    {
        try
        {
            name = FixName(name);
            if (!CounterCache.TryGetValue(name, out var counter))
            {
                counter = Prometheus.Metrics.CreateCounter(name, description ?? name, new CounterConfiguration
                {
                    LabelNames = instances.Select(i => FixName(i.labelName)).ToArray()
                });
                CounterCache.AddOrUpdate(name, counter, (_, __) => counter);
            }

            counter.WithLabels(instances.Select(i => i.labelValue ?? "null").ToArray()).Inc(countNumber);
        }
        catch (Exception e)
        {
            Log?.LogError(e, "Failed on Metric count with multiple labels! name={name}, desc={description}", name, description ?? "");
        }
    }

    #endregion
}