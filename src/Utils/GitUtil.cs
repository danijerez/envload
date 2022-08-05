
using envload.Utils;
using libc.translation;
using LibGit2Sharp;
using Serilog;
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

        public static void Clone(TextField usernameField, TextField passwordField, TextField pathField, TextField urlField, TextField proyectField, TextField branchField, ListView listFiles, ILocalizer rb)
        {

            string? username = usernameField.Text.ToString();
            string? password = passwordField.Text.ToString();
            string? path = pathField.Text.ToString();
            string? url = urlField.Text.ToString();
            string? branch = branchField.Text.ToString();
            string? proyect = proyectField.Text.ToString();
            string directory = path + @"\" + proyect;
            int response = MessageBox.Query(100, 8, rb.Get("alerts.info"),
                string.Format("{0}\n{1}\n\n{2}", rb.Get("msg.clone"), rb.Get("alerts.owsec"), directory.Truncate(80)),
                rb.Get("yes"), rb.Get("cancel"));

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
                    List<string> transform = files.ToList().Select(x => Path.GetFileName(x)).ToList();
                    listFiles.SetSource(transform);
                }

                var message = string.Format(rb.Get("msg.clonein"), branch, result);
                Log.Information(message);
                MessageBox.Query(70, 8, rb.Get("alerts.info"), message, rb.Get("ok"));

            }
            catch (Exception e)
            {
                Log.Debug(e.Message);
                MessageBox.ErrorQuery(100, 8, rb.Get("alerts.error"), e.Message, rb.Get("ok"));
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
