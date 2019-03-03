namespace Solene.AdminClient.Crossplat

open Plugin.FilePicker
open System

type FunctionCodes () =

    static let instance: Lazy<FunctionCodes> = new Lazy<FunctionCodes>(fun () -> 
        async {
            let! pickedFile = CrossFilePicker.Current.PickFile() |> Async.AwaitTask
            if pickedFile = null then ()
            let contents = System.Text.Encoding.UTF8.GetString(pickedFile.DataArray)
            let lines = contents.Split([|Environment.NewLine|], StringSplitOptions.RemoveEmptyEntries)  
            return FunctionCodes(
                CreatePlayerFunctionCode=lines.[0],
                DeletePlayerFunctionCode=lines.[1],
                GetPlayerFunctionCode=lines.[2],
                GetAllPlayersFunctionCode=lines.[3],
                GetPlayerQuestionsFunctionCode=lines.[4],
                AddQuestionFunctionCode=lines.[5],
                GetAllPlayersAndQuestionsFunctionCode=lines.[6])
        }|> Async.RunSynchronously)

    static member Instance = instance

    member val public CreatePlayerFunctionCode = "" with get, set
    member val public DeletePlayerFunctionCode = "" with get, set
    member val public GetPlayerFunctionCode = "" with get, set
    member val public GetAllPlayersFunctionCode = "" with get, set
    member val public GetPlayerQuestionsFunctionCode = "" with get, set
    member val public AddQuestionFunctionCode = "" with get, set
    member val public GetAllPlayersAndQuestionsFunctionCode = "" with get, set


        

