using envload.Models;
using libc.translation;
using LoadEnv.Models;
using ProtoBuf;
using System.Text.Json;
using Terminal.Gui;

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

        public static void Save(string? path, object obj)
        {
            if (path != null)
                using (var file = File.Create(path)) { Serializer.Serialize(file, obj); }
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

                        EnvironmentDto? source = JsonSerializer.Deserialize<EnvironmentDto>(r.ReadToEnd());
                        if (source != null && source.values != null)
                        {

                            source.values
                            .DistinctBy(x => x.name)
                            .ToList()
                            .ForEach(x =>
                            {
                                if (x.name != null)
                                    Environment.SetEnvironmentVariable(x.name, !clear ? x.value : null, EnvironmentVariableTarget.Machine);
                            });

                            int result = MessageBox.Query(200, source.values.DistinctBy(x => x.name).Count() + 6, rb.Get("alerts.info"), string.Format(rb.Get("msg.enviroments"), text) +
                                $"\n{string.Concat(source.values.DistinctBy(x => x.name).Select((a) => string.Format("\n{0}: {1}", a.name, a.value)))}", rb.Get("ok"));

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
                Locale = "en_US"
            };

            if (File.Exists(pathFileSettings))
                using (var file = File.OpenRead(pathFileSettings))
                {
                    s = Serializer.Deserialize<Settings>(file);
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

    }
}
