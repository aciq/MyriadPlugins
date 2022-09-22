namespace Aciq.MyriadPlugins.Test.Sample1

open Aciq.MyriadPlugins.Attribute

[<Generator.SetFieldByName "conf1">]
type PluginSettings =
    {
        defaultCodeBlockLanguage : string
        defaultCalloutType : string
        something: int
    }

//------------------------------------------------------------------------------
//        This code was generated by myriad.
//        Changes to this file will be lost when the code is regenerated.
//------------------------------------------------------------------------------
namespace rec MyNamespace

module PluginSettings =
    open Aciq.MyriadPlugins.Test.Sample1

    let setFieldByName (key: string) (value: _) (x: PluginSettings) =
        match key with
        | "defaultCodeBlockLanguage" -> { x with defaultCodeBlockLanguage = unbox value }
        | "defaultCalloutType" -> { x with defaultCalloutType = unbox value }
        | "something" -> { x with something = unbox value }
        | _ -> failwith "unimplemented case"

