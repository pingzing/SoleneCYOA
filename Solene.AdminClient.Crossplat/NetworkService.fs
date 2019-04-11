namespace Solene.AdminClient.Crossplat

open FSharp.Data
open Newtonsoft.Json
open Solene.Models
open System

module NetworkService =

    let inline mapAsync f x = async.Bind(x, f >> async.Return)

    type PlayersAndDetails = {
        AllPlayers : Player []
        AllQuestions : AdminQuestion []
    }

    let url path code =
        let uri = UriBuilder(
                    Scheme="https",
                    Host="solene.azurewebsites.net",
                    Path="/api/" + path,
                    Query=sprintf "code=%s" code)
        uri.Uri

    let getAllProfiles = async {
        let! codes = FunctionCodes.Codes.Instance
        let u = url "players-and-details" codes.GetAllPlayersAndQuestionsFunctionCode 
        let! result = u.ToString()
                        |> Http.AsyncRequestString
                        |> mapAsync JsonConvert.DeserializeObject<PlayersAndDetails>
        return result;
    }                