module Client.App

open Elmish
open Elmish.React

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram Index.init Index.update Index.view
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactHydrate "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
