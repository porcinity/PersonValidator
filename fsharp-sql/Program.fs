//#r "FSharp.Data.SQLProvider.dll"
//open FSharp.Data.Sql
//open Npgsql
//
//[<Literal>]
//let DbVendor = Common.DatabaseProviderTypes.POSTGRESQL
//
//[<Literal>]
//let ConnString = "Host=localhost;Database=TheNewPuppyPlace;Username=test;Password=test"
//
//[<Literal>]
//let Schema = "TheNewPuppyPlace"
//
//let [<Literal>] resPath = @"/Users/anthony/.nuget/packages/npgsql/6.0.2"
//
////type DB = SqlDataProvider<DbVendor, ConnString, "", resPath, 1000, true, Owner="public, admin, references">
//type DB = SqlDataProvider<Common.DatabaseProviderTypes.POSTGRESQL, "Host=localhost;Database=TheNewPuppyPlace;Username=test;Password=test">
//let sql = DB.GetDataContext()
//
//query {
//    for person in sql.
//}

open System
open System.Data
open Dapper.FSharp
open Dapper.FSharp.PostgreSQL
open FSharp.Data.Sql.Providers
open Npgsql

type Person =
    {
        Id : int
        Name : string
    }

let personTable = table'<Person> "persons" |> inSchema "public"

let conn = new NpgsqlConnection(@"Host=localhost;Database=fsharp;Username=test;Password=test")

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