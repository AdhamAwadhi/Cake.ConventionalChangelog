#load nuget:?package=Cake.Recipe&version=1.0.0

Environment.SetVariableNames();

BuildParameters.SetParameters(context: Context,
                            buildSystem: BuildSystem,
                            sourceDirectoryPath: "./src",
                            title: "Cake.ConventionalChangelog",
                            repositoryOwner: "AdhamAwadhi",
                            repositoryName: "Cake.ConventionalChangelog",
                            appVeyorAccountName: "AdhamAwadhi",
                            shouldRunGitVersion: true);

BuildParameters.PrintParameters(Context);

ToolSettings.SetToolSettings(context: Context,
                            dupFinderExcludePattern: new string[] {
                                BuildParameters.RootDirectoryPath + "/src/Cake.ConventionalChangelog.Tests/*.cs", BuildParameters.RootDirectoryPath + "/src/Cake.ConventionalChangelog/ChangelogAliases.cs" },
                            testCoverageFilter: "+[*]* -[nunit.*]* -[Cake.Core]* -[Cake.Testing]* -[*.Tests]* ",
                            testCoverageExcludeByAttribute: "*.ExcludeFromCodeCoverage*",
                            testCoverageExcludeByFile: "*/*Designer.cs;*/*.g.cs;*/*.g.i.cs");
Build.Run();
