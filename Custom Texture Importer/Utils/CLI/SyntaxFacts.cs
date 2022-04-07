namespace Custom_Texture_Importer.Utils.CLI;

public static class SyntaxFacts
{
    public static SyntaxKind GetKeywordKind(string text)
    {
        return text switch
        {
            "config" => SyntaxKind.ConfigKeyword,
            "edit" => SyntaxKind.EditKeyword,
            "exit" => SyntaxKind.ExitKeyword,
            "cls" => SyntaxKind.ClsKeyword,
            "restore" => SyntaxKind.RestoreKeyword,
            "colors" => SyntaxKind.ColorsKeyword,
            "true" => SyntaxKind.TrueKeyword,
            "false" => SyntaxKind.FalseKeyword,
            "help" => SyntaxKind.HelpKeyword,
            _ => SyntaxKind.IdentifierToken,
        };
    }
}
