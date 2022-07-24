using LoadEnv.Models;
using LoadEnv.Utils;
using ProtoBuf;
using Terminal.Gui;

Application.Init();
var top = Application.Top;
top.ColorScheme = Colors.Base;
string directory = AppDomain.CurrentDomain.BaseDirectory;

//Example settings
Settings s = new Settings
{
    Url = "https://github.com/danijerez/envload",
    Password = "",
    Username = "",
    Workspace = directory + "workspace",
    Branch = "envs",
    Proyect = "envload",
    PathSettings = directory + "data",
    NameSettings = "settings.bin"
};

var pathFileSettings = s.PathSettings + @"\" + s.NameSettings;

if (File.Exists(pathFileSettings))
    using (var file = File.OpenRead(pathFileSettings)) { s = Serializer.Deserialize<Settings>(file); }
else
{
    Directory.CreateDirectory(s.PathSettings);
    FileUtils.Save(pathFileSettings, s);
}

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