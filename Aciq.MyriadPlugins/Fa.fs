[<RequireQualifiedAccess>]
module Aciq.MyriadPlugins.Fa

open System
open FSharp.Compiler.Syntax
open Myriad.Core
open Myriad.Core.Ast

[<RequireQualifiedAccess>]
module Arg = 

    let create (name:string) (typ:SynType) =
        let argname = SynPat.CreateNamed(Ident.Create name)
        SynPat.CreateTyped(argname,typ) |> SynPat.CreateParen
 
        
[<RequireQualifiedAccess>]        
module Let =
    let create (identifier:string) (args:SynPat list) =
        SynPat.CreateLongIdent(
            LongIdentWithDots.CreateString identifier, args)
        
module Fn =
    let const1 fnName args =
        SynExpr.CreateApp(
            (SynExpr.CreateIdentString fnName),
            SynExpr.CreateConstString args
        )
    

module MatchClause =
    
    let create pattern result =
        SynMatchClause.Create(pattern, None, result)

[<RequireQualifiedAccess>]
module Exp =
    
    let matchOne (matchOnArg:string) (cases: SynExpr list) =

        let matches =
            cases
            |> List.map (fun case ->
                match case with
                | SynExpr.Ident ident ->
                    let p = SynPat.CreateConst(SynConst.CreateString ident.idText)
                    
                    let lido = LongIdentWithDots.CreateString ident.idText
                    let recfn = RecordFieldName(lido,false)
                    
                    let action =
                        SynExpr.CreateRecordUpdate(
                            copyInfo = SynExpr.CreateIdentString "x",
                            fieldUpdates = [
                                recfn,
                                Some (SynExpr.CreateIdentString "value")
                            ]
                        )
                    
//                    let rhs = SynExpr.CreateConst(SynConst.CreateString "something")
                    SynMatchClause.Create(p, None, action)
                | _ -> NotImplementedException() |> raise 
            )
            
        let matches =
            matches @ [
                MatchClause.create
                    (SynPat.CreateWild)
                    (Fn.const1 "failwith" "unimplemented case")
            ]
            
            
        let matchOn =
            let ident = LongIdentWithDots.CreateString matchOnArg
            SynExpr.CreateLongIdent(false, ident, None)
            
        SynExpr.CreateMatch(matchOn, matches)
