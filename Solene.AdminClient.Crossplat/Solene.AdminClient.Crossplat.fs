// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace Solene.AdminClient.Crossplat

open System.Diagnostics
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms
open Xamarin.Forms.Internals
open Solene.Models

module App =     

    type Model = { 
        Profiles : AdminPlayerProfile []
        SelectedProfile : AdminPlayerProfile option
    }

    type Msg = 
    | ListViewSelectedItemChanged of int option
    | AddQuestion of AdminQuestion
    | UpdateProfiles of Model
    | GetRemoteProfiles

    let getAdminPlayerProfiles : Async<AdminPlayerProfile[]> = 
         async {
            let! allProfiles = NetworkService.getAllProfiles
            return allProfiles.AllPlayers 
                |> Array.map (fun profile -> {
                    PlayerInfo=profile; 
                    Questions=allProfiles.AllQuestions 
                        |> Array.filter (fun question -> question.PlayerId = profile.Id)})
         }

    let getProfileName (selectedProfile: AdminPlayerProfile option) =
        selectedProfile |> Option.fold (fun _ profile -> profile.PlayerInfo.Name) ""

    let getProfileQuestions (selectedProfile: AdminPlayerProfile option) =
        selectedProfile |> Option.fold (fun _ profile -> profile.Questions) [||]

    let getInitialProfiles : Cmd<Msg> =
       Cmd.ofMsg (UpdateProfiles {Profiles=[||]; SelectedProfile=Option.None})

    let init () : Model * Cmd<Msg> = {Profiles=[||]; SelectedProfile=Option.None}, getInitialProfiles

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | UpdateProfiles initialModel -> initialModel, Cmd.none
        | GetRemoteProfiles -> model, Cmd.ofAsyncMsg(async {
                let! profiles = getAdminPlayerProfiles
                return UpdateProfiles { Profiles = profiles; SelectedProfile = Option.None }
            })
        | ListViewSelectedItemChanged index -> {
            model with SelectedProfile = 
                        index |> Option.map (fun idx -> Array.item idx model.Profiles)}, Cmd.none
        | AddQuestion newQuestion ->
            let selectedProfile = {
                    PlayerInfo = model.SelectedProfile.Value.PlayerInfo; 
                    Questions = Array.append model.SelectedProfile.Value.Questions [|newQuestion|]} |> Option.Some
            {model with SelectedProfile = selectedProfile}, Cmd.none                        

    let view (model: Model) dispatch =        
        View.MasterDetailPage(
            masterBehavior=MasterBehavior.SplitOnLandscape,
            title = "Solene Admin Client",
            isPresented = (Device.Info.CurrentOrientation.IsPortrait() && (Option.isSome model.SelectedProfile)),
            master = 
                View.ContentPage(title="Players",
                    content = 
                        View.StackLayout(children = [
                            View.Button("LoadProfiles", command = (fun () -> dispatch (GetRemoteProfiles)));
                            View.ListView(items = [ for profile in model.Profiles do
                               yield View.StackLayout(children=
                                   [View.Label profile.PlayerInfo.Name
                                    //View.Label (profile.Questions.[profile.Questions.Length - 1].UpdatedTimestamp.ToString("g"))
                                    View.Label profile.PlayerInfo.Gender
                                    View.Label (profile.PlayerInfo.Id.ToString())])],
                            itemSelected = (fun index -> dispatch(ListViewSelectedItemChanged index)));
                        ])
                ),
            detail = 
                 View.ContentPage(
                    title = "Player View",
                    content = View.Grid(
                        rowdefs=["auto"; "*"],
                        children = [
                            // InfoBar
                            View.Label(getProfileName model.SelectedProfile).GridRow(0);
                            // List of items with footer 
                            View.ListView(
                                selectionMode = ListViewSelectionMode.None,
                                items = [for question in getProfileQuestions model.SelectedProfile do
                                            yield View.StackLayout(children = [
                                                View.Label (question.SequenceNumber.ToString());
                                                View.Label question.Title;
                                                View.Label question.Text;]
                                            )],
                                footer = AddQuestionControl.view {title=""} (AddQuestion >> dispatch)
                            ).GridRow(1);
                        ])
                )
        )   
    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgram init update view

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> Program.runWithDynamicView app

#if DEBUG
    // Uncomment this line to enable live update in debug mode. 
    // See https://fsprojects.github.io/Fabulous/tools.html for further  instructions.
    //
    //do runner.EnableLiveUpdate()
#endif    

    // Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
    // See https://fsprojects.github.io/Fabulous/models.html for further  instructions.
#if APPSAVE
    let modelId = "model"
    override __.OnSleep() = 

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


