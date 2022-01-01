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
        | "" -> None
        | _ -> PersonName s |> Some
        
    let unwrap (Some (PersonName p)) = PersonName p
    let value (PersonName p) = p

type Person =
    {
        Id : int
        Name : PersonName
    }
    
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
let personTable = table'<PersonDto> "persons" |> inSchema "public"
let createPerson (name) =
    {
        Id = 9
        Name = name
    }
let conn = new NpgsqlConnection(@"Host=localhost;Database=fsharp;Username=test;Password=test")

//Console.WriteLine("Please enter a new person's name:")
//let newPersonName = Console.ReadLine()

//let name = PersonName.create newPersonName
//
//let test:PersonDto = {
//    Id = 328142983
//    Name = "Brain"
//}

//let saveme = PersonDto.create test

let savePerson person =
     task {
        let! post =
           insert {
               into personTable
               value person
           }
           |> conn.InsertAsync
        printfn "Success!"
        }
     
//test |> savePerson


//let vnewperson = newPersonName |> makePersonName |> createPerson |> savePerson |> Async.AwaitTask |> Async.RunSynchronously

let getEm = task {
    
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

getEm |> Async.AwaitTask |> Async.RunSynchronously
 
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