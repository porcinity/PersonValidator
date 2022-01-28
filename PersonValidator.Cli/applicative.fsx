open Microsoft.FSharp.Core

let (<!>) = Option.map

let (<*>) fOpt xOpt =
    match fOpt, xOpt with
        | Some f, Some x -> Some (f x)
        | _ -> None

let lift x = Some x

let mult = (*)

// functor: times3 takes one argument
let times3 = mult 3
Option.map times3 (Some 4)   // Some 12
Option.map times3 None       // None

// applicative: mult takes two arguments
mult <!> Some 3 <*> Some 4   // Some 12
mult <!> None   <*> Some 4   // None
mult <!> Some 3 <*> None

// The @ operator concatenates lists
let concatStrings = ["Hello"] @ ["world."] // ["Hello"; "world"]
let concatNums = [1;2;3] @ [4;5] // [1; 2; 3; 4; 5]
let concatTuples = [ (1,"hello") ; (2,"world") ] @ [ (3,"!") ] // [(1, "hello"); (2, "world"); (3, "!")]


// Bind
let tryParse (x:string) =
    match System.Int32.TryParse x with
    | true, x -> Ok x
    | false, _ -> Error "Enter a valid number."
    
let result x =
    match x with
    | x when x > 0 -> Ok <| x + 100
    | _ -> Error "Choose a larger number."
    
let test x =
    x
    |> tryParse
    |> Result.bind result
    
//let (|Even|Odd|) n =
//    if n % 2 = 0 then Even
//    else Odd

let (|Even|Odd|) x =
    match x with
    | x when x % 2 = 0 -> Even
    | _ -> Odd
    
let testNum n =
    match n with
    | Even -> printfn "%i is even" n
    | Odd -> printfn "%i is odd" n;;
    
let tryParse' (x:string) =
    match System.Int32.TryParse x with
    | true, x -> Some x
    | false, _ -> None
    
let printNum num =
    match num with
    | Some n -> printfn $"The number is: {n}"
    | None -> printfn "Ruh roh!"
    
let nums = [ "1"; "2"; "3" ]

let over5 x =
    match x with
    | x when x > 5 -> Some x
    | _ -> None
    
let plusOne x = Some <| x + 1
let minus2 x = Some <| x - 2

6
|> over5
|> Option.bind plusOne
|> Option.bind minus2
|> Option.bind over5

let (>>=) x f = Option.bind f x

let res = 6 |> over5 >>= plusOne >>= minus2 >>= over5

3 |> over5 |> Option.bind plusOne