#r "nuget: Microsoft.PowerShell.SDK"
#r "nuget: System.Text.Json"
#r "nuget: CSharpier.Core"

open System.Collections.Generic
open System.IO
open System.Linq
open System.Text
open System.Text.Json
open System.Text.Json.Serialization
open Json.More

let createSafeOptions<'t> (options: JsonSerializerOptions) =
    let safeOptions = JsonSerializerOptions()

    for converter in options.Converters do
        if
            not (
                converter.GetType().IsGenericType
                && typeof<'t>.GetGenericTypeDefinition() = converter.GetType().GetGenericTypeDefinition()
            )
        then
            safeOptions.Converters.Add(converter)

    safeOptions

type ObjectOrEmptyConverter<'T>() =
    inherit JsonConverter<'T>()

    override this.Read(reader: byref<Utf8JsonReader>, typeToConvert: System.Type, options: JsonSerializerOptions) =
        let safeOptions = createSafeOptions<ObjectOrEmptyConverter<_>> (options)

        match reader.TokenType with
        | JsonTokenType.Null -> Unchecked.defaultof<'T> // Return default value for null
        | JsonTokenType.String -> Unchecked.defaultof<'T> // Return default value for null
        | _ -> JsonSerializer.Deserialize<'T>(&reader, safeOptions)

    override this.Write(writer: Utf8JsonWriter, value: 'T, options: JsonSerializerOptions) =
        JsonSerializer.Serialize(writer, value, options)
// Custom converter to handle PowerShell's ConvertTo-Json single-element array to object conversion
type ArrayOrSingleConverter<'T>() =
    inherit JsonConverter<'T array>()

    override this.Read(reader: byref<Utf8JsonReader>, typeToConvert: System.Type, options: JsonSerializerOptions) =
        // Create options without this converter to avoid infinite recursion
        let safeOptions = createSafeOptions<ArrayOrSingleConverter<_>> (options)

        match reader.TokenType with
        | JsonTokenType.String ->
            // PowerShell ConvertTo-Json returns "" for empty arrays
            let stringValue = reader.GetString()

            if System.String.IsNullOrEmpty(stringValue) then
                [||] // Return empty array for empty string
            else
                // Try to parse as single item if it's a valid string value
                try
                    let singleItem = JsonSerializer.Deserialize<'T>(stringValue, safeOptions)
                    [| singleItem |]
                with _ ->
                    [||] // If parsing fails, return empty array
        | JsonTokenType.StartArray ->
            // Normal array case
            JsonSerializer.Deserialize<'T array>(&reader, safeOptions)
        | JsonTokenType.StartObject ->
            // Single object case - wrap in array
            let singleItem = JsonSerializer.Deserialize<'T>(&reader, safeOptions)
            [| singleItem |]
        | JsonTokenType.Null ->
            // Handle explicit null
            [||]
        | _ ->
            // Handle other cases (like numbers, booleans, etc.)
            [||]

    override this.Write(writer: Utf8JsonWriter, value: 'T array, options: JsonSerializerOptions) =
        JsonSerializer.Serialize(writer, value, options)

let eval (cmd: string) (parameters: IDictionary<string, obj>) (depth: int option) =
    use ps = System.Management.Automation.PowerShell.Create()
    ps.AddCommand cmd |> ignore

    parameters
    |> Seq.iter (fun kvp -> ps.AddParameter(kvp.Key, kvp.Value) |> ignore)

    ps.AddCommand "ConvertTo-Json" |> ignore

    match depth with
    | None -> ()
    | Some depth -> ps.AddParameter("Depth", depth) |> ignore

    // ps.AddParameter("Compress", true) |> ignore
    ps.Invoke() |> Seq.head |> _.ToString() |> JsonDocument.Parse

type Description = { Text: string }

type AlertSet =
    { [<JsonConverter(typeof<ArrayOrSingleConverter<Description>>)>]
      alert: Description array }

type Details =
    { [<JsonConverter(typeof<ArrayOrSingleConverter<Description>>)>]
      description: Description array
      name: string
      verb: string
      noun: string }

type Example =
    { code: string
      [<JsonConverter(typeof<ArrayOrSingleConverter<Description>>)>]
      introduction: Description array
      title: string
      remarks: string }

type Examples =
    { [<JsonConverter(typeof<ArrayOrSingleConverter<Example>>)>]
      example: Example array }

type Parameter =
    { [<JsonConverter(typeof<ArrayOrSingleConverter<Description>>)>]
      description: Description array
      parameterValue: string
      name: string
      required: bool
      variableLength: string
      globbing: string
      pipelineInput: string
      position: string
      aliases: string }

type Parameters =
    { [<JsonConverter(typeof<ArrayOrSingleConverter<Parameter>>)>]
      parameter: Parameter array }

type NavigationLink = { uri: string; linkText: string }

type RelatedLinks =
    { [<JsonConverter(typeof<ArrayOrSingleConverter<NavigationLink>>)>]
      navigationLink: NavigationLink array }

type TypeInfo = { name: string }

type ReturnValue =
    { ``type``: TypeInfo
      [<JsonConverter(typeof<ArrayOrSingleConverter<Description>>)>]
      description: Description array }

type ReturnValues =
    { [<JsonConverter(typeof<ArrayOrSingleConverter<ReturnValue>>)>]
      returnValue: ReturnValue array }

type SyntaxItem =
    { name: string
      [<JsonConverter(typeof<ArrayOrSingleConverter<Parameter>>)>]
      parameter: Parameter array }

type Syntax =
    { [<JsonConverter(typeof<ArrayOrSingleConverter<SyntaxItem>>)>]
      syntaxItem: SyntaxItem array }

type InputType =
    { [<JsonConverter(typeof<ArrayOrSingleConverter<Description>>)>]
      description: Description array
      ``type``: TypeInfo }

type InputTypes =
    { [<JsonConverter(typeof<ArrayOrSingleConverter<InputType>>)>]
      inputType: InputType array }

type GetHelpResult =
    { [<JsonConverter(typeof<ArrayOrSingleConverter<Description>>)>]
      description: Description array
      [<JsonConverter(typeof<ObjectOrEmptyConverter<Details>>)>]
      details: Details
      [<JsonConverter(typeof<ObjectOrEmptyConverter<InputTypes>>)>]
      inputTypes: InputTypes
      [<JsonConverter(typeof<ObjectOrEmptyConverter<Parameters>>)>]
      parameters: Parameters
      [<JsonConverter(typeof<ObjectOrEmptyConverter<AlertSet>>)>]
      alertSet: AlertSet
      [<JsonConverter(typeof<ObjectOrEmptyConverter<Examples>>)>]
      examples: Examples
      [<JsonConverter(typeof<ObjectOrEmptyConverter<Syntax>>)>]
      syntax: Syntax
      [<JsonConverter(typeof<ObjectOrEmptyConverter<ReturnValues>>)>]
      returnValues: ReturnValues
      [<JsonConverter(typeof<ObjectOrEmptyConverter<RelatedLinks>>)>]
      relatedLinks: RelatedLinks
      Name: string
      Category: string
      Synopsis: string
      Component: string option
      Role: string option
      Functionality: string option
      PSSnapIn: string option
      ModuleName: string }


printfn "Generating Hyper-V VM configuration..."
printfn "All Commands"
//-Module Hyper-V
let commands = eval "Get-Command" (dict [ ("Module", "Hyper-V") ]) None
printfn "Total Commands: %d" (commands.RootElement.GetArrayLength())

printfn
    "Commands: %A"
    (commands.RootElement.EnumerateArray()
     |> Seq.map _.GetProperty("Name").GetString()
     |> Seq.toList)

let commandData =
    let cacheFile =
        Path.Combine(Path.GetDirectoryName(__SOURCE_DIRECTORY__), "commands.json")

    if File.Exists(cacheFile) then
        printfn "Loading cached commands from %s" cacheFile
        File.ReadAllText(cacheFile) |> JsonSerializer.Deserialize<GetHelpResult list>
    else
        let data =
            [ for cmd in commands.RootElement.EnumerateArray() do
                  let name = cmd.GetProperty("Name").GetString()
                  let detailObject = eval "Get-Help" (dict [ ("Name", name) ]) (Some(99))
                  printfn "%s" (detailObject.RootElement.ToJsonString())
                  detailObject |> _.Deserialize<GetHelpResult>() ]

        File.WriteAllText(cacheFile, JsonSerializer.Serialize(data))
        data

let data = StringBuilder()

let (!+) (s: string) = data.AppendLine(s) |> ignore

for cmd in commandData do
    printfn "Command: %s" cmd.Name
    printfn "Synopsis: %s" cmd.Synopsis
    printfn "Description: %A" (cmd.description |> Seq.map (fun d -> d.Text) |> Seq.toList)
    printfn "------------------------"
    let funcName = cmd.Name.Replace("-", "_").Replace(" ", "_")
    let parameters = cmd.parameters.parameter.ToArray()

    if parameters.Any() then
        !+ $"public class {funcName}Arguments"
        !+ "{"

        for p in parameters do
            !+ $"// Command: {cmd.Name}"
            !+ $"""public {if p.required then "required " else ""}string {p.name} {{ get; set; }} // {p.description |> Seq.map (fun d -> d.Text) |> String.concat " "}"""

        !+ "}"
// printfn "Command: %s" detail.Name
// printfn "Synopsis: %s" detail.Synopsis
// // printfn "Description: %A" (detail.description |> Seq.map (fun d -> d.Text) |> Seq.toList)
// printfn "------------------------"
// let funcName= detail.Name.Replace("-", "_").Replace(" ", "_")
// let parameters = detail.parameters.parameter.ToArray()
// if parameters.Any() then
//     !+ $"public class {funcName}Arguments"
//
//     for p in parameters do
//
// !+ $"// Command: {detail.Name}"
// !+ $"public void {funcName}("
//
// printfn "------------------------"


let outputFile =
    Path.Combine(Path.GetDirectoryName(__SOURCE_DIRECTORY__), "HyperVApi.g.cs")

let result =
    CSharpier.Core.CSharp.CSharpFormatter
        .Format(
            $$$"""
using FluentHyperV.PowerShell;
using FluentHyperV.SourceGenerator;

namespace FluentHyperV.HyperV;

internal class HyperVApi
{
    private readonly Lazy<HyperVInstance> _powerShellInstance = new(() => new HyperVInstance());

    public HyperVInstance PowerShellInstance => _powerShellInstance.Value;

    {{{data.ToString()}}}
}                                                         
"""
        )
        .Code

File.WriteAllText(outputFile, result)
