namespace Custom_Texture_Importer.Utils.CommandLineParser;

public sealed class EditConfigExpression
    : ExpressionSyntax
{
    public EditConfigExpression(SyntaxToken configKeyword, SyntaxToken dotToken, SyntaxToken editKeyword, SyntaxToken dt, SyntaxToken identifierToken, ValueExpression value)
    {
        ConfigKeyword = configKeyword;
        DotToken = dotToken;
        EditKeyword = editKeyword;
        Dt = dt;
        IdentifierToken = identifierToken;
        Value = value;
    }

    public SyntaxToken ConfigKeyword { get; }
    public SyntaxToken DotToken { get; }
    public SyntaxToken EditKeyword { get; }
    public SyntaxToken Dt { get; }
    public SyntaxToken IdentifierToken { get; }
    public ValueExpression Value { get; }

    public override SyntaxKind Kind => SyntaxKind.EditConfigExpression;
}
