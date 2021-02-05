namespace Client

module Index =
    open Fable.Remoting.Client
    open Common
    open Shared
    open Elmish

    let combine (paths: string list) =
        paths
        |> List.collect (fun path -> List.ofArray (path.Split('/')))
        |> List.filter (fun segment -> not (segment.Contains(".")))
        |> List.filter (System.String.IsNullOrWhiteSpace >> not)
        |> String.concat "/"
        |> sprintf "/%s"

    let normalize (path: string) = combine [ path ]

    let normalizeRoutes typeName methodName =
        Route.builder typeName methodName
        |> normalize

    let api =
        Remoting.createApi()
        |> Remoting.withRouteBuilder normalizeRoutes
        |> Remoting.buildProxy<IApi>

    type Model = {
        Msg : string
        Data : SomeOtherData option }

    type Msg =
        | Data of SomeOtherData
        | Act of SomeData
        | Acted of string

    let loadData query =
        Cmd.fromAsync
            { Value = api.Show query
              Error = fun _ -> failwith "error"
              Success = fun data -> Data data }

    let actOnData command =
        Cmd.fromAsync
            { Value = api.Act command
              Error = fun _ -> failwith "error"
              Success = fun acted -> Acted acted }

    let init _ =
        let initialModel = { Data= None; Msg = "" }
        initialModel, loadData ""

    let update (msg: Msg) (model: Model): Model * Cmd<Msg> =
        match msg with
        | Data data -> { model with Data = Some data }, Cmd.none
        | Act data -> model, actOnData { Data = data }
        | Acted s -> { model with Msg = s }, Cmd.none

    open Fable.React
    open Fable.React.Props

    let view (model: Model) (dispatch: Dispatch<Msg>) =

        match model.Data with
        | None -> div [] [ str "no data yet!"]
        | Some data ->
            
            div [] [
                span [] [ str model.Msg ]
                button [OnClick (fun _ -> (Act >> dispatch) data.C) ] [ str "Click me to see the magic"]
            ]