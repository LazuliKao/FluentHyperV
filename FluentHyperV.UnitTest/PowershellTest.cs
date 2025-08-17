using System.Management.Automation;
using FluentHyperV.Models;
using FluentHyperV.PowerShell;
using Json.More;
using Xunit.Abstractions;

namespace FluentHyperV.UnitTest;

public class PowershellTest(ITestOutputHelper testOutputHelper)
{
    public static PSObject extractProperty(PSObject psObject, string propertyName)
    {
        if (psObject.Properties[propertyName] is not { } property)
            throw new ArgumentException($"Property '{propertyName}' not found in PSObject.");
        return (PSObject)property.Value;
    }

    public static PSObject extractPropertyArray(PSObject psObject, string propertyName)
    {
        if (psObject.Properties[propertyName] is not { } property)
            throw new ArgumentException($"Property '{propertyName}' not found in PSObject.");
        var firstObject = ((PSObject)property.Value).Properties.First().Value;
        if (firstObject is not object[] array)
        {
            var fo = firstObject as PSObject;
            if (fo is null)
                throw new ArgumentException(
                    $"Property '{propertyName}' is not an array. it is {firstObject.GetType()}."
                );
            return fo;
        }

        return (PSObject)array[0];
    }

    [Fact]
    public void TestInstance()
    {
        var instance = new PowerShellInstance();
        var resultRaw = instance.InvokeFunction("Get-Help", new() { ["Name"] = "Get-Process" });
        var parameters = (PSObject)
            extractProperty(resultRaw, "returnValues").Properties["returnValue"].Value;
        // testOutputHelper.WriteLine(parameters.Members);
        foreach (var property in parameters.Members)
        {
            testOutputHelper.WriteLine(
                $"public {property.TypeNameOfValue} {property.Name} {{get; set;}}"
            );
        }
        // var parameters = extractPropertyArray(resultRaw, "inputTypes");
        // foreach (var property in parameters.Properties)
        // {
        //     testOutputHelper.WriteLine($"public {property.TypeNameOfValue} {property.Name} {{get; set;}}");
        //     testOutputHelper.WriteLine(property.TypeNameOfValue);
        //     foreach (PSPropertyInfo psPropertyInfo in ((PSObject)property.Value).Properties)
        //     {
        //         testOutputHelper.WriteLine(psPropertyInfo.Name);
        //         testOutputHelper.WriteLine(psPropertyInfo.Value.ToString());
        //         testOutputHelper.WriteLine("===");
        //     }
        // }

        foreach (var property in ((PSObject)(resultRaw.Properties["Syntax"].Value)).Properties)
            testOutputHelper.WriteLine(property.Name);
        foreach (var property in ((PSObject)(resultRaw.Properties["parameters"].Value)).Properties)
            testOutputHelper.WriteLine(property.Name);
        foreach (var property in ((PSObject)(resultRaw.Properties["inputTypes"].Value)).Properties)
            testOutputHelper.WriteLine(property.Name);
        foreach (
            var property in ((PSObject)(resultRaw.Properties["relatedLinks"].Value)).Properties
        )
            testOutputHelper.WriteLine(property.Name);
        foreach (
            var property in ((PSObject)(resultRaw.Properties["returnValues"].Value)).Properties
        )
            testOutputHelper.WriteLine(property.Name);
        // (object[])((PSObject)resultRaw.Properties["relatedLinks"].Value).Properties.First()
        // var result = instance.InvokeFunction<HelpResult>(
        //     "Get-Help",
        //     new() { ["Name"] = "Get-Process" },
        //     onError: err => { testOutputHelper.WriteLine(err.Message); }
        // );

        // Assert.NotNull(result);
        // testOutputHelper.WriteLine(result.ToJsonDocument().RootElement.ToJsonString());
        // testOutputHelper.WriteLine(result.parameters.ToJsonDocument().RootElement.ToJsonString());

        // foreach (var property in result.Properties)
        // {
        //     testOutputHelper.WriteLine($"{property.TypeNameOfValue} {property.Name}");
        // }
    }
}
