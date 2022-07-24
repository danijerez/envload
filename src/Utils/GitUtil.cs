using LibGit2Sharp;
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

        public static void Clone(TextField usernameField, TextField passwordField, TextField pathField, TextField urlField, TextField proyectField, TextField branchField, ListView listFiles)
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
