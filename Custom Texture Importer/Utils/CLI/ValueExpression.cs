namespace Custom_Texture_Importer.Utils.CLI;

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
