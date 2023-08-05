using System.Collections.Concurrent;

namespace ECSConfigProxy.ECS
{
    public class ECSConfig
    {
        public ConcurrentDictionary<string, ConfigValue> ConfigValues { get; set; }
    }

    public class ConfigValue
    {
        public string VariantName { get; set; }

        public bool IsDefault { get; set; }

        public double AllocationPercentage { get; set; }

        public int Priority { get; set; }

        public object Value { get; set; }

        public List<Filter> Filters { get; set; }
    }

    public class Filter
    {
        public string name { get; set; }

        public string dataType { get; set; }

        public string value { get; set; }
    }
}
