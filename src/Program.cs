using LoadEnv.Models;
using LoadEnv.Utils;
using ProtoBuf;
using Terminal.Gui;

Application.Init();
var top = Application.Top;
var colorScheme = "Base";

string directory = AppDomain.CurrentDomain.BaseDirectory;
var pathSettings = directory + "data";
var workspace = directory + "workspace";
var nameSettings = "settings.bin";
var defaultBranch = "envs";
var defaultProyect = "envload";
var exampleRepoUrl = @"https://github.com/danijerez/envload";
var pathFileSettings = pathSettings + @"\" + nameSettings;

Settings s = new Settings
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
};

if (File.Exists(pathFileSettings))
    using (var file = File.OpenRead(pathFileSettings)) { 
        s = Serializer.Deserialize<Settings>(file);
        s.PathSettings = pathSettings;
        s.Workspace = workspace;
    }
else
{
    Directory.CreateDirectory(s.PathSettings);
    FileUtils.Save(pathFileSettings, s);
}

top.ColorScheme = Colors.ColorSchemes[colorScheme];
var loop = Application.MainLoop;

var pb = new ProgressBar()
{
    X = 0,
    Y = 1,
    Width = Dim.Fill(),
    Height = Dim.Fill(),
};

loop.AddTimeout(TimeSpan.FromMilliseconds(35), timer);

bool timer(MainLoop caller)
{
    pb.Pulse();
    return true;
}

top.Add(UI.Config(s), pb);

Application.Run();
Application.Shutdown();