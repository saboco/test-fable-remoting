namespace Shared

type OtherDataA = {
        Text : string
        Value : string
    }
type OtherDataB = {
    MataA : string
    MataC : string
    MataB : Map<System.Guid,OtherDataA>
}
type SomeData = {
    CataA : string
    CataC : string
    CataB : Map<System.Guid, OtherDataB>
}
type SomeOtherData = {
    A : string
    B : int
    C : SomeData
}
type Command = {
    Data : SomeData
}

type Query = string
module Route =
    let builder typeName methodName = sprintf "/api/%s/%s" typeName methodName

type IApi ={
  Show : string -> Async<SomeOtherData>
  Act : Command -> Async<string>
}