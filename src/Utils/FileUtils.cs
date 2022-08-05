using envload.Models;
using libc.translation;
using LoadEnv.Models;
using Serilog;
using System.Text.Json;
using Terminal.Gui;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace LoadEnv.Utils
{
    public static class FileUtils
    {
        public static string directory = AppDomain.CurrentDomain.BaseDirectory;
        private static string colorScheme = "Base";
        private static string pathSettings = directory + "data";
        private static string workspace = directory + "workspace";
        private static string nameSettings = "settings.bin";
        private static string defaultBranch = "envs";
        private static string defaultProyect = "envload";
        private static string exampleRepoUrl = @"https://github.com/danijerez/envload";
        private static string pathFileSettings = pathSettings + @"\" + nameSettings;
        private static string defaultLocale = "en_US";
        public static string version = "0.0.5";

        public static void Save(string? path, object obj)
        {
            if (path != null)
                using (var file = File.Create(path)) { ProtoBuf.Serializer.Serialize(file, obj); }
        }

        public static void InyectEnviroments(ListView listFiles, bool clear, Settings s, ILocalizer rb)
        {
            string pathFiles = s.Workspace + @"\" + s.Proyect;

            var text = !clear ? rb.Get("injected") : rb.Get("deleted");
            var list = listFiles.Source.ToList();
            var select = list[listFiles.SelectedItem];
            if (select != null)
            {
                var path = select.ToString();
                if (path != null && !path.Equals(""))
                {
                    try
                    {
                        using StreamReader r = new(pathFiles + @"\" + path);

                        var source = TryDeserialiceEnviroments(r);

                        if (source != null && source.values != null)
                        {

                            source.values
                            .DistinctBy(x => x.name)
                            .ToList()
                            .ForEach(x =>
                            {
                                if (x.name != null)
                                    Parallel.Invoke(() => Environment.SetEnvironmentVariable(x.name, !clear ? x.value : null, EnvironmentVariableTarget.Machine));
                            });

                            int result = MessageBox.Query(200, source.values.DistinctBy(x => x.name).Count() + 6, rb.Get("alerts.info"), string.Format(rb.Get("msg.enviroments"), text) +
                                $"\n{string.Concat(source.values.Where(x => x.name != null).DistinctBy(x => x.name).Select((a) => string.Format("\n{0}{1}", a.name, new string(' ', 50 - a.name.Length))))}", rb.Get("ok"));

                        }

                    }
                    catch (Exception e)
                    {
                        MessageBox.ErrorQuery(70, 8, rb.Get("alerts.error"), e.Message, rb.Get("ok"));
                    }

                }
            }
        }

        public static Settings InitSettings()
        {
            Settings s = new()
            {
                Url = exampleRepoUrl,
                Password = string.Empty,
                Username = string.Empty,
                Workspace = workspace,
                Branch = defaultBranch,
                Proyect = defaultProyect,
                PathSettings = pathSettings,
                NameSettings = nameSettings,
                ColorScheme = colorScheme,
                Locale = defaultLocale,
                Version = version
            };

            if (File.Exists(pathFileSettings))
                using (var file = File.OpenRead(pathFileSettings))
                {
                    s = ProtoBuf.Serializer.Deserialize<Settings>(file);
                    s.PathSettings = pathSettings;
                    s.Workspace = workspace;
                }
            else
            {
                Directory.CreateDirectory(s.PathSettings);
                Save(pathFileSettings, s);
            }

            return s;
        }

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
                    Log.Warning(e2.Message);
                    return null;
                }

            }

        }

    }
}
