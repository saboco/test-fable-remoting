namespace Server


module Index =
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.Extensions.Configuration
    open Giraffe
    open Giraffe.GiraffeViewEngine
    open Shared
    open System.Text
    open Newtonsoft.Json
    open Fable.Remoting.Json
    open Shared

    /// use FableJsonConverter for complex types (in case state gets complex)
    let private fableConverter = FableJsonConverter() :> JsonConverter
    
    let private fableSerialize o =
        JsonConvert.SerializeObject(o, fableConverter)
    
    let bodyContent (state: InitialState) =
        let stateJson = // Serialize twice to output json as js string in html
            state |> fableSerialize |> fableSerialize
        
        StringBuilder()
            .Append("<div id=\"elmish-app\"></div>")
            .Append("<script>var __INIT_STATE__=")
            .Append(stateJson)
            .Append(";</script>")
            .ToString()

    let indexTemplate lang (assetsBaseUrl: System.Uri) (state: InitialState) =
        html [ _lang lang ] [
            head [] [
                meta [ _charset "utf-8" ]
                link [ _rel "shortcut icon"
                       _type "image/png"
                       _href "/favicon.png" ]
            ]
            body [] [
                rawText (bodyContent state)
                script [ _src (sprintf "%O%s" assetsBaseUrl "app.js") ] []
                script [ _src (sprintf "%O%s" assetsBaseUrl "style.js") ] []
                script [ _src (sprintf "%O%s" assetsBaseUrl "vendors_app.js") ] []
                script [ _src (sprintf "%O%s" assetsBaseUrl "vendors_app_style.js") ] []
            ]
        ]

    let indexHandler (config: IConfiguration) =
        fun ctx next ->
            task {

                let getParam name =
                    config.GetSection("WebSite").GetValue(name)

                let assetsBaseUrl = getParam "AssetsBaseUrl" |> System.Uri
                let defaultLang = getParam "DefaultLang"
                
                let debugUser =
                    { AuthorizedUser.UserName = "Cécile"
                      Claims =
                          [| "Root" |]}

                let state =
                    { HotelId = "12710"
                      Token = "Token: Root"
                      AuthorizedUser = debugUser
                      ShowMenu = true }

                let index =
                    indexTemplate defaultLang assetsBaseUrl state

                return! htmlView index ctx next
            }
