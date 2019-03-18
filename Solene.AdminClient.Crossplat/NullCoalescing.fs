namespace Solene.AdminClient.Crossplat

open System

module Operators=
    type NullCoalesce =  

        static member Coalesce(a: 'a option, b: 'a Lazy) = 
            match a with 
            | Some a -> a 
            | _ -> b.Value

        static member Coalesce(a: 'a Nullable, b: 'a Lazy) = 
            if a.HasValue then a.Value
            else b.Value

        static member Coalesce(a: 'a when 'a:null, b: 'a Lazy) = 
            match a with 
            | null -> b.Value 
            | _ -> a

    let inline nullCoalesceHelper< ^t, ^a, ^b, ^c when (^t or ^a) : (static member Coalesce : ^a * ^b -> ^c)> a b = 
            // calling the statically inferred member
            ((^t or ^a) : (static member Coalesce : ^a * ^b -> ^c) (a, b))

    let inline (|??) a b = nullCoalesceHelper<NullCoalesce, _, _, _> a b