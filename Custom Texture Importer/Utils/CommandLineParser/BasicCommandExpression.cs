namespace Custom_Texture_Importer.Utils.CommandLineParser;

public sealed class BasicCommandExpression
    : ExpressionSyntax
{
    public BasicCommandExpression(SyntaxToken command)
    {
        Command = command;
    }

    public override SyntaxKind Kind => SyntaxKind.BasicCommandExpression;

    public SyntaxToken Command { get; }
}
