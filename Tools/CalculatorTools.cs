using System.ComponentModel;
using ModelContextProtocol.Server;

namespace CalculatorMcp.Tools;

[McpServerToolType]
public static class CalculatorTools
{
    [McpServerTool(Name = "add", ReadOnly = true), Description("Adds two numbers and returns the sum.")]
    public static double Add(
        [Description("The first number.")] double a,
        [Description("The second number.")] double b) => a + b;

    [McpServerTool(Name = "multiply", ReadOnly = true), Description("Multiplies two numbers and returns the product.")]
    public static double Multiply(
        [Description("The first number.")] double a,
        [Description("The second number.")] double b) => a * b;
}
