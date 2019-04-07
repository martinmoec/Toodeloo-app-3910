module Toodeloo.Client

open Elmish
open Fable.Helpers.ReactNative

open Elmish.React
open Elmish.HMR

open Toodeloo.Model
open Toodeloo.View

let init () : Model * Cmd<Msg> =
    loadEntries Defaults.defaultModel

let update (msg : Msg) (model : Model) : Model * Cmd<Msg> = 
    match msg with
    | NewEntry msg -> handleNewEntry msg model
    | SaveEntry n -> saveEntry n model 
    | EditEntry msg -> handleEditEntry msg model 
    | StartEdit id -> startEdit id model 
    | SaveEdit -> saveEdit model
    | CancelEdit -> cancelEdit model 
    | DeleteEntry n -> deleteEntry n model 
    | NotifyError err -> { model with errorMsg = Some err }, Cmd.none
    | ClearError -> { model with errorMsg = None }, Cmd.none
    | ToggleInfoPane -> 
        { model with showInfoPane = not model.showInfoPane }, Cmd.none
    | EntrySaved e -> entrySaved e model
    | EntriesLoadedOk entries -> initEntries model (entries |> Ok)
    | EntriesLoadedErr entries -> initEntries model (entries |> Error)
    | Ignore -> model, Cmd.none

let view model dispatch =
    mainView model dispatch [] 

open Elmish.ReactNative

// App
Program.mkProgram init update view
|> Program.withConsoleTrace
|> Program.withHMR
|> Program.withReactNative "ReactNativeApp"
|> Program.run
