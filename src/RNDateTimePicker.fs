module RNDateTimePicker

(*

    Implements a binding in order to be able to use the npm-package 'react-native-datepicker' in our app

*)

open System
open Fable.Core
open Fable.Import
open Fable.Import.React
open Fable.Helpers
open Fable.Core.JsInterop
open Fable.Helpers.React

type IRNDateTimePickerProperties = 
    inherit React.Props<IRNDateTimePickerStatic>
and IRNDateTimePickerStatic = 
    inherit React.ComponentClass<IRNDateTimePickerStatic>
and RNDateTimePicker = 
    RNDateTimePickerStatic

type Globals = 
    //[<Import("DatePicker", "react-native-datepicker")>] 
    [<Import("default", from="react-native-datepicker")>]
    static member RNDateTimePicker with get() : RNDateTimePicker = jsNative and set( v : RNDateTimePicker ): unit = jsNative


type IRNDateTimePickerProps = 
    interface end 

[<StringEnum>]
type DatePickerMode =
| [<CompiledName("date")>] Date
| [<CompiledName("datetime")>] Datetime
| [<CompiledName("time")>] Time

[<RequireQualifiedAccess>]
type Props = 
    | [<CompiledName("date")>] Date of DateTime
    | [<CompiledName("onDateChange")>] OnDateChange of ( DateTime -> unit )
    | [<CompiledName("format")>] Format of string
    | [<CompiledName("mode")>] Mode of DatePickerMode
    | MinDate of string
    | MaxDate of string
    | ConfirmBtnText of string
    | CancelBtnText of string
        interface IRNDateTimePickerProps

let inline rnDateTimePicker ( props : IRNDateTimePickerProps list ) ( children : React.ReactElement list ) = 
    React.createElement( Globals.RNDateTimePicker, keyValueList CaseRules.LowerFirst props, children )