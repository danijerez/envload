using libc.translation;
using LoadEnv.Models;
using Serilog;
using System.Data;
using System.Text;
using Terminal.Gui;

namespace LoadEnv.Utils
{
    public static class UI
    {

        public static void Init(Toplevel top, Settings s, ILocalizer rb)
        {
            top.Add(Containers(s, rb), CreateProgressBar(), CreateStatusBar(s, rb));
        }

        private static View Containers(Settings s, ILocalizer rb)
        {
            var container = new View() { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };

            //config
            var containerConfig = new FrameView(rb.Get("config")) { X = 0, Y = 2, Width = Dim.Sized(60), Height = 6 };
            var proyectLabel = new Label(rb.Get("name")) { X = 1, Y = 0 };
            var urlLabel = new Label(rb.Get("repo")) { X = 1, Y = 1 };
            var pathLabel = new Label(rb.Get("work")) { X = 1, Y = 2 };
            var branchLabel = new Label(rb.Get("branch")) { X = 1, Y = 3 };
            var proyectValue = new TextField(s.Proyect) { X = Pos.Right(branchLabel), Y = Pos.Top(proyectLabel), Width = Dim.Fill() };
            var urlValue = new TextField(s.Url) { X = Pos.Right(branchLabel), Y = Pos.Top(urlLabel), Width = Dim.Fill() };
            var pathValue = new TextField(s.Workspace) { X = Pos.Right(branchLabel), Y = Pos.Top(pathLabel), Width = Dim.Fill() };
            var branchValue = new TextField(s.Branch) { X = Pos.Right(branchLabel), Y = Pos.Top(branchLabel), Width = Dim.Fill() };

            containerConfig.Add(proyectLabel, urlLabel, pathLabel, branchLabel, proyectValue, urlValue, pathValue, branchValue);

            //git
            var containerGit = new FrameView(rb.Get("git")) { X = 0, Y = 7, Width = Dim.Sized(60), Height = 4 };
            var loginLabel = new Label(rb.Get("user")) { X = 1, Y = 0 };
            var passLabel = new Label(rb.Get("pass")) { X = 1, Y = 1 };
            var loginValue = new TextField(s.Username) { X = Pos.Right(branchLabel), Y = Pos.Top(loginLabel), Width = Dim.Fill() };
            var passValue = new TextField(s.Password) { Secret = true, X = Pos.Right(branchLabel), Y = Pos.Top(passLabel), Width = Dim.Fill() };
            containerGit.Add(loginLabel, passLabel, loginValue, passValue);

            string pathFiles = pathValue.Text.ToString() + @"\" + proyectValue.Text.ToString();

            //files
            var containerFiles = new FrameView(rb.Get("files")) { X = 0, Y = 11, Width = Dim.Sized(60), Height = Dim.Fill() - 1 };

            //envs
            var containerEnvs = new FrameView(rb.Get("envs")) { X = Pos.Right(containerConfig), Y = 2, Width = Dim.Fill(), Height = Dim.Fill() - 1 };

            TableView tableEnvs = new() { X = 1, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            var listFiles = ListFiles(containerEnvs, tableEnvs, pathFiles, rb);


            containerFiles.Add(listFiles);
            containerEnvs.Add(tableEnvs);

            container.Add(containerConfig, containerGit, containerFiles, containerEnvs,
                MenuTop(loginValue, passValue, pathValue, urlValue, proyectValue, branchValue, listFiles, s, rb));

            return container;
        }

        private static ListView ListFiles(FrameView containerEnvs, TableView tableEnvs, string pathFiles, ILocalizer rb)
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
                        var source = FileUtils.TryDeserialiceEnviroments(r);
                        var dt = new DataTable();
                        if (source != null && source.values != null)
                        {
                            if (source.project != null && source.environment != null)
                                containerEnvs.Title = string.Format("{0} -> {1} ~ {2}", rb.Get("envs"), source.project, source.environment);
                            List<string> values = new();

                            dt.Columns.Add(rb.Get("name"));
                            dt.Columns.Add(rb.Get("value"));

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

                        }
                        else
                        {
                            containerEnvs.Title = string.Format("{0} -> {1} ~ {2}", rb.Get("envs"), 404, rb.Get("notfound"));
                        }

                        tableEnvs.Table = dt;
                    }
                    catch (Exception e)
                    {
                        Log.Debug(e.Message);
                        MessageBox.ErrorQuery(70, 8, rb.Get("alerts.error"), e.Message, rb.Get("ok"));
                    }
                }
            };

            return listFiles;
        }

        private static MenuBar MenuTop(TextField username, TextField password, TextField path, TextField url, TextField proyect, TextField branch, ListView listFiles, Settings s, ILocalizer rb)
        {
            var pathFileSettings = s.PathSettings + @"\" + s.NameSettings;
            var clone = new MenuItem(rb.Get("menu.git.options.clone"), rb.Get("menu.git.info.clone"), () => { GitUtil.Clone(username, password, path, url, proyect, branch, listFiles, rb); });
            var save = new MenuItem(rb.Get("menu.settings.options.save"), rb.Get("menu.settings.info.save"), () =>
            {

                s.Branch = branch.Text.ToString();
                s.Proyect = proyect.Text.ToString();
                s.Url = url.Text.ToString();
                s.Username = username.Text.ToString();
                s.Password = password.Text.ToString();
                s.Workspace = path.Text.ToString();
                FileUtils.Save(pathFileSettings, s);

            });

            var reset = new MenuItem(rb.Get("menu.settings.options.reset"), rb.Get("menu.settings.info.reset"), () =>
            {
                branch.Text = "";
                proyect.Text = "";
                url.Text = "";
                username.Text = "";
                password.Text = "";
                path.Text = "";
                File.Delete(pathFileSettings);

            });

            var inyect = new MenuItem(rb.Get("menu.enviroments.options.inyect"), rb.Get("menu.enviroments.info.inyect"), () =>
            {
                if (listFiles.Source == null)
                    MessageBox.Query(70, 8, rb.Get("alerts.info"), rb.Get("msg.nopath"), rb.Get("ok"));
                else
                {
                    int response = MessageBox.Query(70, 8, rb.Get("alerts.info"), string.Format("{0}\n{1}", rb.Get("msg.inyect"), rb.Get("alerts.fewsec")),
                        rb.Get("yes"), rb.Get("cancel"));
                    if (response.Equals(0))
                        FileUtils.InyectEnviroments(listFiles, false, s, rb);
                }

            });

            var clear = new MenuItem(rb.Get("menu.enviroments.options.clear"), rb.Get("menu.enviroments.info.clear"), () =>
            {
                if (listFiles.Source == null)
                    MessageBox.Query(70, 8, rb.Get("alerts.info"), rb.Get("msg.nopath"), rb.Get("ok"));
                else
                {
                    int response = MessageBox.Query(70, 8, rb.Get("alerts.info"), string.Format("{0}\n{1}", rb.Get("msg.remove"), rb.Get("alerts.fewsec")),
                        rb.Get("yes"), rb.Get("cancel"));
                    if (response.Equals(0))
                        FileUtils.InyectEnviroments(listFiles, true, s, rb);
                }

            });

            var about = About(rb);



            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem (rb.Get("menu.git.title"), new MenuItem [] { clone }),
                new MenuBarItem (rb.Get("menu.settings.title"), new MenuItem [] { save, reset, new MenuBarItem (rb.Get("menu.settings.options.languages"), MenuLanguages(s, rb)) }),
                new MenuBarItem (rb.Get("menu.enviroments.title"), new MenuItem [] { inyect, clear }),
                new MenuBarItem (rb.Get("menu.help.title"), new MenuItem [] {
                    new MenuItem (rb.Get("menu.help.options.about"), rb.Get("menu.help.info.about"),
                    () =>  MessageBox.Query (about.Length + 2, 15, rb.Get("about"), about.ToString(), rb.Get("ok")), null, null, Key.CtrlMask | Key.A)
            })
                 }); ;



            return menu;
        }

        private static StringBuilder About(ILocalizer rb)
        {

            StringBuilder aboutMessage = new();
            aboutMessage.AppendLine(@"");
            aboutMessage.AppendLine(rb.Get("msg.des"));
            aboutMessage.AppendLine(@"By danijerez (https://github.com/danijerez)");
            aboutMessage.AppendLine(@"");
            aboutMessage.AppendLine(@"");
            aboutMessage.AppendLine(@"  ____|                    |                           |");
            aboutMessage.AppendLine(@"  __|     __ \   \ \   /   |        _ \     _` |    _` |");
            aboutMessage.AppendLine(@"  |       |   |   \ \ /    |       (   |   (   |   (   |");
            aboutMessage.AppendLine(@" _____|  _|  _|    \_/    _____|  \___/   \__,_|  \__,_|");
            aboutMessage.AppendLine($"");
            aboutMessage.AppendLine($"~ v{FileUtils.version} ~");

            return aboutMessage;
        }


        private static ProgressBar CreateProgressBar()
        {
            var pb = new ProgressBar()
            {
                X = 0,
                Y = 1,
                Width = Dim.Fill(),
                Height = Dim.Fill(),
            };

            var loop = Application.MainLoop;

            loop.AddTimeout(TimeSpan.FromMilliseconds(35), timer);

            bool timer(MainLoop caller)
            {
                pb.Pulse();
                return true;
            }
            return pb;
        }

        private static StatusBar CreateStatusBar(Settings s, ILocalizer rb)
        {
            return new StatusBar(new StatusItem[] {
                new StatusItem(Key.Null, "EnvLoad v" + FileUtils.version, null),
                new StatusItem(Key.Null, s.Locale, null),
                new StatusItem(Key.F1, "~F1~ " + rb.Get("color"), () => {
                    if (s.ColorScheme == null)
                        return;
                    if (s.ColorScheme.Equals("Base"))
                        s.ColorScheme = "Error";
                    else if (s.ColorScheme.Equals("Error"))
                        s.ColorScheme = "Dialog";
                    else if (s.ColorScheme.Equals("Dialog"))
                        s.ColorScheme = "Menu";
                    else
                        s.ColorScheme = "Base";
                    Application.Top.ColorScheme = Colors.ColorSchemes[s.ColorScheme];
                })
            });
        }

        private static MenuItem[] MenuLanguages(Settings s, ILocalizer rb)
        {

            return new MenuItem[] {
                new MenuItem(rb.Get("languages.en_US"), null, () => { ChangeLanguage(s, rb, "en_US"); }) {},
                new MenuItem(rb.Get("languages.zh_CN"), null, () => { ChangeLanguage(s, rb, "zh_CN"); }){}
            };
        }

        private static void ChangeLanguage(Settings s, ILocalizer rb, string locale)
        {
            var pathFileSettings = s.PathSettings + @"\" + s.NameSettings;
            s.Locale = locale;
            FileUtils.Save(pathFileSettings, s);
            int response = MessageBox.Query(70, 8, rb.Get("languages." + locale), "\n" + rb.Get("msg.reset"), rb.Get("reset"), rb.Get("cancel"));
            if (response.Equals(0))
            {
                System.Diagnostics.Process.Start(FileUtils.directory + "envload");
                Environment.Exit(0);
            }

        }
    }
}
