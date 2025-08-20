#r "nuget: Microsoft.PowerShell.SDK"
#r "nuget: System.Text.Json"
#r "nuget: CSharpier.Core"

open System
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

type ArrayOrSingleConverterOption<'T>() =
    inherit JsonConverter<'T array option>()

    override this.Read(reader: byref<Utf8JsonReader>, typeToConvert: System.Type, options: JsonSerializerOptions) =
        // Create options without this converter to avoid infinite recursion
        let safeOptions = createSafeOptions<ArrayOrSingleConverter<_>> (options)

        match reader.TokenType with
        | JsonTokenType.String ->
            // PowerShell ConvertTo-Json returns "" for empty arrays
            let stringValue = reader.GetString()

            if System.String.IsNullOrEmpty(stringValue) then
                Some [||] // Return empty array for empty string
            else
                // Try to parse as single item if it's a valid string value
                try
                    let singleItem = JsonSerializer.Deserialize<'T>(stringValue, safeOptions)
                    Some [| singleItem |]
                with _ ->
                    Some [||] // If parsing fails, return empty array
        | JsonTokenType.StartArray ->
            // Normal array case
            Some(JsonSerializer.Deserialize<'T array>(&reader, safeOptions))
        | JsonTokenType.StartObject ->
            // Single object case - wrap in array
            let singleItem = JsonSerializer.Deserialize<'T>(&reader, safeOptions)
            Some [| singleItem |]
        | JsonTokenType.Null ->
            // Handle explicit null
            Some [||]
        | _ ->
            // Handle other cases (like numbers, booleans, etc.)
            Some [||]

    override this.Write(writer: Utf8JsonWriter, value: 'T array option, options: JsonSerializerOptions) =
        JsonSerializer.Serialize(writer, value, options)

type StringBooleanConverter() =
    inherit JsonConverter<bool>()

    override this.Read(reader: byref<Utf8JsonReader>, typeToConvert: System.Type, options: JsonSerializerOptions) =
        match reader.TokenType with
        | JsonTokenType.String ->
            let strValue = reader.GetString()

            match strValue with
            | "true" -> true
            | "false" -> false
            | _ -> failwithf "Invalid boolean string value: %s" strValue
        | _ -> failwith "Expected a string token for boolean conversion"

    override this.Write(writer: Utf8JsonWriter, value: bool, options: JsonSerializerOptions) =
        writer.WriteStringValue(if value then "true" else "false")

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
    { [<JsonConverter(typeof<ArrayOrSingleConverterOption<Description>>)>]
      description: Description array option
      parameterValue: string
      name: string
      [<JsonConverter(typeof<StringBooleanConverter>)>]
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
    { [<JsonConverter(typeof<ArrayOrSingleConverterOption<Description>>)>]
      description: Description array option
      [<JsonConverter(typeof<ObjectOrEmptyConverter<Details>>)>]
      details: Details
      [<JsonConverter(typeof<ObjectOrEmptyConverter<InputTypes>>)>]
      inputTypes: InputTypes
      [<JsonConverter(typeof<ObjectOrEmptyConverter<Parameters option>>)>]
      parameters: Parameters option
      [<JsonConverter(typeof<ObjectOrEmptyConverter<AlertSet>>)>]
      alertSet: AlertSet
      [<JsonConverter(typeof<ObjectOrEmptyConverter<Examples>>)>]
      examples: Examples
      [<JsonConverter(typeof<ObjectOrEmptyConverter<Syntax>>)>]
      syntax: Syntax
      [<JsonConverter(typeof<ObjectOrEmptyConverter<ReturnValues option>>)>]
      returnValues: ReturnValues option
      [<JsonConverter(typeof<ObjectOrEmptyConverter<RelatedLinks option>>)>]
      relatedLinks: RelatedLinks option
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
        let allCommandNames =
            commands.RootElement.EnumerateArray()
            |> Seq.map _.GetProperty("Name").GetString()
            |> Seq.toList
            |> Seq.distinct

        let data =
            [ for name in allCommandNames do
                  let detailObject = eval "Get-Help" (dict [ ("Name", name) ]) (Some(99))
                  printfn "%s" (detailObject.RootElement.ToJsonString())
                  detailObject |> _.Deserialize<GetHelpResult>() ]
            |> List.distinctBy _.Name

        File.WriteAllText(cacheFile, JsonSerializer.Serialize(data, JsonSerializerOptions(WriteIndented = true)))
        data

let data = StringBuilder()

let (!+) (s: string) = data.AppendLine(s) |> ignore

for cmd in commandData do
    printfn "Command: %s" cmd.Name
    printfn "Synopsis: %s" cmd.Synopsis

    match cmd.description with
    | Some desc -> printfn "Description: %A" (desc |> Seq.map (fun d -> d.Text) |> Seq.toList)
    | None -> printfn "Description: None"

    printfn "------------------------"
    let funcName = cmd.Name.Replace("-", "_").Replace(" ", "_")

    let hasParameters =
        cmd.parameters.IsSome && cmd.parameters.Value.parameter.Length > 0

    if hasParameters then
        let parameters = cmd.parameters.Value.parameter.ToArray()
        !+ $"public class {funcName}Arguments"
        !+ "{"

        for p in parameters do
            match p.description with
            | Some desc ->
                !+ "/**"

                for d in desc do
                    !+ $" * {d.Text}"

                !+ " */"
            | None -> ()

            !+ $"""public {if p.required && p.name <> "ClusterObject" then
                               "required "
                           else
                               ""}{match p.parameterValue with
                                   | "VirtualMachine[]"
                                   | "VMSnapshot[]"
                                   | "SwitchParameter"
                                   | "int" as p -> p
                                   | "Int32" -> "int"
                                   | "String" -> "string"
                                   | "String[]" -> "string[]"
                                   | "pscredential[]" -> "PSCredential[]"
                                   | pt when String.IsNullOrEmpty(pt) -> "object"
                                   | pt -> pt}? {p.name} {{ get; set; }}"""

        !+ "}"

    let hasReturnValue =
        cmd.returnValues.IsSome
        && cmd.returnValues.Value.returnValue.Length > 0
        && cmd.returnValues.Value.returnValue.First().``type``.name <> "None"

    let typeName =
        let mapper =
            dict
                [ ("Microsoft.HyperV.PowerShell.SystemSwitchExtension",
                   "Microsoft.HyperV.PowerShell.VMSystemSwitchExtension")
                  ("Microsoft.HyperV.PowerShell.VMNetworkAdapterFailoverSetting", "PSObject") ]

        if hasReturnValue then
            let name =
                let name = cmd.returnValues.Value.returnValue.First().``type``.name
                let contains, value = mapper.TryGetValue name
                if contains then value else name

            if name.StartsWith("Microsoft.HyperV.PowerShell.Commands", StringComparison.InvariantCultureIgnoreCase) then
                name.Substring("Microsoft.HyperV.PowerShell.Commands.".Length)
            elif name.StartsWith("Microsoft.HyperV.Powershell", StringComparison.InvariantCultureIgnoreCase) then
                name.Substring("Microsoft.HyperV.Powershell.".Length)
            else
                name
        else
            "void"

    !+ $"""
/** <summary>{if cmd.relatedLinks.IsSome then
                          let links = cmd.relatedLinks.Value

                          if links.navigationLink |> isNull then
                              ""
                          else
                              let fixLink link =
                                  link.uri.Replace("&","&amp;")
                              links.navigationLink
                              |> Seq.map (fun link -> $"\n * <see href=\"{fixLink link}\">{link.linkText}</see>")
                              |> fun x -> String.Join("\n * ", x)
                      else
                          ""}
 * {cmd.Synopsis.Replace("\n", "\n * ")}
 * </summary>
 * <remarks>
 * {if cmd.description.IsSome then
        System.String.Join("\n", cmd.description.Value |> Seq.map _.Text)
    else
        "No description available."}
 * </remarks>{if
                  hasReturnValue
                  && cmd.returnValues.Value.returnValue |> isNull |> not
                  && cmd.returnValues.Value.returnValue.Any()
                  && cmd.returnValues.Value.returnValue.First().description |> isNull |> not
              then
                  let returnValueDesc =
                      cmd.returnValues.Value.returnValue.First().description
                      |> Array.map _.Text
                      |> fun v -> String.Join("\n * ", v)

                  if String.IsNullOrWhiteSpace(returnValueDesc) then
                      ""
                  else
                      "\n * <returns>" + returnValueDesc + "</returns>"
              else
                  ""}
 */
public {if hasReturnValue then $"{typeName}[]" else "void"} {funcName}({if hasParameters then $"{funcName}Arguments args" else ""})"""

    !+ "{"
    !+ $"    var parameters = new Dictionary<string, object>();"

    if hasParameters then
        for p in cmd.parameters.Value.parameter do
            !+ $"    if(args.{p.name} is not null) parameters.Add(\"{p.name}\", args.{p.name});"

    !+ $"""    using var instance = new HyperVInstance();
    var result = instance.InvokeFunction{if hasReturnValue then $"<{typeName}>" else ""}("{cmd.Name}",
    parameters);"""

    if hasReturnValue then
        !+ $"    return result;"

    !+ "}"

let outputFile =
    Path.Combine(Path.GetDirectoryName(__SOURCE_DIRECTORY__), "HyperVApi.g.cs")

let result =
    CSharpier.Core.CSharp.CSharpFormatter
        .Format(
            $$$"""
using System.Management.Automation;
using FluentHyperV.PowerShell;
using FluentHyperV.SourceGenerator;
using System.Text.Json;
using Microsoft.HyperV.PowerShell;
using Microsoft.HyperV.PowerShell.Commands;
using Microsoft.Management.Infrastructure;
using Microsoft.Vhd.PowerShell;
using System.Collections;
using System.Diagnostics;

namespace FluentHyperV.HyperV;

public class HyperVApi
{
    {{{data.ToString()}}}
}                                                         
"""
        )
        .Code

File.WriteAllText(outputFile, result)
