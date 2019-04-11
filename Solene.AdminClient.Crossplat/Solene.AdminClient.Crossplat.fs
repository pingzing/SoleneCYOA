// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace Solene.AdminClient.Crossplat

open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms
open Xamarin.Forms.Internals
open Solene.Models

module App =     

    let inline last (arr:_[]) = arr.[arr.Length - 1]

    type Model = { 
        Profiles : AdminPlayerProfile []
        SelectedProfile : AdminPlayerProfile option
    }

    type Msg = 
    | ListViewSelectedItemChanged of int option
    | AddQuestion of AdminQuestion
    | UpdateProfiles of Model
    | GetRemoteProfiles

    let getAdminPlayerProfiles : Async<AdminPlayerProfile[]> = async {
        let! allProfiles = NetworkService.getAllProfiles
        return allProfiles.AllPlayers 
            |> Array.map (fun profile -> {
                PlayerInfo=profile; 
                Questions=allProfiles.AllQuestions 
                    |> Array.filter (fun question -> question.PlayerId = profile.Id)})
        }

    let getProfileName (selectedProfile: AdminPlayerProfile option) =
        selectedProfile |> Option.fold (fun _ profile -> profile.PlayerInfo.Name) ""

    let getProfileId (selectedProfile: AdminPlayerProfile option) =
        selectedProfile |> Option.fold (fun _ profile -> profile.PlayerInfo.Id.ToString()) ""

    let getProfileQuestions (selectedProfile: AdminPlayerProfile option) =
        selectedProfile |> Option.fold (fun _ profile -> profile.Questions) [||]

    let getInitialProfiles : Cmd<Msg> =
       Cmd.ofMsg (UpdateProfiles {Profiles=[||]; SelectedProfile=Option.None;})

    let init () : Model * Cmd<Msg> = {Profiles=[||]; SelectedProfile=Option.None;}, getInitialProfiles

    let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
        match msg with
        | UpdateProfiles updatedModel -> updatedModel, Cmd.none
        | GetRemoteProfiles -> model, Cmd.ofAsyncMsg(async {
            let! remoteProfiles = getAdminPlayerProfiles
            let remoteProfiles = remoteProfiles                                 
                                |> Array.map (fun profile -> 
                                    {Questions = profile.Questions |> Array.sortBy (fun q -> 
                                        q.SequenceNumber); PlayerInfo = profile.PlayerInfo})
                                |> Array.sortByDescending (fun profile -> (last profile.Questions).UpdatedTimestamp)
            return UpdateProfiles {Profiles = remoteProfiles; SelectedProfile = Option.None;}
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
                                                    yield View.StackLayout(spacing = 0.0, padding = 5.0, children = [
                                                            View.StackLayout(orientation = StackOrientation.Horizontal, children = [
                                                                View.Label profile.PlayerInfo.Name
                                                                View.Label ((last profile.Questions).UpdatedTimestamp.ToString("g"))
                                                            ])
                                                            View.Label profile.PlayerInfo.Gender
                                                            View.Label (profile.PlayerInfo.Id.ToString(), textColor = Color.SlateGray)
                                                       ])
                                                  ],
                                        itemSelected = (fun index -> dispatch(ListViewSelectedItemChanged index)));
                        ])
                ),
            detail = 
                 View.ContentPage(
                    title = getProfileName model.SelectedProfile,
                    content = View.Grid(
                        rowdefs=["auto"; "*"],
                        rowSpacing = 0.0,
                        children = [
                            // InfoBar                            
                            View.Label(getProfileId model.SelectedProfile, fontSize = 16, margin = Thickness(5.0)).GridRow(0);
                            // List of items with footer 
                            View.ListView(                                
                                selectionMode = ListViewSelectionMode.None,
                                items = [for question in getProfileQuestions model.SelectedProfile do
                                            yield View.StackLayout(spacing = 0.0, padding = Thickness(10.0, 0.0), children = [
                                                View.StackLayout([View.Label (question.SequenceNumber.ToString()); View.Label question.Title;], orientation = StackOrientation.Horizontal);
                                                View.Label(question.Id.ToString("N"), fontSize = 12);
                                                View.Label question.Text;
                                                View.Label(question.ChosenAnswer, textColor = Color.Orange, fontAttributes = FontAttributes.Bold);
                                                View.FlexLayout(
                                                    direction = FlexDirection.Row,
                                                    wrap = FlexWrap.Wrap,
                                                    children = [for option in question.PrefilledAnswers do
                                                                    yield View.Button(option, isEnabled = false, margin = Thickness(0.0, 0.0, 5.0, 0.0))
                                                ]);
                                                View.BoxView(color = Color.Gray, horizontalOptions = LayoutOptions.FillAndExpand, heightRequest = 1.0, margin = Thickness(0.0, 5.0))
                                            ])
                                        ],
                                footer = "AddQuestionControl placeholder"//AddQuestionControl.view {title=""} (AddQuestion >> dispatch)
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


