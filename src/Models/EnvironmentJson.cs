namespace envload.Models
{
    public class EnvironmentJson
    {
        public string? project { get; set; }
        public string? environment { get; set; }
        public List<Value>? values { get; set; }
    }

    public class Value
    {
        public string? name { get; set; }
        public string? value { get; set; }
    }
}
