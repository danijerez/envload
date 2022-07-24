using ProtoBuf;

namespace LoadEnv.Utils
{
    public static class FileUtils
    {
        public static void Save(string? path, object obj)
        {
            if (path != null)
                using (var file = File.Create(path)) { Serializer.Serialize(file, obj); }
        }

    }
}
