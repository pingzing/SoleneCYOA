namespace Solene.AdminClient.Crossplat

open Fabulous.Core
open Fabulous.DynamicViews
open Solene.Models
open Xamarin.Forms


module PlayerProfilePage =

    type Model = { 
        Player: Player
        Questions: AdminQuestion list
    }

    type Msg = 
    | AddQuestion of AdminQuestion

    let update msg (model: Model) =
        match msg with 
        | AddQuestion newQuestion ->
            {model with Questions = List.append model.Questions [newQuestion] }, Cmd.none
    
    let view (model: Model) dispatch =
        View.ContentPage(
            title = "Player View",
            content = View.Grid(
                rowdefs=["auto"; "*"],
                children = [
                    // InfoBar
                    View.Label(model.Player.Name).GridRow(0);
                    // List of items with footer 
                    View.ListView(
                        selectionMode = ListViewSelectionMode.None,
                        items = [for question in model.Questions do
                                    yield View.StackLayout(children = [
                                        View.Label (question.SequenceNumber.ToString());
                                        View.Label question.Title;
                                        View.Label question.Text;]
                                    )],
                        footer = AddQuestionControl.view {title=""} (AddQuestion >> dispatch)
                    ).GridRow(1);
                ])
        )
