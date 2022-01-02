(* Dapper.Fsharp*)
open System
open System.Data
open Dapper.FSharp
open Dapper.FSharp.PostgreSQL
open FSharp.Data.Sql.Providers
open Microsoft.FSharp.Core
open Npgsql

type PersonName = PersonName of string
module PersonName =
    let create s =
        match s with
        | "" -> Error "Cannot be blank"
        | _ -> PersonName s |> Ok 
        
    let unwrap (Ok (PersonName p)) = PersonName p
    let value = function
        PersonName p -> p
    
type Person =
    {
        Id : int
        Name : PersonName
    }
    
module Person =
    
    let create (input:string) =
        match input with
        | "" -> Error "Cannot create person with blank name."
        | _ -> Ok
                {
                    Id = Random().Next()
                    Name = PersonName input
                }
    
    let makeWithPersonName name =
        {
            Id = Random().Next()
            Name = PersonName.unwrap name
        }
    
//    let ofDto (person:Person) =
//        {
//            Id = person.Id
//            Name = PersonName.value person.Name
//        }

type PersonDto  =
        {
            Id : int
            Name : string
        }
        
module PersonDto =
    let create (person:Person) =
        let name = PersonName.value person.Name
        {
            Id = person.Id
            Name = name
        }

let conn = new NpgsqlConnection(@"Host=localhost;Database=fsharp;Username=test;Password=test")

let personTable = table'<PersonDto> "persons" |> inSchema "public"

let savePerson (person:PersonDto) =
     task {
        let! post =
           insert {
               into personTable
               value person
           }
           |> conn.InsertAsync
        printfn "Success!"
        }

Console.WriteLine("Enter a name:")
let input = Console.ReadLine()


let test () = task {
    let person = Person.create input
    match person with
    | Ok p ->
            let! result =
                p
                |> PersonDto.create
                |> savePerson
            result
    | Error e -> printfn $"{e}"    
}
    
let getEm () = task {
    let! result =
        select {
        for p in personTable do
            selectAll
        } |> conn.SelectAsync<PersonDto>
    result
    |> Seq.toList
    |> List.map (fun x -> printfn $"ID: {x.Id}\nName: {x.Name}")
    |> ignore
}

test () |> Async.AwaitTask |> Async.RunSynchronously
getEm() |> Async.AwaitTask |> Async.RunSynchronously

//let createPerson (name) =
//    {
//        Id = 9
//        Name = name
//    }

//
//Console.WriteLine("Please enter a new person's name:")
//let newPersonName = Console.ReadLine()
//
//let name = PersonName.create newPersonName
//
//let test:Person = {
//    Id = 328142983
//    Name = PersonName "mista mahn"
//}
//
//let saveme = PersonDto.create test
//
//let reelPerson:Person =
//    {
//        Id = 234
//        Name = name
//    }
//

//
//
////let vnewperson = newPersonName |> makePersonName |> createPerson |> savePerson |> Async.AwaitTask |> Async.RunSynchronously
//

//
//getEm |> Async.AwaitTask |> Async.RunSynchronously
// 
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