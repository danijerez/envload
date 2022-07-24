﻿using envload.Models;
using LibGit2Sharp;
using LoadEnv.Models;
using System.Text.Json;
using Terminal.Gui;

namespace LoadEnv.Utils
{
    public static class GitUtil
    {

        private static CloneOptions Options(string? username, string? password, string? branch)
        {
            Credentials credentials = new UsernamePasswordCredentials()
            {
                Username = username,
                Password = password
            };

            return new CloneOptions
            {
                BranchName = branch,
                CredentialsProvider = (url, usernameFromUrl, types) => credentials
            };
        }

        public static MenuBar Options(TextField username, TextField password, TextField path, TextField url, TextField proyect, TextField branch, ListView listFiles, Settings s, ProgressBar p)
        {
            var pathFileSettings = s.PathSettings + @"\" + s.NameSettings;
            var clone = new MenuItem("_Clone", "clone the configured repository.", () => { Clone(username, password, path, url, proyect, branch, listFiles); });
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
                InyectEnviroments(listFiles, false, p);
            });

            var clear = new MenuItem("_Clear", "clear current enviroments selected in system.", () =>
            {
                InyectEnviroments(listFiles, true, p);

            });

            var menu = new MenuBar(new MenuBarItem[] {
                new MenuBarItem ("_Data", new MenuItem [] { clone }),
                new MenuBarItem ("_Settings", new MenuItem [] { save, reset }),
                new MenuBarItem ("_Enviroments", new MenuItem [] { inyect, clear })
            });

            return menu;
        }

        private static void InyectEnviroments(ListView listFiles, bool clear, ProgressBar p)
        {
            var text = !clear ? "injected" : "deleted";
            var list = listFiles.Source.ToList();
            var select = list[listFiles.SelectedItem];
            if (select != null)
            {
                var path = select.ToString();
                if (path != null && !path.Equals(""))
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
                                p.Fraction += (float)i / (float)source.values.Count();
                                
                            }

                            int result = MessageBox.Query(100, source.values.Count() + 4, "Info", $"Environment variables {text} in system: " +
                                $"{string.Concat(source.values.Select((a) => string.Format("\n[{0}]:[{1}]", a.name, a.value)))}", "ok");
                            
                            if (result.Equals(0))
                                p.Fraction = 0;
                        }
                    }
                }
            }
        }


        private static void Clone(TextField usernameField, TextField passwordField, TextField pathField, TextField urlField, TextField proyectField, TextField branchField, ListView listFiles)
        {

            string? username = usernameField.Text.ToString();
            string? password = passwordField.Text.ToString();
            string? path = pathField.Text.ToString();
            string? url = urlField.Text.ToString();
            string? branch = branchField.Text.ToString();
            string? proyect = proyectField.Text.ToString();
            string directory = path + @"\" + proyect;
            int response = MessageBox.Query(70, 8, "Info", $"Do you want to clone the repository?\n" +
                $"the process may take a few seconds and overwrites the directory: {directory}", "yes", "cancel");

            if (response.Equals(1))
                return;

            try
            {
                if (path != null && !path.Equals("") && !Directory.Exists(path))
                    Directory.CreateDirectory(path);
                else if (Directory.Exists(path))
                {
                    foreach (var item in Directory.GetFiles(path))
                        File.Delete(item);
                    DeleteDirectory(path);
                    Directory.CreateDirectory(path);
                }

                string result = Repository.Clone(url, directory, Options(username, password, branch));

                if (Directory.Exists(pathField.Text.ToString() + @"\" + proyectField.Text.ToString()))
                {
                    string[] files = Directory.GetFiles(pathField.Text.ToString() + @"\" + proyectField.Text.ToString());
                    listFiles.SetSource(files);
                }

                MessageBox.Query(70, 8, "Info", $"Repository in branch '{branch}' cloned in '{result}'", "ok");

            }
            catch (Exception e)
            {
                MessageBox.ErrorQuery(70, 8, "Error", e.Message, "ok");
            }


        }

        private static void DeleteDirectory(string directory)
        {
            foreach (string subdirectory in Directory.EnumerateDirectories(directory))
            {
                DeleteDirectory(subdirectory);
            }

            foreach (string fileName in Directory.EnumerateFiles(directory))
            {
                var fileInfo = new FileInfo(fileName)
                {
                    Attributes = FileAttributes.Normal
                };
                fileInfo.Delete();
            }

            Directory.Delete(directory);
        }

    }
}
