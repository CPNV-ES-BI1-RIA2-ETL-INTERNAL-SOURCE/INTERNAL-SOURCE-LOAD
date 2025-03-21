# INTERNAL-SOURCE-LOAD

## Description

This is a .NET 8 solution, designed to be a microservice in an ETL software.

This is the **Load** part.

- The **Restfull API** working with ASP.net core
- Core maintainer : Delgado Noah

## Getting Started

### Prerequisites

- IDE: Visual Studio 2022+ [Download](https://visualstudio.microsoft.com/)
- .NET SDK 8.0+ [Download](https://dotnet.microsoft.com/download)
- Git version 2.39+ [Download](https://git-scm.com/)

### Getting started
#### Build the project:
```shell
dotnet restore
dotnet build
```

_With Docker_
```shell
docker build --target build -t build .
```

#### Run the api locally

```shell
cd INTERNAL-SOURCE-LOAD
dotnet run
```

You can go to
[http://localhost:5054/swagger/](http://localhost:5054/swagger/index.html) to
see API endpoints(api/v1/documents/load).

#### Test projects:

```shell
cd INTERNAL_SOURCE_LOAD_TESTS
```

And then

```shell
dotnet test
```

## Collaborate

### Directory Structure

```shell
INTERNAL-SOURCE-LOAD
├── INTERNAL-SOURCE-LOAD       // Main project
├── INTERNAL-SOURCE-LOAD_TESTS // Test projects
├── .gitignore                 // Git ignore rules
├───docs                        // Project documentation
├── README.md                  // Project overview and usage instructions
```

### Workflow

- [How to commit](https://www.conventionalcommits.org/en/v1.0.0/)
- [Gitflow Workflow](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow).

## License

Distributed under the MIT License. See [LICENSE.txt](LICENSE.txt) for more information.

## Contact

- If needed you can create an issue on GitHub
