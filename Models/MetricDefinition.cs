namespace TopBarDock.Models
{
    public class MetricDefinition
    {
        public string Key { get; }
        public string Label { get; }
        public bool Enabled { get; set; }
        public int Order { get; set; }

        public Func<string> ValueProvider { get; }

        public MetricDefinition(
            string key,
            string label,
            bool enabled,
            Func<string> valueProvider,
            int order = 0)
        {
            Key = key;
            Label = label;
            Enabled = enabled;
            ValueProvider = valueProvider;
            Order = order;
        }
    }
}
