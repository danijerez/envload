using ProtoBuf;

namespace LoadEnv.Models
{
    [ProtoContract]
    public class Settings
    {
        [ProtoMember(1)]
        public string? Workspace { get; set; }
        [ProtoMember(2)]
        public string? LastFile { get; set; }
        [ProtoMember(3)]
        public string? Locale { get; set; }
        [ProtoMember(4)]
        public int Width { get; set; }
        [ProtoMember(5)]
        public int Height { get; set; }

    }
}
