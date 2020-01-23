open System
open System.Globalization
open System.IO

open FParsec

open Saturn
open Giraffe

open FSharp.Control.Tasks.V2.ContextInsensitive

type CodePoint =
    {
        Address : int
        Name : string
    }

let getCodePointAnalyzer () =

    let text = 
        use wc = new System.Net.Http.HttpClient()
        wc.GetStringAsync("https://www.unicode.org/Public/12.0.0/ucd/UnicodeData.txt").Result

    let p : Parser<CodePoint, unit> = 
        let charToNum = function
            | '0' -> 0
            | '1' -> 1
            | '2' -> 2
            | '3' -> 3
            | '4' -> 4
            | '5' -> 5
            | '6' -> 6
            | '7' -> 7
            | '8' -> 8
            | '9' -> 9
            | 'A' -> 10
            | 'B' -> 11
            | 'C' -> 12
            | 'D' -> 13
            | 'E' -> 14
            | 'F' -> 15
        // 003D;EQUALS SIGN;Sm;0;ON;;;;;N;;;;;
        pipe2 
            (many1Chars (hex) .>> skipChar ';' |>> (fun hs -> hs |> Seq.fold (fun acc c -> acc * 16 + (charToNum c)) 0))
            (many1Chars (noneOf [';']))
            (fun a b -> { Address = a; Name = b })

    let cps =
        FParsec.CharParsers.run (sepEndBy p (skipRestOfLine true)) (text.Trim ()) |> function Success (c, _, _) -> c | Failure (msg, _, _) -> failwith msg
        |> Seq.map (fun c -> c.Address, c)
        |> readOnlyDict

    fun text ->
        let info = text |> StringInfo
        seq {
            for i = 0 to (info.LengthInTextElements - 1) do
                info.SubstringByTextElements(i, 1)
        }
        |> Seq.collect (fun c -> 
            match c.ToCharArray () with
            | [| c |] -> [ cps.[c |> int] ]
            | [| hi; lo |] ->
                let hi = hi |> int
                let lo = lo |> int
                if hi >= 0xD800 && hi <= 0xDBFF && lo >= 0xDC00 && lo <= 0xDFFF then // Surrogate Pair
                    let cp =
                        (hi - 0xD800) * 0x0400
                        + (lo - 0xDC00)
                        + 0x10000
                    [ cps.[cp] ]
                else
                    [
                        cps.[hi]
                        cps.[lo]
                    ]
        )


let top getCodePoints = router {
    post "/api" (fun next ctx ->
        task {
            let! text = ctx.BindJsonAsync ()
            let r = text |> getCodePoints
            return! json r next ctx
        }
    )
    getf "/api/%s" (getCodePoints >> json)
}

let clientPath = @"..\..\public" |> DirectoryInfo
let app getCodePoints = application {
    use_router (top getCodePoints)
    url "http://[::1]:8080"
    use_static clientPath.FullName
    use_gzip
}

[<EntryPoint>]
let main _ =
    let getCodePoints = getCodePointAnalyzer ()
    getCodePoints |> app |> run
    0
