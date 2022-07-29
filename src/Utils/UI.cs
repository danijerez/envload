using envload.Models;
using LoadEnv.Models;
using Serilog;
using System.Data;
using System.Text;
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
            var containerConfig = new FrameView("config") { X = 0, Y = 2, Width = Dim.Sized(60), Height = 6 };
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
            var containerGit = new FrameView("git") { X = 0, Y = 7, Width = Dim.Sized(60), Height = 4 };
            var loginLabel = new Label("user: ") { X = 1, Y = 0 };
            var passLabel = new Label("pass: ") { X = 1, Y = 1 };
            var loginValue = new TextField(s.Username) { X = Pos.Right(branchLabel), Y = Pos.Top(loginLabel), Width = Dim.Fill() };
            var passValue = new TextField(s.Password) { Secret = true, X = Pos.Right(branchLabel), Y = Pos.Top(passLabel), Width = Dim.Fill() };
            containerGit.Add(loginLabel, passLabel, loginValue, passValue);

            string pathFiles = pathValue.Text.ToString() + @"\" + proyectValue.Text.ToString();

            //files
            var containerFiles = new FrameView("files") { X = 0, Y = 11, Width = Dim.Sized(60), Height = Dim.Fill() - 1 };

            //envs
            var containerEnvs = new FrameView("enviroments") { X = Pos.Right(containerConfig), Y = 2, Width = Dim.Fill(), Height = Dim.Fill() - 1 };

            TableView tableEnvs = new() { X = 1, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            var listFiles = ListFiles(tableEnvs, pathFiles);


            containerFiles.Add(listFiles);
            containerEnvs.Add(tableEnvs);

            container.Add(containerConfig, containerGit, containerFiles, containerEnvs, MenuTop(loginValue, passValue, pathValue, urlValue, proyectValue, branchValue, listFiles, s));

            return container;
        }

        public static ListView ListFiles(TableView tableEnvs, string pathFiles)
        {
            ListView listFiles = new()
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
                List<string> transform = files.ToList().Select(x => Path.GetFileName(x)).ToList();
                listFiles.SetSource(transform);
            }

            listFiles.SelectedItemChanged += (selected) =>
            {
                if (selected.Value == null)
                    return;

                var path = selected.Value.ToString();
                if (path != null)
                {
                    using StreamReader r = new(pathFiles + @"\" + path);
                    try
                    {
                        EnvironmentDto? source = JsonSerializer.Deserialize<EnvironmentDto>(r.ReadToEnd());

                        if (source != null && source.values != null)
                        {
                            List<string> values = new();


                            var dt = new DataTable();
                            dt.Columns.Add("Name");
                            dt.Columns.Add("Value");

                            source.values
                            .DistinctBy(x => x.name)
                            .ToList()
                            .ForEach(x =>
                            {
                                var newRow = dt.NewRow();
                                newRow[0] = x.name;
                                newRow[1] = x.value;
                                dt.Rows.Add(newRow);
                            });

                            tableEnvs.Table = dt;
                            tableEnvs.EnsureValidScrollOffsets();
                            tableEnvs.Redraw(tableEnvs.Bounds);

                        }
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e.Message);
                        MessageBox.ErrorQuery(70, 8, "Error", e.Message, "ok");
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
                if (listFiles.Source == null)
                    MessageBox.Query(70, 8, "Info", $"Before loading variables you need to select a directory", "ok");
                else
                {
                    int response = MessageBox.Query(70, 8, "Info", $"Do you want to inject the listed enviroments into the system?\n" +
                                    $"the process may take a few seconds.", "yes", "cancel");
                    if (response.Equals(0))
                        Parallel.Invoke(() => FileUtils.InyectEnviroments(listFiles, false, s));
                }

            });

            var clear = new MenuItem("_Clear", "clear current enviroments selected in system.", () =>
            {
                if (listFiles.Source == null)
                    MessageBox.Query(70, 8, "Info", $"There is nothing selected to clear", "ok");
                else
                {
                    int response = MessageBox.Query(70, 8, "Info", $"Do you want to remove the listed enviroments into the system?\n" +
                                    $"the process may take a few seconds.", "yes", "cancel");
                    if (response.Equals(0))
                        Parallel.Invoke(() => FileUtils.InyectEnviroments(listFiles, true, s));
                }

            });

            var about = About();

            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_Git", new MenuItem [] { clone }),
                new MenuBarItem ("_Settings", new MenuItem [] { save, reset }),
                new MenuBarItem ("_Enviroments", new MenuItem [] { inyect, clear }),
                new MenuBarItem ("_Help", new MenuItem [] { new MenuItem ("_About...", "About this app", () =>  MessageBox.Query (about.Length + 2, 15, "About", about.ToString(), "_Ok"), null, null, Key.CtrlMask | Key.A)
            })
                 });



            return menu;
        }

        private static StringBuilder About()
        {

            StringBuilder aboutMessage = new();
            aboutMessage.AppendLine(@"");
            aboutMessage.AppendLine(@"  ____|                    |                           |");
            aboutMessage.AppendLine(@"  __|     __ \   \ \   /   |        _ \     _` |    _` |");
            aboutMessage.AppendLine(@"  |       |   |   \ \ /    |       (   |   (   |   (   |");
            aboutMessage.AppendLine(@" _____|  _|  _|    \_/    _____|  \___/   \__,_|  \__,_|");
            aboutMessage.AppendLine(@"");
            aboutMessage.AppendLine(@"");
            aboutMessage.AppendLine(@"Load environment variables in the operating system.");
            aboutMessage.AppendLine(@"By danijerez (https://github.com/danijerez)");
            aboutMessage.AppendLine(@"");
            return aboutMessage;
        }

    }
}
