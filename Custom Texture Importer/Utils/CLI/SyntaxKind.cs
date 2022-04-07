namespace Custom_Texture_Importer.Utils.CLI;

public enum SyntaxKind
{
    EndOfFileToken,
    WhitespaceToken,
    ConfigKeyword,
    ExitKeyword,
    ClsKeyword,
    RestoreKeyword,
    ColorsKeyword,
    HelpKeyword,
    IdentifierToken,
    TrueKeyword,
    FalseKeyword,
    StringToken,
    DotToken,
    BadToken,
    EditKeyword,
    EditConfigExpression,
    ValueExpression,
    BasicCommandExpression,
    SaveKeyword
}
