using envload.Models;
using LoadEnv.Models;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace envload.Utils
{
    public static class FileUtils
    {
        public static string directory = AppDomain.CurrentDomain.BaseDirectory;
        public static string pathSettings = directory + "data";
        public static string nameSettings = "settings.bin";
        public static string pathFileSettings = pathSettings + @"\" + nameSettings;
        public static EnvironmentDto? TryDeserialiceEnviroments(StreamReader r)
        {
            try
            {
                return JsonSerializer.Deserialize<EnvironmentDto>(r.ReadToEnd());
            }
            catch (Exception)
            {
                try
                {
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                        .Build();
                    r.BaseStream.Position = 0;
                    return deserializer.Deserialize<EnvironmentDto>(r.ReadToEnd());
                }
                catch (Exception e2)
                {
                    return null;
                }

            }

        }

        public static void Save(string? path, object obj)
        {
            if (path != null)
                using (var file = File.Create(path)) { ProtoBuf.Serializer.Serialize(file, obj); }
        }

        public static void InyectEnviroments(EnvironmentDto envs, bool clear)
        {
            if (envs != null && envs.values != null)
            {
                envs.values
                    .DistinctBy(x => x.name)
                    .ToList()
                    .ForEach(x =>
                    {
                        if (x.name != null)
                            Parallel.Invoke(() => Environment.SetEnvironmentVariable(x.name, !clear ? x.value : null, EnvironmentVariableTarget.Machine));
                    });
            }
        }

        public static Settings InitSettings()
        {
            Settings s = new()
            {
                Locale = "en_US",
            };

            if (File.Exists(pathFileSettings))
                using (var file = File.OpenRead(pathFileSettings))
                {
                    s = ProtoBuf.Serializer.Deserialize<Settings>(file);
                }
            else
            {
                Directory.CreateDirectory(pathSettings);
                Save(pathFileSettings, s);
            }

            return s;
        }


    }
}
