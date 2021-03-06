﻿(*
    FSharpLint, a linter for F#.
    Copyright (C) 2014 Matthew Mcveigh

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*)

namespace FSharpLint.Framework

module LoadAnalysers =

    open Microsoft.FSharp.Compiler.SourceCodeServices
    open Microsoft.FSharp.Compiler.Range
    open FSharpLint.Framework
    open System.Linq

    type AnalyserVisitor = Ast.VisitorInfo -> CheckFileResults -> Ast.Visitor

    type PlainTextVisitor = Ast.VisitorInfo -> string -> string -> unit

    type AnalyserType =
        | Ast of AnalyserVisitor
        | PlainText of PlainTextVisitor

    type AnalyserPlugin =
        {
            Name: string
            Analyser: AnalyserType
        }

    type IRegisterPlugin =
        abstract RegisterPlugin : AnalyserPlugin with get

    let loadPlugins (assembly:System.Reflection.Assembly) =
        assembly.GetTypes()
            .Where(fun (t:System.Type) -> 
                t.GetInterfaces().Contains(typeof<IRegisterPlugin>)
                && t.GetConstructor(System.Type.EmptyTypes) <> null)
            .Select(fun (t:System.Type) -> 
                System.Activator.CreateInstance(t) :?> IRegisterPlugin)
            .Select(fun (x:IRegisterPlugin) -> x.RegisterPlugin)
                |> List.ofSeq