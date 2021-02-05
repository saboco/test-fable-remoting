namespace Server

module LoggingMiddleware =

    open System
    open FSharp.Control.Tasks.V2.ContextInsensitive
    open Microsoft.AspNetCore.Http
    open Microsoft.AspNetCore.Builder
    open Microsoft.Extensions.Logging
    open Microsoft.AspNetCore.Http.Extensions
    open System.IO
    open System.Collections.Generic

    type RequestLoggerMiddleware(next: RequestDelegate, logger : ILogger<RequestLoggerMiddleware>) =
        let nl = Environment.NewLine

        let tryGetHeader (request : HttpRequest) name =
            if request.Headers.ContainsKey(name)
            then request.Headers.[name] |> sprintf "%O" |> Some
            else None

        let printHeaders (headers : IHeaderDictionary) =
            let printHeader (header : KeyValuePair<string,Microsoft.Extensions.Primitives.StringValues>) =
                let key = header.Key
                let value = sprintf "%O" header.Value
                sprintf "%s:%s" key value
            Seq.map printHeader headers
            |> Seq.fold (fun state line -> state + nl + line) ""

        let formatRequest(request: HttpRequest) =
            task {
                request.EnableBuffering()

                let body = request.Body
                let reader = new IO.StreamReader(body, Text.Encoding.UTF8, true,  1024, true)
                let! bodyAsText = reader.ReadToEndAsync()
                body.Seek(0L, SeekOrigin.Begin) |> ignore
                request.Body <- body

                return sprintf """--- Request In ---%s%O %s %s%s%s%s%s"""
                        nl
                        request.Method (UriHelper.GetDisplayUrl(request)) request.Protocol
                        (printHeaders request.Headers)
                        nl
                        nl
                        bodyAsText }

        let formatResponse (response: HttpResponse) =
            task {
                response.Body.Seek(0L, SeekOrigin.Begin) |> ignore
                let! content = (new StreamReader(response.Body)).ReadToEndAsync()
                response.Body.Seek(0L, SeekOrigin.Begin) |> ignore

                return sprintf """--- Response Out ---%s%i %s %s%s%s%s%s"""
                        nl
                        response.StatusCode (UriHelper.GetDisplayUrl(response.HttpContext.Request)) response.HttpContext.Request.Protocol
                        (printHeaders response.Headers)
                        nl
                        nl
                        content }

        member __.Invoke(context: HttpContext) =
            task {

                let! formattedRequest = formatRequest(context.Request)
                let originalBodyStream = context.Response.Body
                use  responseBody = new MemoryStream()
                context.Response.Body <- responseBody

                do! next.Invoke(context)

                let! formattedResponse = formatResponse(context.Response)
                logger.LogTrace(sprintf "%s%s%s%s%s" formattedRequest nl nl nl formattedResponse)

                do! responseBody.CopyToAsync(originalBodyStream)
            }

    type IApplicationBuilder with
        member this.UseRequestLogger() =
            this.UseMiddleware<RequestLoggerMiddleware>()


