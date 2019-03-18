// Copyright 2018 Fabulous contributors. See LICENSE.md for license.
namespace Solene.AdminClient.Crossplat

open System.Diagnostics
open Fabulous.Core
open Fabulous.DynamicViews
open Xamarin.Forms
open Xamarin.Forms.Internals

module App = 

    type Model = { 
        Profiles : AdminPlayerProfile list
        SelectedProfile : AdminPlayerProfile option
        PlayerProfileModel : PlayerProfilePage.Model       
    }

    type Msg =     
    | ListViewSelectedItemChanged of int option
    | PlayerProfileMsg of PlayerProfilePage.Msg

    let init () = 
        let playerProfileInitModel, playerProfileInitCmd = PlayerProfilePage.init ()

        let initModel = { 
            SelectedProfile = None 
            Profiles = []        
        }

        initModel, Cmd.none

    let update msg model =
        match msg with
        | ListViewSelectedItemChanged index -> {
            model with SelectedProfile = 
                        index |> Option.map (fun idx -> List.item idx model.Profiles)}, Cmd.none
        | PlayerProfileMsg msg -> 
            
            

    let view (model: Model) dispatch =
        View.MasterDetailPage(
            masterBehavior=MasterBehavior.SplitOnLandscape,
            title = "Solene Admin Client",
            isPresented = (Device.Info.CurrentOrientation.IsPortrait() && (Option.isSome model.SelectedProfile)),
            master = 
                View.ContentPage(title="Players",
                    content = 
                        View.ListView(items = [ View.Label "Item 1"; 
                                                View.Label "Item 2";],
                            itemSelected = (fun index -> dispatch(ListViewSelectedItemChanged index)))),
            detail = 
                PlayerProfilePage.view { Player = model.SelectedProfile.Value.PlayerInfo; Questions = model.SelectedProfile.Value.Questions } (PlayerProfileMsg >> dispatch)
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


