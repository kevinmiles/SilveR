# SilveR

SilveR is a cross platform (Windows, Linux & macOS) statistical analysis system with the UI written in .net core (currently 2.1.x), local data storage in DbSQLLite and the statistical analysis performed in R 3.5.1.

## Getting Started

Clone the project, open in Visual Studio, run the tests and then run/deploy. The repo contains a stripped down version of R 3.5.1 so that it should produce analyes straight away. You can always point the system at your own R setup (YMMV!)

See deployment for notes on how to deploy the project on a live system.

### Installing

SilveR is a self-hosted web app written in .net core, designed to run locally on Windows, Linux and macOS.

Releases can be produced through the following dotnet commands:
dotnet publish -c Release -r win-x64
dotnet publish -c Release -r linux-x64
dotnet publish -c Release -r osx-x64 

For windows you will need to xcopy the R-3.5.1 folder to the root of the publish output folder. For linux and mac you will need to provide your own R install (We need help here to work out how to add a specific version of R into the build process)

To run the published system run SilveR.exe. A console window will appear stating that the system is listening on http://localhost:5000. Open your browser and navigate to that location.




## Built With

* [.net Core](https://dotnet.microsoft.com/download) - The web framework used
* [sqllite](https://www.nuget.org/packages/Microsoft.Data.Sqlite.Core/) - Local data storage of data files and analyses
* [R](https://www.r-project.org/) - The R system for statistical analysis, powered by R script files

## Contributing

TODO 
Please read [CONTRIBUTING.md](https://gist.github.com/PurpleBooth/b24679402957c63ec426) for details on our code of conduct, and the process for submitting pull requests to us.

## License

This project is licensed under the MIT License - see the [LICENSE.md](LICENSE.md) file for details
