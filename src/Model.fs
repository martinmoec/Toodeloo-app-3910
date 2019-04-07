module Toodeloo.Model

open System
open Elmish
open Fable.PowerPack
open Fable.PowerPack.Fetch
open Fable.Core.JsInterop

open Toodeloo.Constants

type Todo = { 
    title       : string
    description : string 
    priority    : int
    due         : System.DateTime option
}

let defaultTodo = { 
    title       = ""
    description = ""
    priority    = 0
    due         = None
}

type Model = { 
    entries         : Map<int, Todo>
    createForm      : Todo option
    editForm        : (int * Todo) option
    errorMsg        : string option
    showInfoPane    : bool
}

module Defaults =
    let defaultModel = {
        entries = Map.empty
        createForm = None
        errorMsg = None
        editForm = None
        showInfoPane = false
    }

// Example how to split Msg into submessages
type UpdateEntryMsg =
| UpdatePri         of int
| UpdateTitle       of string
| UpdateDescription of string
| UpdateDue         of System.DateTime option

type NewEntryMsg = UpdateEntryMsg

type EditEntryMsg = UpdateEntryMsg

type Msg =
| NewEntry          of NewEntryMsg
| SaveEntry         of Todo
| DeleteEntry       of int
| StartEdit         of int
| EditEntry         of EditEntryMsg 
| SaveEdit 
| CancelEdit 
| NotifyError       of string
| ClearError
| ToggleInfoPane
| EntrySaved        of Result<int * Todo, Exception>
| EntriesLoadedOk   of (int * Todo) array
| EntriesLoadedErr  of Exception
| Ignore 

let private notifyErr e = Cmd.ofMsg (Msg.NotifyError e)

let private notifyExn (e : Exception) = Cmd.ofMsg (Msg.NotifyError e.Message)

let initEntries model (todos : Result<(int * Todo) array, Exception>) =
    match todos with 
    | Ok e ->
        { model with 
            entries = Map.ofArray e
        }, Cmd.none
    | Error ex -> 
        model, notifyExn ex 

let handleNewEntry (msg : NewEntryMsg) (model : Model) =
    let form = 
        match model.createForm with
        | Some f -> f
        | None -> { 
            title = ""
            description = ""
            priority = 1
            due = None
            }
    let entry = 
        match msg with
        | UpdatePri y -> { form with priority = y }
        | UpdateDue y ->  { form with due = y }
        | UpdateTitle y -> { form with title = y }
        | UpdateDescription y -> { form with description = y }
    { model with createForm = Some entry }, Cmd.none

let loadEntries (model : Model) =
    let p () =
        let requestPath = baseAddress + "/api/load" 
        promise {
            return! fetchAs<(int * Todo) array> requestPath []
        }
    model, Cmd.ofPromise p () EntriesLoadedOk  EntriesLoadedErr

let saveEntry (x : Todo) (model : Model) =
    let p () =
        let bdy = toJson x
        let props = [
            RequestProperties.Method HttpMethod.POST
            RequestProperties.Body !^bdy
        ]
        let requestPath = baseAddress + "/api/save" 
        promise {
            return! fetchAs<int * Todo> requestPath props
        }
    model, Cmd.ofPromise p () (Ok >> EntrySaved) (Error >> EntrySaved)

let entrySaved (e : Result<int * Todo, System.Exception>) model =
        match e with
        | Ok (id, entry) ->
            let todo' = model.entries |> Map.add id entry 
            let model' = { 
                model with 
                    entries = todo' 
                    createForm = None
                }
            model', Cmd.none 
        | Error err -> model, Cmd.ofMsg (NotifyError err.Message)        

let deleteEntry (x : int) (model : Model) =
    let model' = { model with entries = Map.remove x model.entries }
    let requestPath = sprintf "%s/api/delete/%i" baseAddress x
    let p () = promise {
            return! fetchAs<int option> requestPath []
        }
    model', Cmd.ofPromise p () (fun _ -> Ignore) (string >> NotifyError)

let private updateEntry (msg : EditEntryMsg) (entry : Todo) =
    match msg with
    | UpdateTitle t -> { entry with title = t}
    | UpdateDescription t -> { entry with description = t}
    | UpdatePri p -> { entry with priority = p}
    | UpdateDue d -> { entry with due = d}

let startEdit id model =
    match Map.tryFind id model.entries with
    | Some entry -> 
        let model' = { model with editForm = Some (id, entry) }
        model', Cmd.none
    | None -> model, notifyErr <| "TaskId not found, " + string id

let handleEditEntry (msg : UpdateEntryMsg) (model : Model) =
    match model.editForm with
    | Some (id, entry) -> 
        let entry' = updateEntry msg entry
        let model' = { model with editForm = Some (id, entry') }
        model', Cmd.none
    | None -> model, notifyErr "Error in entry editor"

let saveEdit model =
    match model.editForm with
    | Some (id, entry) -> 
        let requestPath = sprintf "%s/api/update/%i" baseAddress id
        let bdy = toJson entry
        let props = [
            RequestProperties.Method HttpMethod.POST
            RequestProperties.Body !^bdy
        ]
        let p () = promise {
                return! fetchAs<int option> requestPath props
            }
        let model' = {
            model with 
                entries = Map.add id entry model.entries
                editForm = None 
            }
        model', Cmd.ofPromise p () (fun _ -> Ignore) (string >> NotifyError)
    | None -> model, notifyErr "Error saving edited entry"

let cancelEdit model =
    let model' = { model with editForm = None }
    model', Cmd.none