Iceberg
==
🚧 Under Construction 🚧

### What is Iceberg?

Iceberg is a CLI application used for mapping the code dependencies in C# projects/solutions and generating basic [DGML](https://docs.microsoft.com/en-us/visualstudio/modeling/directed-graph-markup-language-dgml-reference?view=vs-2022) code maps.

### Currently Supports

- [x] Upstream Method Dependencies (partial support)
- [x] Downstream Method Dependencies (partial support)

### Basic CLI Commands

>
> **load** - Loads the specified solution file or project file(s). Automatically unloads the solution or project(s) that are currently loaded.
> 
> `load [-p|--path <file path(s)>]`
> 
> Options:
> `-p | --path` (required) The file path for of single solution file (.sln) or one or more project files (.csproj) to load into Iceberg.

> 
> **unload** - Unloads any solution or projects(s) that are currently loaded.
> 
> `unload`
> 
> Options: None

>
> **map** - Generates a dependency graph.
> 
> `map [-f|--flow {d|downstream, u|upstream}] [-m|--method <method name>] [-c|--class <class name>] [-p|--p <project>] [-d|--distance <distance>]`
> 
> Options:
> `-f | --flow` (required) The "flow"/direction of the dependency map. Upstream {u | upstream} maps all method calls which the entry point is directly or indirectly dependent on. Downstream {d | downstream} maps all method calls which are directly or indirectly dependent on the entry point.
> 
> `-c | --class` (required) The name of the class to use as the entry point when mapping.
>
> `-m | --method` The name of a specific method to use as the entry point when mapping. If none is provided, all methods defined on the class will be included in the generated map.
> 
> `-p | --project` The name of a specific project to search for entry points in. If a provided, only that projected will be searched. Otherwise, all available projects will be searched.
> 
> `-d | --distance` The maximum distance to map from the entry point. If provided, the generated map will only include dependencies which are no more than n degrees of separation away from the entry point. If not provided, all dependencies will be fully mapped.

>
> **view** - Opens the last generated dependency graph (for the current session) using the default application (e.g. Visual Studio's DGML Viewer).
> 
> `view`
>
> Options: None
