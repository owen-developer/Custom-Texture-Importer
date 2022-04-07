namespace Custom_Texture_Importer.Utils.CommandLineParser;

public sealed class ValueExpression
    : ExpressionSyntax
{
    public ValueExpression(SyntaxToken value)
    {
        Value = value;
    }

    public SyntaxToken Value { get; }

    public override SyntaxKind Kind => SyntaxKind.ValueExpression;
}
