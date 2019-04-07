module Toodeloo.View

open System
open Toodeloo.Model


// rendering views with ReactNative
module R = Fable.Helpers.ReactNative
module P = Fable.Helpers.ReactNative.Props
open Fable.Helpers.ReactNative.Props
open RNDateTimePicker

let navbar =
    // simple solution to avoid the iOS camera blocking part of the view
    let padding = 
        if R.RN.Platform.OS = Fable.Import.ReactNative.Ios then
            6.
        else
            0.

    R.view [
            P.ViewProperties.Style [
                P.BackgroundColor "#333333"
                P.FlexStyle.Padding ( R.dip 10. ) 
            ]
        ] [
            R.text [
                P.TextProperties.Style [
                    P.Color "#f6f6f6"
                    P.FontSize 25.
                    P.FlexStyle.Top ( R.pct 6. )
                    P.FlexStyle.Left ( R.dip 5. )
                    P.TextAlign P.TextAlignment.Center
                    P.PaddingTop ( R.pct padding )
                ]
            ] "Toodeloo"
        ]

let baseInputStyle : P.IStyle list = 
    [
        P.BorderBottomColor "#aaaaaa"
        P.BorderBottomWidth 3.
        P.FontSize 25.
        P.FlexStyle.MinWidth ( R.pct 75. )
    ]

let addView ( model : Model ) ( dispatch : Msg -> unit ) = 
    let dispatch' = NewEntry >> dispatch
    
    let dateVal = 
        match model.createForm with
        | ( Some cf ) when cf.due.IsSome -> cf.due.Value
        | _ -> System.DateTime.Now 
    
    R.view [
        P.ViewProperties.Style [
            P.BackgroundColor "#e6e6e6"
            P.FlexStyle.Flex 0.7
            P.FlexStyle.Padding ( R.pct 5. )
            P.JustifyContent JustifyContent.SpaceBetween
            P.AlignItems ItemAlignment.Center
            P.BorderBottomColor "#aaaaaa"
            P.BorderBottomWidth 2.
        ]
    ] [
        R.text [
            P.TextProperties.Style [
                P.FontSize 25.
            ]
        ] "Add Todo"
        
        R.textInput [
            P.TextInput.TextInputProperties.Style baseInputStyle
            P.TextInput.TextInputProperties.Placeholder "Title"
            P.TextInput.TextInputProperties.KeyboardType P.KeyboardType.Default
            P.TextInput.TextInputProperties.OnChangeText ( UpdateTitle >> dispatch' )
        ]

        R.textInput [
            P.TextInput.TextInputProperties.Style baseInputStyle
            P.TextInput.TextInputProperties.Placeholder "Description"
            P.TextInput.TextInputProperties.KeyboardType P.KeyboardType.Default
            P.TextInput.TextInputProperties.OnChangeText ( UpdateDescription >> dispatch' )
        ]

        rnDateTimePicker [
            RNDateTimePicker.OnDateChange( fun dt -> dispatch' ( UpdateDue ( Some dt ) ) )
            RNDateTimePicker.Props.Mode DatePickerMode.Date
            RNDateTimePicker.Props.Date dateVal
            RNDateTimePicker.Props.MinDate ( DateTime.Now.ToString "yyyy-MM-dd" )
            RNDateTimePicker.Props.ConfirmBtnText "Confirm"
            RNDateTimePicker.Props.CancelBtnText "Cancel"
        ] []
        
        R.text [
            P.TextProperties.Style [
                P.Color "#333333"
                P.TextAlign P.TextAlignment.Center
                P.FontSize 25.
            ]
        ] "Add entry"
        |> R.touchableHighlightWithChild [
            P.TouchableHighlightProperties.Style [
                    P.BackgroundColor "#00d1b2"
                    P.FlexStyle.Padding ( R.dip 10. )
                    P.BorderRadius 4.
                    
            ]
            P.TouchableHighlightProperties.UnderlayColor "#5499C4"
            OnPress ( fun _ -> 
                match model.createForm with
                | Some cf -> dispatch ( SaveEntry cf )
                | None -> () )]
        ]

let rowTextStyle flx : IStyle list = [
        P.FlexStyle.Flex flx
        P.FontSize 15.
        P.TextAlign P.TextAlignment.Center
        P.FlexStyle.AlignSelf Alignment.Center

    ]

let todoEntry todoId title desc due = 
    R.view [
        P.ViewProperties.Style [
            P.FlexStyle.Flex 1.0
            P.FlexStyle.FlexDirection FlexDirection.Row
            P.FlexStyle.Padding ( R.dip 10. )
            P.BorderBottomColor "#00d1b2"
            P.BorderBottomWidth 2.
            P.FlexStyle.JustifyContent JustifyContent.SpaceBetween
        ]
    ] [
        R.text [
            P.TextProperties.Style ( rowTextStyle 0.1 )
        ] todoId

        R.text [
            P.TextProperties.Style ( rowTextStyle 0.3 )
        ] title
        
        R.text [
            P.TextProperties.Style ( rowTextStyle 0.4 )
        ] desc

        R.text [
            P.TextProperties.Style ( rowTextStyle 0.2 )
        ] due
    ]


let todoListView model dispatch = 
    R.scrollView [
        P.ScrollViewProperties.Style [
            P.FlexStyle.Flex 0.3
            P.FlexStyle.PaddingTop ( R.pct 5. )
        ]
    ] [
        let dueToString ( due : System.DateTime option) = 
            match due with
            | Some dt -> dt.ToString "dd/MM/yy"
            | None -> ""

        for todoId, entry in Map.toArray model.entries do
            yield todoEntry ( string todoId ) entry.title entry.description ( dueToString entry.due )

    ]

let mainView (model : Model ) (dispatch : Msg -> unit) elt =

    R.view [
        P.ViewProperties.Style [
            P.FlexStyle.Flex 1.

        ]
    ] [
        
        navbar

        addView model dispatch
        
        todoListView model dispatch 
    ]