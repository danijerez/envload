
using LoadEnv.Models;
using LoadEnv.Utils;
using ProtoBuf;
using Terminal.Gui;

Application.Init();
var top = Application.Top;
top.ColorScheme = Colors.Base;
string directory = AppDomain.CurrentDomain.BaseDirectory;

Settings s = new Settings
{
    Url = "",
    Password = "",
    Username = "",
    Workspace = directory + "workspace",
    Branch = "",
    Proyect = "",
    PathSettings = directory + "data",
    NameSettings = "settings.bin"
};

var pathFileSettings = s.PathSettings + @"\" + s.NameSettings;

if(File.Exists(pathFileSettings))
    using (var file = File.OpenRead(pathFileSettings)) { s = Serializer.Deserialize<Settings>(file); }
else
{
    Directory.CreateDirectory(s.PathSettings);
    FileUtils.Save(pathFileSettings, s);
}

top.Add(UI.Config(s));

Application.Run();
Application.Shutdown();

/*Environment.SetEnvironmentVariable("Test1", "Value1", EnvironmentVariableTarget.Machine);
Environment.SetEnvironmentVariable("Test1", null, EnvironmentVariableTarget.Machine);
Console.ReadLine();*/