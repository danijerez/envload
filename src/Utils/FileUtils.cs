using ProtoBuf;

namespace LoadEnv.Utils
{
    public static class FileUtils
    {
        public static byte[]? ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;

            using var stream = new MemoryStream();
            Serializer.Serialize(stream, obj);

            return stream.ToArray();
        }

        public static T ByteArrayToObject<T>(byte[] arrBytes)
        {
            using var stream = new MemoryStream();

            // Ensure that our stream is at the beginning.
            stream.Write(arrBytes, 0, arrBytes.Length);
            stream.Seek(0, SeekOrigin.Begin);

            return Serializer.Deserialize<T>(stream);
        }

        public static void Save(string? path, object obj)
        {
            if (path != null)
                using (var file = File.Create(path)) { Serializer.Serialize(file, obj); }
        }

    }
}
