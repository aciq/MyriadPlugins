module Aciq.MyriadPlugins.Attribute


open System
open Myriad.Core

[<RequireQualifiedAccess>]
module Generator =
    
    /// <summary>
    /// [toml options]  <br/>
    /// namespace : string  <br/>
    /// </summary>
    [<AttributeUsage(AttributeTargets.Struct)>]
    type SetFieldByNameAttribute(configGroup: string)  =
        inherit Attribute()

    let getConfigFromAttribute<'a> (configGetter: string -> seq<string * obj>) typeDef =
        match Ast.getAttribute<'a> typeDef with
        | None -> Seq.empty
        | Some a ->
            match Ast.getAttributeConstants a with
            | [] -> Seq.empty
            | [ name ] -> configGetter name
            | [ name; _ ] -> configGetter name
            | others -> failwithf $"More than two constants are not yet supported: %A{others}"
