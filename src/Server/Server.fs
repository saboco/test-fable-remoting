namespace Server

open Shared
open Newtonsoft.Json

module Host =

    open System
    open System.IO

    open Microsoft.AspNetCore.Builder
    open Microsoft.AspNetCore.Hosting
    open Microsoft.Extensions.Logging
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.Extensions.Configuration

    open Fable.Remoting.Server
    open Fable.Remoting.Giraffe

    open NLog
    open NLog.Web
    open NLog.Extensions.Logging
    open Giraffe
    open LoggingMiddleware

    let tryGetEnv key =
        match Environment.GetEnvironmentVariable key with
        | x when String.IsNullOrWhiteSpace x -> None
        | x -> Some x

    let publicPath = Path.GetFullPath "../Client/public"

    let port =
        "SERVER_PORT"
        |> tryGetEnv
        |> Option.map uint16
        |> Option.defaultValue 8085us

    let logger = LogManager.GetLogger("UI.Server")

    let handleAct  : Command -> Async<string> =
        fun command ->
            async {
                printfn "%A" command
                return command.Data.CataA
            }

    let handleShow : Query -> Async<SomeOtherData> =
        fun _ ->
            async {

                let otherDataA = [
                    { Text = "uno"; Value = "1"}
                    { Text = "dos"; Value = "2"}
                    { Text = "tres"; Value = "3"}
                    { Text = "cuatro"; Value = "4"}
                    { Text = "cinco"; Value = "5"}
                ]
                let someDataB = [
                    { MataA = "mataA1"; MataC = "mataC1"; MataB = List.map (fun a -> Guid.NewGuid(), a) otherDataA |> Map.ofList }
                    { MataA = "mataA2"; MataC = "mataC2"; MataB = List.map (fun a -> Guid.NewGuid(), a) otherDataA |> Map.ofList }
                    { MataA = "mataA3"; MataC = "mataC3"; MataB = List.map (fun a -> Guid.NewGuid(), a) otherDataA |> Map.ofList }
                    { MataA = "mataA4"; MataC = "mataC4"; MataB = List.map (fun a -> Guid.NewGuid(), a) otherDataA |> Map.ofList }
                ]

                let some1 =
                    {   CataA = "cataA1"
                        CataC = "cataC1"
                        CataB = List.map (fun s -> Guid.NewGuid(), s) someDataB |> Map.ofList  }

                let data : SomeOtherData = {
                    A = "hola"
                    B = 1
                    C = some1
                }
                return data
            }

    let api () = {
            Show = handleShow
            Act = handleAct }

    let createApi () =
        Remoting.createApi ()
        |> Remoting.withRouteBuilder Route.builder
        |> Remoting.fromValue (api ())
        |> Remoting.withDiagnosticsLogger (logger.Debug)
        |> Remoting.buildHttpHandler

    let webApp () =
        choose [
            route "/ping" >=> text "pong"
            createApi ()
        ]

    let configureApp (app: IApplicationBuilder) =
        app
            .UseDefaultFiles()
            .UseStaticFiles()
            .UseRequestLogger()
            .UseGiraffe(webApp ())

    let configureServices (services: IServiceCollection) =

        services.AddGiraffe() |> ignore

    let configureLogging (host: WebHostBuilderContext) (builder: ILoggingBuilder) =
        builder
            .ClearProviders()
            .SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace)
        |> ignore

        builder.AddConfiguration(host.Configuration.GetSection("Logging"))
        |> ignore

        LogManager.Configuration <- NLogLoggingConfiguration(host.Configuration.GetSection("Logging:NLog"))


    let configureAppConfiguration (builder: IConfigurationBuilder) =
        builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
        |> ignore

    WebHostBuilder()
        .UseKestrel()
        .UseWebRoot(publicPath)
        .UseContentRoot(publicPath)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureAppConfiguration(configureAppConfiguration)
        .ConfigureServices(Action<IServiceCollection> configureServices)
        .ConfigureLogging(configureLogging)
        .UseUrls("http://0.0.0.0:" + port.ToString() + "/")
        .UseNLog()
        .UseIISIntegration()
        .Build()
        .Run()
