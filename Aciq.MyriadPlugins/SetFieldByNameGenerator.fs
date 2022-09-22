namespace Aciq.MyriadPlugins

open Aciq.MyriadPlugins.Attribute
open FSharp.Compiler.Syntax
open FSharp.Compiler.Text.Range
open FSharp.Compiler.Text.Range
open Myriad.Core
open Myriad.Core.Ast

module private Impl =
    
    let createSetFieldByName (parent: LongIdent) (fields: SynField list) =
        let recordType =
            LongIdentWithDots.Create(parent |> List.map (fun i -> i.idText))
            |> SynType.CreateLongIdent
            
        let arguments =
            let a1 = Fa.Arg.create "key" (SynType.String())
            let a2 = Fa.Arg.create "value" (SynType.Anon range0)
            let a3 = Fa.Arg.create "x" (recordType)
            Fa.Let.create "setFieldByName" [a1;a2;a3]

        let fieldnames =
            fields
            |> List.map (fun (SynField.SynField (_, _, id, _, _, _, _, _)) ->
                let fieldIdent =
                    match id with
                    | None -> failwith "no field name"
                    | Some f -> f
                let name = LongIdentWithDots.Create([ fieldIdent.idText ])
                let ident = SynExpr.CreateIdent(fieldIdent)
                ident
            )
            
        let expr = Fa.Exp.matchOne "key" fieldnames

        let returnTypeInfo = SynBindingReturnInfo.Create(recordType)
        SynModuleDecl.CreateLet [ SynBinding.Let(pattern = arguments, expr = expr, returnInfo = returnTypeInfo) ]

    
    let createOuter (namespaceId: LongIdent) (typeDefn: SynTypeDefn) (config: (string * obj) seq) : SynModuleOrNamespace =
        let (SynTypeDefn (synComponentInfo, synTypeDefnRepr, _members, _implicitCtor, _, _)) =
            typeDefn

        let (SynComponentInfo (_attributes, _typeParams, _constraints, recordId, _doc, _preferPostfix, _access, _)) =
            synComponentInfo

        match synTypeDefnRepr with
        | SynTypeDefnRepr.Simple (SynTypeDefnSimpleRepr.Record (_accessibility, recordFields, _recordRange), _) ->

            let ident =
                LongIdentWithDots.Create(
                    namespaceId
                    |> List.map (fun ident -> ident.idText)
                )

            let openTarget = SynOpenDeclTarget.ModuleOrNamespace(ident.Lid, range0)
            let openParent = SynModuleDecl.CreateOpen openTarget


            let create = createSetFieldByName recordId recordFields

            let decls =
                [ yield openParent
                  yield create
                ]

            let info = SynComponentInfo.Create recordId
            let mdl = SynModuleDecl.CreateNestedModule(info, decls)

            let fieldsNamespace =
                config
                |> Seq.tryPick (fun (n, v) ->
                    if n = "namespace" then
                        Some(v :?> string)
                    else
                        None)
                |> Option.defaultValue "UnknownNamespace"

            SynModuleOrNamespace.CreateNamespace(Ident.CreateLong fieldsNamespace, isRecursive = true, decls = [ mdl ])
        | _ -> failwithf "Not a record type"



[<MyriadGenerator("aciq.myriadplugins.setfieldbynamegenerator")>]
type SetFieldByNameGenerator() =
    interface IMyriadGenerator with
        member _.ValidInputExtensions = seq { ".fs" }

        member _.Generate(context: GeneratorContext) =
            
            let ast, _ =
                Ast.fromFilename context.InputFilename
                |> Async.RunSynchronously
                |> Array.head

            let namespaceAndrecords =
                Ast.extractRecords ast
                |> List.choose (fun (ns, types) ->
                    match types
                        |> List.filter Ast.hasAttribute<Generator.SetFieldByNameAttribute>
                        with
                    | [] -> None
                    | types -> Some(ns, types))

            let modules =
                namespaceAndrecords
                |> List.collect (fun (ns, records) ->
                    records
                    |> List.map (fun record ->
                        let config =
                            Generator.getConfigFromAttribute<Generator.SetFieldByNameAttribute>
                                context.ConfigGetter
                                record
                        let recordModule =
                            Impl.createOuter ns record config
                        recordModule))

            Output.Ast modules

