using envload.Models;
using LoadEnv.Models;
using ProtoBuf;
using System.Text.Json;
using Terminal.Gui;

namespace LoadEnv.Utils
{
    public static class FileUtils
    {
        public static void Save(string? path, object obj)
        {
            if (path != null)
                using (var file = File.Create(path)) { Serializer.Serialize(file, obj); }
        }

        public static void InyectEnviroments(ListView listFiles, bool clear, Settings s)
        {
            string pathFiles = s.Workspace + @"\" + s.Proyect;

            var text = !clear ? "injected" : "deleted";
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

                            int result = MessageBox.Query(200, source.values.DistinctBy(x => x.name).Count() + 6, "Info", $"Environment variables {text} in system:\n" +
                                $"{string.Concat(source.values.DistinctBy(x => x.name).Select((a) => string.Format("\n{0}: {1}", a.name, a.value)))}", "ok");

                        }

                    }
                    catch (Exception e)
                    {
                        MessageBox.ErrorQuery(70, 8, "Error", e.Message, "ok");
                    }

                }
            }
        }

    }
}
