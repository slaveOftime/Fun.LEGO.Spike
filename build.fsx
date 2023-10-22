#r "nuget: Fun.Build, 1.0.1"

open Fun.Build

let options = {|
    GithubAction = EnvArg.Create("GITHUB_ENV", description = "Run only in in github action container")
    NugetAPIKey = EnvArg.Create("NUGET_API_KEY", description = "Nuget api key")
|}

pipeline "publish" {
    stage "Build packages" {
        run (fun _ ->
            let version = Changelog.GetLastVersion __SOURCE_DIRECTORY__ |> Option.defaultWith (fun () -> failwith "Version is not found")
            $"dotnet pack -c Release Fun.LEGO.Spike/Fun.LEGO.Spike.csproj -p:PackageVersion={version.Version} -o ."
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
