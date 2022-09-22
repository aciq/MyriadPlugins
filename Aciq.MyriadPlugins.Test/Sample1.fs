namespace Aciq.MyriadPlugins.Test.Sample1

open Aciq.MyriadPlugins.Attribute

[<Generator.SetFieldByName "conf1">]
type PluginSettings =
    {
        defaultCodeBlockLanguage : string
        defaultCalloutType : string
        setting2 : bool
        setting4 : bool
        newSetting : string
        newSetting2 : string
    }




