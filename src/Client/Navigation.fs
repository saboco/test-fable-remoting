module Client.Navigation

open Elmish.UrlParser
open Fable.Core.JsInterop
open Elmish.Navigation

/// The different pages of the application. If you add a new page, then add an entry here.
[<RequireQualifiedAccess>]
type Route =
    | RootPath
    | NotFound

type Url = Route


let toPath route =
    match route with
    | Route.RootPath -> "/"
    | Route.NotFound -> "/notfound" 

let pageParser: Parser<_ -> _, _> =
    oneOf [ 
            map Route.NotFound (s "notfound")
            map Route.RootPath (s "") ]

let goToUrl (e: Browser.Types.MouseEvent) =
    e.preventDefault ()
    let href: string = !!e.currentTarget?attributes?href?value

    Navigation.newUrl href
    |> List.map (fun f -> f ignore)
    |> ignore

let gotoRoute (route: Route) =
    let path = toPath route
    Navigation.newUrl path

let urlParser location = parsePath pageParser location
