using envload.Models;
using LoadEnv.Models;
using System.Data;
using System.Text.Json;
using Terminal.Gui;
using static Terminal.Gui.TableView;

namespace LoadEnv.Utils
{
    public static class UI
    {
        public static FrameView Config(Settings s)
        {
            var container = new FrameView() { X = 0, Y = 0, Width = Dim.Fill(), Height = Dim.Fill() };

            //config
            var containerConfig = new FrameView("config") { X = 0, Y = 1, Width = Dim.Fill(), Height = 6 };
            var proyectLabel = new Label("name: ") { X = 3, Y = 0 };
            var urlLabel = new Label("repo: ") { X = 3, Y = 1 };
            var pathLabel = new Label("work: ") { X = 3, Y = 2 };
            var branchLabel = new Label("branch: ") { X = 3, Y = 3 };
            var proyectValue = new TextField(s.Proyect) { X = Pos.Right(branchLabel), Y = Pos.Top(proyectLabel), Width = Dim.Fill() };
            var urlValue = new TextField(s.Url) { X = Pos.Right(branchLabel), Y = Pos.Top(urlLabel), Width = Dim.Fill() };
            var pathValue = new TextField(s.Workspace) { X = Pos.Right(branchLabel), Y = Pos.Top(pathLabel), Width = Dim.Fill() };
            var branchValue = new TextField(s.Branch) { X = Pos.Right(branchLabel), Y = Pos.Top(branchLabel), Width = Dim.Fill() };
            containerConfig.Add(proyectLabel, urlLabel, pathLabel, branchLabel, proyectValue, urlValue, pathValue, branchValue);

            //git
            var containerGit = new FrameView("git") { X = 0, Y = 7, Width = Dim.Fill(), Height = 4 };
            var loginLabel = new Label("user: ") { X = 3, Y = 0 };
            var passLabel = new Label("pass: ") { X = 3, Y = 1 };
            var loginValue = new TextField(s.Username) { X = Pos.Right(branchLabel), Y = Pos.Top(loginLabel), Width = Dim.Fill() };
            var passValue = new TextField(s.Password) { Secret = true, X = Pos.Right(branchLabel), Y = Pos.Top(passLabel), Width = Dim.Fill() };
            containerGit.Add(loginLabel, passLabel, loginValue, passValue);

            //files
            var containerFiles = new FrameView("files") { X = 0, Y = 11, Width = Dim.Fill(), Height = 6 };
            string[] files = Directory.GetFiles(pathValue.Text.ToString() + @"\" + proyectValue.Text.ToString());

            ListView list = new ListView
            {
                X = 3,
                Y = 0,
                Height = Dim.Fill(),
                Width = Dim.Fill(),
                AllowsMultipleSelection = false
            };

            list.SetSource(files);

            DataTable table = new DataTable() { };
            table.Columns.Add(new DataColumn() { ColumnName = "id" });
            table.Columns.Add(new DataColumn() { ColumnName = "name" });
            table.Columns.Add(new DataColumn() { ColumnName = "value" });

            TableView tableEnvs = new TableView { 
                X = 0, Y = Pos.Bottom(containerFiles), Width = Dim.Fill(), Height = Dim.Fill(), 
                Style = new TableStyle() {AlwaysShowHeaders = true }, 
                Table = table,
            };

            list.SelectedItemChanged += (selected) =>
            {
                using (StreamReader r = new StreamReader(selected.Value.ToString()))
                {
                    string json = r.ReadToEnd();
                    EnvironmentJson? source = JsonSerializer.Deserialize<EnvironmentJson>(json);
                    table.Rows.Clear();
                    for (int i= 0; i < source.values.Count();i++)
                    {
                        DataRow row = table.NewRow();
                        row["id"] = i+1;
                        row["name"] = source.values[i].name;
                        row["value"] = source.values[i].value;
                        table.Rows.Add(row);
                    }
                    
                    table.MinimumCapacity = source.values.Count();
                    tableEnvs.Table = table;
                }
            };

            containerFiles.Add(list);
            container.Add(containerConfig, containerGit, containerFiles, tableEnvs, GitUtil.Options(loginValue, passValue, pathValue, urlValue, proyectValue, branchValue, s));

            return container;
        }

    }
}
