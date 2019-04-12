namespace Solene.AdminClient.Crossplat

open Newtonsoft.Json
open System
open Solene.Models
open Operators

type QuestionJsonConverter() = 
    inherit JsonConverter()

    override this.CanRead = true
    override this.CanWrite = false

    override this.CanConvert (objectType: Type) : bool =        
        objectType = typeof<AdminQuestion>

    override this.ReadJson (reader: JsonReader, objectType: Type, existingValue: obj, serializer: JsonSerializer) : obj =
        match reader.TokenType with
        | JsonToken.StartObject -> 
            let value = 
                if existingValue = null then null 
                else serializer.ContractResolver.ResolveContract(objectType).DefaultCreator
            match value with
            | null -> null
            | _ -> 
                serializer.Populate(reader, existingValue)
                match existingValue with 
                | t when t.GetType() = typeof<AdminQuestion> -> 
                    let question = existingValue :?> AdminQuestion
                    question.Text <- question.Text.Replace("\\n", Environment.NewLine)
                    question :> obj
                | _ -> existingValue
        | JsonToken.Null ->
            null
        | _ -> raise (new JsonSerializationException())

    override this.WriteJson (writer: JsonWriter, value: obj, serializer: JsonSerializer) : unit =
        raise (new NotImplementedException())
        

