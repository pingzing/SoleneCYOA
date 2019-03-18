namespace Solene.AdminClient.Crossplat

open Solene.Models
open Fabulous.Core
open Fabulous.DynamicViews

module AddQuestionControl =


    type Model = { title : string }

    type Msg = 
    | AddQuestion of string

    let init () = 
        {title = ""},
        Cmd.none

    let update msg (model: Model) =
        match msg with
        | AddQuestion question ->
            {model with title = question}

    let view (model: Model) dispatch =
        View.Grid()

