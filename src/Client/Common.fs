module Client.Common

open Elmish

[<AutoOpen>]
module Cmd =
    type AsyncValParams<'a, 'b> =
        { Value: Async<'a>
          Success: 'a -> 'b
          Error: exn -> 'b }

    let fromAsync (asyncParams: AsyncValParams<'a, 'b>): Elmish.Cmd<'b> =
        Cmd.OfAsync.either (fun () -> asyncParams.Value) () asyncParams.Success asyncParams.Error


