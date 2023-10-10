#r "nuget: Fun.Build, 1.0.0"

open System
open System.IO
open Fun.Build


let options = {|
    GithubAction = EnvArg.Create("GITHUB_ACTION", description = "Run only in in github action container")
    NugetAPIKey = EnvArg.Create("NUGET_API_KEY", description = "Nuget api key")
|}

let getVersionFromChangelogFor dir =
    File.ReadLines(Path.Combine(dir, "CHANGELOG.md"))
    |> Seq.tryPick (fun line ->
        let line = line.Trim()
        // We'd better to not release unreleased version
        if "## [Unreleased]".Equals(line, StringComparison.OrdinalIgnoreCase) then
            None
        // Simple way to find the version string
        else if line.StartsWith "## [" && line.Contains "]" then
            let version = line.Substring(4, line.IndexOf("]") - 4)
            // In the future we can verify version format according to more rules
            if Char.IsDigit version[0] |> not then failwith "First number should be digit"
            Some(version)
        else
            None
    )

pipeline "publish" {
    stage "Build packages" {
        run (fun _ ->
            let version = getVersionFromChangelogFor __SOURCE_DIRECTORY__ |> Option.defaultWith (fun () -> failwith "Version is not found")
            $"dotnet pack -c Release Fun.LEGO.Spike/Fun.LEGO.Spike.csproj -p:PackageVersion={version} -o ."
        )
    }
    stage "Publish packages to nuget" {
        whenBranch "main"
        whenEnvVar options.GithubAction
        whenEnvVar options.NugetAPIKey
        run (fun ctx ->
            let key = ctx.GetEnvVar options.NugetAPIKey.Name
            ctx.RunSensitiveCommand $"""dotnet nuget push *.nupkg -s https://api.nuget.org/v3/index.json --skip-duplicate -k {key}"""
        )
    }
    runIfOnlySpecified false
}


tryPrintPipelineCommandHelp ()
