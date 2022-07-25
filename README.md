# :scroll: EnvLoad ~ <img href="https://github.com/danijerez/envload/releases/tag/alpha" src="https://img.shields.io/badge/prerelease-0.0.1-orange"> <img src="https://img.shields.io/badge/dotnet-6.0-purple"> [![Twitter Follow](https://img.shields.io/twitter/follow/d4nijerez?style=social)](https://twitter.com/d4nijerez) ![GitHub Followers](https://img.shields.io/github/followers/danijerez?style=social) 

<b>EnvLoad</b> is a little portable program to loads environment variables into the operating system.
Environment variables are stored in repositories (public or private) in json files classified by project and with the following structure.
[Example](https://github.com/danijerez/envload/blob/envs/envload_local.json)
```json
{
    "project": "envload",
    "environment": "local",
    "values": [
        {
            "name": "env1",
            "value": "1"
        },
        {
            "name": "env2",
            "value": "2"
        },
        {
            "name": "env3",
            "value": "3"
        }
    ]
}
```

:moyai: I've actually created it for my own use, it's a hassle having to set up a local environment and forgetting to remove the settings before committ. It is open source in case you want to use it or improve it.

### :tv: ~ ui example
<img src="img/envload.png"> 

### :page_with_curl: ~ system result
<img src="img/system_envs.png">

:floppy_disk: The configuration is saved in a binary file 'settings.bin' encrypted :shipit: with the library [protobuf-net](https://github.com/protobuf-net/protobuf-net). The interface is simple (improvable :suspect:), designed with the library [Terminal.Gui](https://github.com/migueldeicaza/gui.cs/) and the connection with the repositories is thanks to [LibGit2Sharp](https://github.com/libgit2/libgit2sharp/) .

:warning: requires administrator permissions defined in the [app.manifest](https://github.com/danijerez/envload/blob/main/src/app.manifest)).

:construction: project is experimental and is in alpha phase.

### TargetFramework .net 6
| Name        | Descripción | Version     |
| ----------- | ----------- | ----------- |
| [protobuf-net](https://github.com/protobuf-net/protobuf-net)      | Protocol Buffers library for idiomatic .NET                        |3.1.17|
| [Terminal.Gui](https://github.com/migueldeicaza/gui.cs/)          | Console-based user interface toolkit for .NET applications.        |1.6.4|
| [LibGit2Sharp](https://github.com/libgit2/libgit2sharp/)          | Git + .NET/Mono = ❤        |0.26.2|

### If you use it or have any suggestions comment! ✌️
