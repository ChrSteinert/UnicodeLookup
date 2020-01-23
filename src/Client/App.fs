module App

open Elmish
open Elmish.React
open Fable.Core.JsInterop
open Fable.React
open Fable.React.Props
open Fulma

type CodePoint =
    {
        Address : int
        Name : string
    }

type Model = 
    {
        Input : string
        CodePoints : CodePoint list
        ErrorText : string
    }

type Msg =
    | GetChars of string
    | CharsReady of CodePoint list
    | Error of exn

let init() = { Input = ""; CodePoints = List.empty; ErrorText = "" }, Cmd.none

let update (msg:Msg) (model:Model) =
    match msg with
    | GetChars c -> 
        if System.String.IsNullOrEmpty c then { model with Input = ""; CodePoints = List.empty; ErrorText = "" }, Cmd.none
        else { model with Input = c }, Cmd.OfPromise.either (fun c -> Thoth.Fetch.Fetch.post("/api", c, isCamelCase = true)) c (CharsReady) (Error)
    | CharsReady c -> { model with CodePoints = c; ErrorText = "" }, Cmd.none
    | Error e -> { model with CodePoints = List.empty; ErrorText = e.Message }, Cmd.none

let view (model:Model) dispatch =
    let viewTable = 
        Table.table [ Table.IsStriped; Table.IsFullWidth; Table.IsHoverable; Table.IsBordered; Table.Props [ Props.Hidden (model.CodePoints.Length = 0) ] ]
            [
                thead [] [ tr [] [ th [] [ str "Codepoint" ]; th [] [ str "Name" ] ]]
                tbody []
                    (model.CodePoints
                    |> Seq.map (fun c -> 
                        tr [] 
                            [
                                td [] [ c.Address.ToString "X4" |> str]
                                td [] [ c.Name |> str ]
                            ]
                    ))       
            ]

    let viewError = 
        Container.container [ Container.Props [ Hidden (model.ErrorText.Length = 0) ]; Container.IsFluid ]
            [
                Message.message [ Message.Color IsDanger ]
                    [
                        Message.header [] [ sprintf "Search for '%s' failed" model.Input |> str ]
                        Message.body [] [ model.ErrorText |> str ]
                    ]
            ]

    let viewContent =
        [
            Columns.columns []
                [
                    Column.column []
                        [
                            Input.text [ 
                                Input.Props 
                                    [ 
                                        OnChange (fun c -> dispatch (GetChars !!c.target?value))
                                    ]
                            ] 
                        ]
                ]
            Columns.columns []
                [
                    Column.column []
                        [
                            viewTable
                            viewError
                        ]
                ]
        ]
            
                    
            
    div []
        [
            Hero.hero [ Hero.Color IsPrimary; Hero.IsBold ]
                [
                    Hero.body []
                        [
                            Container.container [] [ Heading.h1 [] [ str "Unicode Lookup" ] ]
                        ]
                ]   

            Section.section [] viewContent
        ]
        
Program.mkProgram init update view
|> Program.withReactSynchronous "elmish-app"
|> Program.withConsoleTrace
|> Program.run
