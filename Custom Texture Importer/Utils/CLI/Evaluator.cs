using Custom_Texture_Importer.Models;
using Custom_Texture_Importer.UI;
using Spectre.Console;
using System.Reflection;

namespace Custom_Texture_Importer.Utils.CLI;

public sealed class Evaluator
{
    private readonly ExpressionSyntax _root;

    public Evaluator(ExpressionSyntax root)
    {
        _root = root;
    }

    public async Task Evaluate()
    {
        await Evaluate(_root);
    }

    private async Task Evaluate(ExpressionSyntax node)
    {
        switch (node.Kind)
        {
            case SyntaxKind.EditConfigExpression:
                EvaluateEditConfig((EditConfigExpression)node);
                break;
            case SyntaxKind.BasicCommandExpression:
                await EvaluateBasicCommand((BasicCommandExpression)node);
                break;
            default:
                throw new Exception($"Unexpected node: {node.Kind}");
        }
    }

    private void EvaluateEditConfig(EditConfigExpression node)
    {
        var value = EvaluateValueExpression(node.Value);
        var property = Config.CurrentConfig.GetType().GetProperty(node.IdentifierToken.Text);
        property.SetValue(Config.CurrentConfig, value);
        Config.SaveConfig();
    }

    public static void PrintHelpMessage()
    {
        GUI.WriteLineColored(Color.Purple, "Available commands:");
        GUI.WriteLineColored(GUI.INFO_COLOR, "  exit - Exit the program.");
        GUI.WriteLineColored(GUI.INFO_COLOR, "  colors - List all available colors.");
        GUI.WriteLineColored(GUI.INFO_COLOR, "  cls - Clear the screen.");
        GUI.WriteLineColored(GUI.INFO_COLOR, "  restore - Remove the duped files.");
        GUI.WriteLineColored(GUI.INFO_COLOR, "  config.edit.[[Json Property Name]] [[Value]] - Edits the json property, then saves.");
        GUI.WriteLineColored(GUI.INFO_COLOR, "  help - Shows this help message.");
        GUI.WriteLineColored(GUI.INFO_COLOR, "  #config.edit.HelpMessage false - Disables the help message on startup");
    }

    private async Task EvaluateBasicCommand(BasicCommandExpression node)
    {
        switch (node.Command.Kind)
        {
            case SyntaxKind.ExitKeyword:
            {
                Environment.Exit(0);
                break;
            }
            case SyntaxKind.ColorsKeyword:
            {
                var colorType = typeof(Color);
                var staticProperties = colorType.GetProperties(BindingFlags.Public | BindingFlags.Static);
                foreach (var property in staticProperties)
                {
                    if (property.PropertyType == typeof(Color))
                    {
                        var color = (Color)property.GetValue(null);
                        GUI.WriteLineColored(color, color.ToMarkup());
                    }
                }
                break;
            }
            case SyntaxKind.ClsKeyword:
            {
                GUI.Clear();
                break;
            }
            case SyntaxKind.RestoreKeyword:
            {
                await FortniteUtil.RemoveDupedUcas();
                break;
            }
            case SyntaxKind.ConfigKeyword:
            {
                var configString = Config.CurrentConfig.ToString();
                GUI.WriteLineColored(GUI.INFO_COLOR, configString);
                break;
            }
            case SyntaxKind.HelpKeyword:
            {
                PrintHelpMessage();
                break;
            }
            default:
            {
                GUI.WriteLineColored(GUI.ERROR_COLOR, $"Invalid command: {node.Command.Text}");
                break;
            }
        }
    }

    private object EvaluateValueExpression(ValueExpression node)
    {
        switch (node.Value.Kind)
        {
            case SyntaxKind.TrueKeyword:
                return true;
            case SyntaxKind.FalseKeyword:
                return false;
            case SyntaxKind.StringToken:
                return node.Value.Text;
            default:
                throw new Exception($"Unexpected node: {node.Value.Kind}");
        }
    }
}
