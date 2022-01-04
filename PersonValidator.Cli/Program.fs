(* Dapper.Fsharp*)
open System
open Dapper.FSharp
open Dapper.FSharp.PostgreSQL
open Microsoft.FSharp.Core
open Npgsql 

type PersonName = PersonName of string

module PersonName =
    let create s =
        match s with
        | "" -> Error ["Cannot be blank"]
        | _ -> PersonName s |> Ok 
    
    let value = function
        PersonName p -> p

type PersonAge = PersonAge of int

module PersonAge =
    let (|TooOld|TooYoung|GoodAge|) n =
        match n with
        | x when x > 100 -> TooOld
        | x when x < 1 -> TooYoung
        | _ -> GoodAge
    
    let fromInt n =
        match n with
        | TooOld -> Error ["Too old."]
        | TooYoung -> Error ["Too young."]
        | GoodAge -> Ok <| PersonAge n
        
    let value (PersonAge x) = x

type Person = {
    Id : int
    Name : PersonName
    Age : PersonAge
}

module Person =
    let id = Random().Next()
    
    let create name age =
        {
            Id = id
            Name = name
            Age = age
        }
        
    let tryCreate name age =
        let name = PersonName.create name
        let age = PersonAge.fromInt age
        match name, age with
        | Error e1, Error e2 -> Error [List.concat [e1;e2]]
        | Error e, _ -> Error [e]
        | _, Error e -> Error [e]
        | Ok name, Ok age ->
            Ok { Id = id
                 Name = name
                 Age = age }

type PersonDto  =
        { Id : int
          Name : string
          Age : int }
        
module PersonDto =
    let create (person:Person) =
        let name = PersonName.value person.Name
        let age = PersonAge.value person.Age
        { Id = person.Id
          Name = name
          Age = age }

let conn = new NpgsqlConnection(@"Host=localhost;Database=fsharp;Username=test;Password=test")

let personTable = table'<PersonDto> "persons" |> inSchema "public"

let savePerson (person:PersonDto) = task {
    let! post =
       insert {
           into personTable
           value person
       }
       |> conn.InsertAsync
//  printfn "Success!"
    post
}

Console.WriteLine("Enter a name:")
let userNameInput = Console.ReadLine()

Console.WriteLine("Enter an age:")
let userAgeInput = Console.ReadLine()
let intAge (s:string) = s |> int
    
    

//let test () = task {
//    let person = Person.create input
//    match person with
//    | Ok p ->
//            let! result =
//                p
//                |> PersonDto.create
//                |> savePerson
//            result
//    | Error e -> printfn $"{e}"    
//}

//let test2 () = task {
//    let name = PersonName.create input
//    let age = PersonAge.fromInt 18
//    match name with
//    | Ok n ->
//        let! res =
//            let (person:Person) = {
//                Id = Random().Next()
//                Name = n
//                Age = age
//            }
//            person
//            |> PersonDto.create
//            |> savePerson
//        res
//    | Error e -> printfn $"{e}"
//}

let test3 () = task {
    let name = PersonName.create userNameInput
    let age = intAge userAgeInput |> PersonAge.fromInt
    match name, age with
    | Error e1, Error e2 -> printfn $"{e1}\n{e2}"
    | Error e, _ -> printfn $"{e}"
    | _, Error e -> printfn $"{e}"
    | Ok n, Ok a ->
        printfn "did it!"
        let! res =
            let person = Person.create n a
            printfn $"{person}"
            person
            |> PersonDto.create
            |> savePerson
        res
        
}

let test4 () = task {
    let age = intAge userAgeInput
    let person = Person.tryCreate userNameInput age
    match person with
    | Ok p ->
        printfn $"Person to be saved to db: {p}"
        let! res =
            p
            |> PersonDto.create
            |> savePerson
        res
    | Error e -> printfn $"{e}"
}

let apply fResult xResult =
    match fResult,xResult with
    | Ok f, Ok x -> Ok (f x)
    | Error ex, Ok _ -> Error ex
    | Ok _, Error ex -> Error ex
    | Error ex1, Error ex2 -> Error (ex1 @ ex2)

let (<!>) = Result.map
let (<*>) = apply

let validatePerson name age =
    let name = PersonName.create name
    let age = PersonAge.fromInt age
    Person.create <!> name <*> age

let applicativeSave person = task {
    match person with
    | Ok p ->
        let! res = p |> PersonDto.create |> savePerson
        res
    | Error e -> e |> List.map (fun e -> printfn $"Error: {e}")   
}

let applicativeTest () = task {
    let person = validatePerson userNameInput (intAge userAgeInput)
    let! res = applicativeSave person
    res
}

let showPeople p =
    p
    |> Seq.toList
    |> List.map (fun x -> printfn $"ID: {x.Id}\nName: {x.Name}\nAge: {x.Age}")
    |> ignore

let getEm () = task {
    let! result =
        select {
        for p in personTable do
            selectAll
        } |> conn.SelectAsync<PersonDto>
    result |> showPeople
}


applicativeTest().Wait()
getEm().Wait()

(* *)

//open System
//open Npgsql.FSharp
//
//let connectionString : string =
//    Sql.host "localhost"
//    |> Sql.database "fsharp"
//    |> Sql.username "test"
//    |> Sql.password "test"
//    |> Sql.port 5432
//    |> Sql.formatConnectionString
//    
//type Person = {
//    Id : int
//    Name : string
//}
//
//let getAllUsers (connectionString: string)  =
//    connectionString
//    |> Sql.connect
//    |> Sql.query "SELECT * FROM persons"
//    |> Sql.execute (fun read ->
//        {
//            Id = read.int "Id"
//            Name = read.string "Name"
//        })
//
//let results = getAllUsers connectionString
//
//results
//|> List.map (fun x -> printfn $"Name: {x.Name}")
//|> List.map (fun x -> printfn "%s" x)
    
//getAllUsers
//|> List.map (fun x -> printfn x)