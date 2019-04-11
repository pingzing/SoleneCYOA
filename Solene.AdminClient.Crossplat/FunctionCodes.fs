namespace Solene.AdminClient.Crossplat

open Plugin.FilePicker
open System
open System.Threading.Tasks
open Xamarin.Forms

module FunctionCodes =   

    [<Literal>]
    let defaultKey = "defaultKey"

    let memoizeAsync f =
        let cache = System.Collections.Concurrent.ConcurrentDictionary<'a, Task<'b>>()
        fun x ->
            cache.GetOrAdd(x, fun x -> f(x) |> Async.StartAsTask) |> Async.AwaitTask

    let openFileAsync = async {
        let tcs = new TaskCompletionSource<Async<Abstractions.FileData>>()

        Device.BeginInvokeOnMainThread(fun () ->     
            async {
                let fileResult = CrossFilePicker.Current.PickFile() |> Async.AwaitTask
                tcs.SetResult(fileResult)
            }   |> Async.StartImmediate         
        )
        let! data = tcs.Task |> Async.AwaitTask
        return! data
    }

    type Codes() =  
        static let openFileKeyed key = openFileAsync
        static let getIoData = memoizeAsync openFileKeyed defaultKey
        static let readFile key = async {
            let! pickedFile = getIoData
            if pickedFile = null then ()
            let contents = System.Text.Encoding.UTF8.GetString(pickedFile.DataArray)
            let lines = contents.Split([|Environment.NewLine|], StringSplitOptions.RemoveEmptyEntries)  
            return Codes(
                CreatePlayerFunctionCode=lines.[0],
                DeletePlayerFunctionCode=lines.[1],
                GetPlayerFunctionCode=lines.[2],
                GetAllPlayersFunctionCode=lines.[3],
                GetPlayerQuestionsFunctionCode=lines.[4],
                AddQuestionFunctionCode=lines.[5],
                GetAllPlayersAndQuestionsFunctionCode=lines.[6])
        }

        static let codes = memoizeAsync readFile defaultKey
        static member Instance = codes

        member val public CreatePlayerFunctionCode = "" with get, set
        member val public DeletePlayerFunctionCode = "" with get, set
        member val public GetPlayerFunctionCode = "" with get, set
        member val public GetAllPlayersFunctionCode = "" with get, set
        member val public GetPlayerQuestionsFunctionCode = "" with get, set
        member val public AddQuestionFunctionCode = "" with get, set
        member val public GetAllPlayersAndQuestionsFunctionCode = "" with get, set