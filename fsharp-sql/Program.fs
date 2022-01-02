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

type PersonAge = PersonAge of int

module PersonAge =
    
    let fromInt x =
        match x with
        | x when x > 100 -> Error "Too old."
        | x when x < 1 -> Error "Too young."
        | _ -> PersonAge x |> Ok
        
    let unwrap (Ok (PersonAge i)) = PersonAge i
        
    let value (PersonAge x) = x
        
type Person =
    {
        Id : int
        Name : PersonName
        Age : PersonAge
    }
    
module Person =
    
//    let create (input:string) =
//        match input with
//        | "" -> Error "Cannot create person with blank name."
//        | _ -> Ok
//                {
//                    Id = Random().Next()
//                    Name = PersonName input
//                }
    
    let makeWithPersonName name age =
        {
            Id = Random().Next()
            Name = name
            Age = age
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
            Age : int
        }
        
module PersonDto =
    let create (person:Person) =
        let name = PersonName.value person.Name
        let age = PersonAge.value person.Age
        {
            Id = person.Id
            Name = name
            Age = age
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

Console.WriteLine("Enter an age:")
let inputAge = Console.ReadLine()
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
    let name = PersonName.create input
    let age = intAge inputAge |> PersonAge.fromInt
    match name, age with
    | Error e, _ -> printfn $"{e}"
    | _, Error e -> printfn $"{e}"
    | Ok n, Ok a ->
        printfn "did it!"
        let! res =
            let person = Person.makeWithPersonName n a
            printfn $"{person}"
            person
            |> PersonDto.create
            |> savePerson
        res
        
}
    
let getEm () = task {
    let! result =
        select {
        for p in personTable do
            selectAll
        } |> conn.SelectAsync<PersonDto>
    result
    |> Seq.toList
    |> List.map (fun x -> printfn $"ID: {x.Id}\nName: {x.Name}\nAge: {x.Age}")
    |> ignore
}

test3 () |> Async.AwaitTask |> Async.RunSynchronously
getEm () |> Async.AwaitTask |> Async.RunSynchronously

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