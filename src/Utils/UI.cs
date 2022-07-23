using LoadEnv.Models;
using Terminal.Gui;

namespace LoadEnv.Utils
{
    public static class UI
    {
        public static FrameView Config(Settings s)
        {

            var containerConfig = new FrameView("config") { X = 0, Y = 1, Width = Dim.Fill(), Height = 6 };
            var proyectLabel = new Label("name: ") { X = 3, Y = 0 };
            var urlLabel = new Label("repo: ") { X = 3, Y = 1 };
            var pathLabel = new Label("work: ") { X = 3, Y = 2 };
            var branchLabel = new Label("branch: ") { X = 3, Y = 3 };
            var proyectValue = new TextField(s.Proyect) { X = Pos.Right(branchLabel), Y = Pos.Top(proyectLabel), Width = Dim.Fill() };
            var urlValue = new TextField(s.Url) { X = Pos.Right(branchLabel), Y = Pos.Top(urlLabel), Width = Dim.Fill() };
            var pathValue = new TextField(s.Workspace) { X = Pos.Right(branchLabel), Y = Pos.Top(pathLabel), Width = Dim.Fill()};
            var branchValue = new TextField(s.Branch) { X = Pos.Right(branchLabel), Y = Pos.Top(branchLabel), Width = Dim.Fill() };
            containerConfig.Add(proyectLabel, urlLabel, pathLabel, branchLabel, proyectValue, urlValue, pathValue, branchValue);

            var containerGit = new FrameView("git") { X = 0, Y = 7, Width = Dim.Fill(), Height = 4 };
            var loginLabel = new Label("user: ") { X = 3, Y = 0};
            var passLabel = new Label("pass: ") { X = 3, Y = 1 };

            var loginValue = new TextField(s.Username) { X = Pos.Right(branchLabel), Y = Pos.Top(loginLabel), Width = Dim.Fill() };
            var passValue = new TextField(s.Password) { Secret = true, X = Pos.Right(branchLabel), Y = Pos.Top(passLabel), Width = Dim.Fill() };

            containerGit.Add(loginLabel, passLabel, loginValue, passValue);

            var container = new FrameView() { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };
            container.Add(containerConfig, containerGit, GitUtil.Options(loginValue, passValue, pathValue, urlValue, proyectValue, branchValue, s));

            return container;
        }

    }
}
