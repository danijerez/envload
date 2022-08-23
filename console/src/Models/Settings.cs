using ProtoBuf;

namespace LoadEnv.Models
{
    [ProtoContract]
    public class Settings
    {
        [ProtoMember(1)]
        public string? Url { get; set; }
        [ProtoMember(2)]
        public string? Workspace { get; set; }
        [ProtoMember(3)]
        public string? Proyect { get; set; }
        [ProtoMember(4)]
        public string? Password { get; set; }
        [ProtoMember(5)]
        public string? Username { get; set; }
        [ProtoMember(6)]
        public string? Branch { get; set; }
        [ProtoMember(7)]
        public string? PathSettings { get; set; }
        [ProtoMember(8)]
        public string? NameSettings { get; set; }
        [ProtoMember(9)]
        public string? Locale { get; set; }
        [ProtoMember(10)]
        public string? ColorScheme { get; set; }
        [ProtoMember(11)]
        public string? Version { get; set; }
    }
}
