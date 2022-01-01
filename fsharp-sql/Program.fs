//#r "FSharp.Data.SQLProvider.dll"
//open FSharp.Data.Sql
//open Npgsql
//
//[<Literal>]
//let DbVendor = Common.DatabaseProviderTypes.POSTGRESQL
//
//[<Literal>]
//let ConnString = "Host=localhost;Database=fsharp;Username=test;Password=test"
//
//[<Literal>]
//let Schema = "persons"
//
//let [<Literal>] resPath = @"/Users/anthony/.nuget/packages/npgsql/6.0.2"
//
//type DB = SqlDataProvider<DbVendor, ConnString, "", resPath, 1000, true, Owner="public">
//let context = DB.GetDataContext()
//
//let response =
//    query {
//        for p in context.
//    }

(* Dapper.Fsharp*)
open System
open System.Data
open Dapper.FSharp
open Dapper.FSharp.PostgreSQL
open FSharp.Data.Sql.Providers
open Npgsql

type PersonName = PersonName of string

type Person =
    {
        Id : int
        Name : PersonName
    }

let personTable = table'<Person> "persons" |> inSchema "public"

let conn = new NpgsqlConnection(@"Host=localhost;Database=fsharp;Username=test;Password=test")

Console.WriteLine("Please enter a new person's name:")
let newPersonName = Console.ReadLine() 

let makePersonName string =
    match string with
    | "" -> None
    | _ -> Some (PersonName string)

//let createPerson (personName:PersonName option)=
//    match personName with
//    | Some name -> 
//        {
//        Id = Random().Next(5, 100)
//        Name = name
//        }
//        printfn "person created!"
//    | None -> "e"

let createPerson (name:PersonName option) =
    match name with
    | Some name ->
        Some {
            Id = Random().Next()
            Name = name
        }
    | None -> None
        
type Error = Error

let savePerson (person:Person option) : Result<Person, Error> =
    match person with
    | Some person -> task {
        let! post =
           insert {
               into personTable
               value person
           }
           |> conn.InsertAsync
        printfn "Success!"
        }
    | None -> Error

let vnewperson = newPersonName |> makePersonName |> createPerson |> savePerson |> Async.AwaitTask |> Async.RunSynchronously

let getEm = task {
    
    let! result = 
        select {
        for p in personTable do
            selectAll
        } |> conn.SelectAsync<Person>
    
 
    
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