using LoadEnv.Models;
using LoadEnv.Utils;
using ProtoBuf;
using Serilog;
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

Log.Logger = new LoggerConfiguration()
         .MinimumLevel.Debug()
         .WriteTo.File(directory + @"\log\envload_.txt")
         .CreateLogger();

Settings s = new()
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
    using (var file = File.OpenRead(pathFileSettings))
    {
        s = Serializer.Deserialize<Settings>(file);
        s.PathSettings = pathSettings;
        s.Workspace = workspace;
    }
else
{
    Directory.CreateDirectory(s.PathSettings);
    FileUtils.Save(pathFileSettings, s);
}

top.ColorScheme = Colors.ColorSchemes[s.ColorScheme];
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

var statusBar = new StatusBar(new StatusItem[] {
                new StatusItem(Key.Null, "EnvLoad v0.0.2", null),
            new StatusItem(Key.F1, "~F1~ Color", () =>{
                if(s.ColorScheme == null)
                    return;
                if (s.ColorScheme.Equals("Base"))
                    s.ColorScheme = "Error";
                else if (s.ColorScheme.Equals("Error"))
                    s.ColorScheme = "Dialog";
                else if (s.ColorScheme.Equals("Dialog"))
                    s.ColorScheme = "Menu";
                else
                    s.ColorScheme = "Base";
                top.ColorScheme = Colors.ColorSchemes[s.ColorScheme];
            } )

        });


top.Add(UI.Config(s), pb, statusBar);

try
{
    Application.Run();
    Application.Shutdown();
}
catch (Exception e)
{
    Log.Error(e.Message, e);
}
