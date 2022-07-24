using envload.Models;
using LoadEnv.Models;
using System.Text.Json;
using Terminal.Gui;

namespace LoadEnv.Utils
{
    public static class UI
    {
        public static View Config(Settings s)
        {
            var container = new View() { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };

            //config
            var containerConfig = new FrameView("config") { X = 0, Y = 2, Width = Dim.Fill() - 70, Height = 6 };
            var proyectLabel = new Label("name: ") { X = 1, Y = 0 };
            var urlLabel = new Label("repo: ") { X = 1, Y = 1 };
            var pathLabel = new Label("work: ") { X = 1, Y = 2 };
            var branchLabel = new Label("branch: ") { X = 1, Y = 3 };
            var proyectValue = new TextField(s.Proyect) { X = Pos.Right(branchLabel), Y = Pos.Top(proyectLabel), Width = Dim.Fill() };
            var urlValue = new TextField(s.Url) { X = Pos.Right(branchLabel), Y = Pos.Top(urlLabel), Width = Dim.Fill() };
            var pathValue = new TextField(s.Workspace) { X = Pos.Right(branchLabel), Y = Pos.Top(pathLabel), Width = Dim.Fill() };
            var branchValue = new TextField(s.Branch) { X = Pos.Right(branchLabel), Y = Pos.Top(branchLabel), Width = Dim.Fill() };
            containerConfig.Add(proyectLabel, urlLabel, pathLabel, branchLabel, proyectValue, urlValue, pathValue, branchValue);

            //git
            var containerGit = new FrameView("git") { X = 0, Y = 7, Width = Dim.Fill() - 70, Height = 4 };
            var loginLabel = new Label("user: ") { X = 1, Y = 0 };
            var passLabel = new Label("pass: ") { X = 1, Y = 1 };
            var loginValue = new TextField(s.Username) { X = Pos.Right(branchLabel), Y = Pos.Top(loginLabel), Width = Dim.Fill() };
            var passValue = new TextField(s.Password) { Secret = true, X = Pos.Right(branchLabel), Y = Pos.Top(passLabel), Width = Dim.Fill() };
            containerGit.Add(loginLabel, passLabel, loginValue, passValue);

            string pathFiles = pathValue.Text.ToString() + @"\" + proyectValue.Text.ToString();

            //files
            var containerFiles = new FrameView("files") { X = 0, Y = 11, Width = Dim.Fill() - 70, Height = Dim.Fill() };

            //envs
            var containerEnvs = new FrameView("enviroments") { X = Pos.Right(containerConfig), Y = 2, Width = Dim.Fill(), Height = Dim.Fill() };

            ListView listEnvs = new ListView { X = 1, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            var listFiles = ListFiles(listEnvs, pathFiles);
            containerFiles.Add(listFiles);
            containerEnvs.Add(listEnvs);

            container.Add(containerConfig, containerGit, containerFiles, containerEnvs, MenuTop(loginValue, passValue, pathValue, urlValue, proyectValue, branchValue, listFiles, s));

            return container;
        }

        public static ListView ListFiles(ListView listEnvs, string pathFiles)
        {
            ListView listFiles = new ListView()
            {
                X = 1,
                Y = 0,
                Height = Dim.Fill(),
                Width = Dim.Fill(),
                AllowsMultipleSelection = false,
            };

            if (Directory.Exists(pathFiles))
            {
                string[] files = Directory.GetFiles(pathFiles);
                listFiles.SetSource(files);
            }

            listFiles.SelectedItemChanged += (selected) =>
            {
                if (selected.Value == null)
                    return;

                var path = selected.Value.ToString();
                if (path != null)
                {
                    using (StreamReader r = new StreamReader(path))
                    {
                        try
                        {
                            EnvironmentJson? source = JsonSerializer.Deserialize<EnvironmentJson>(r.ReadToEnd());

                            if (source != null && source.values != null)
                            {
                                List<string> values = new List<string>();
                                for (int i = 0; i < source.values.Count(); i++)
                                {
                                    values.Add(string.Format("{0}: {1}", source.values[i].name, source.values[i].value));
                                }
                                listEnvs.SetSource(values);
                            }
                        }
                        catch (Exception e)
                        {
                            MessageBox.ErrorQuery(70, 8, "Error", e.Message, "ok");
                        }
                    }
                }
            };

            return listFiles;
        }

        public static MenuBar MenuTop(TextField username, TextField password, TextField path, TextField url, TextField proyect, TextField branch, ListView listFiles, Settings s)
        {
            var pathFileSettings = s.PathSettings + @"\" + s.NameSettings;
            var clone = new MenuItem("_Clone", "clone the configured repository.", () => { GitUtil.Clone(username, password, path, url, proyect, branch, listFiles); });
            var save = new MenuItem("_Save", "save changes in settings file.", () =>
            {

                s.Branch = branch.Text.ToString();
                s.Proyect = proyect.Text.ToString();
                s.Url = url.Text.ToString();
                s.Username = username.Text.ToString();
                s.Password = password.Text.ToString();
                s.Workspace = path.Text.ToString();
                FileUtils.Save(pathFileSettings, s);

            });

            var reset = new MenuItem("_Reset", "reset settings fields.", () =>
            {
                branch.Text = "";
                proyect.Text = "";
                url.Text = "";
                username.Text = "";
                password.Text = "";
                path.Text = "";
                File.Delete(pathFileSettings);

            });

            var inyect = new MenuItem("_Inyect", "inyect current enviroments selected in system.", () =>
            {
                int response = MessageBox.Query(70, 8, "Info", $"Do you want to inject the listed enviroments into the system?\n" +
                $"the process may take a few seconds.", "yes", "cancel");
                if (response.Equals(0))
                    Parallel.Invoke(() => InyectEnviroments(listFiles, false));
            });

            var clear = new MenuItem("_Clear", "clear current enviroments selected in system.", () =>
            {
                int response = MessageBox.Query(70, 8, "Info", $"Do you want to remove the listed enviroments into the system?\n" +
                $"the process may take a few seconds.", "yes", "cancel");
                if (response.Equals(0))
                    Parallel.Invoke(() => InyectEnviroments(listFiles, true));

            });

            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_Data", new MenuItem [] { clone }),
                new MenuBarItem ("_Settings", new MenuItem [] { save, reset }),
                new MenuBarItem ("_Enviroments", new MenuItem [] { inyect, clear })
            });

            return menu;
        }

        private static void InyectEnviroments(ListView listFiles, bool clear)
        {
            if (listFiles.Source == null)
                return;

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
                        using (StreamReader r = new StreamReader(path))
                        {

                            EnvironmentJson? source = JsonSerializer.Deserialize<EnvironmentJson>(r.ReadToEnd());
                            if (source != null && source.values != null)
                            {
                                for (var i = 0; i < source.values.Count(); i++)
                                {
                                    var name = source.values[i].name;
                                    if (name != null)
                                        Environment.SetEnvironmentVariable(name, !clear ? source.values[i].value : null, EnvironmentVariableTarget.Machine);
                                }
                                int result = MessageBox.Query(100, source.values.Count() + 6, "Info", $"Environment variables {text} in system:\n" +
                                    $"{string.Concat(source.values.Select((a) => string.Format("\n[{0}]:[{1}]", a.name, a.value)))}", "ok");

                            }
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
