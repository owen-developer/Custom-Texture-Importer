namespace Custom_Texture_Importer.Utils.CommandLineParser;

public sealed class Parser
{
    private readonly List<string> _diagnostics;
    private readonly SyntaxToken[] _tokens;
    private int _position;
    
    public Parser(string text)
    {
        var lexer = new Lexer(text);
        var tokens = new List<SyntaxToken>();
        SyntaxToken token;
        do
        {
            token = lexer.Lex();
            if (token.Kind != SyntaxKind.WhitespaceToken &&
                token.Kind != SyntaxKind.BadToken)
            {
                tokens.Add(token);
            }
        } while (token.Kind != SyntaxKind.EndOfFileToken);

        _diagnostics = new List<string>();
        _diagnostics.AddRange(lexer.Diagnostics);
        _tokens = tokens.ToArray();
        _position = 0;
    }

    public IEnumerable<string> Diagnostics => _diagnostics;

    private SyntaxToken Peek(int offset = 0)
    {
        var index = _position + offset;
        if (index >= _tokens.Length)
        {
            return _tokens[^1];
        }

        return _tokens[index];
    }

    private SyntaxToken Current => Peek();

    private SyntaxToken MatchToken(SyntaxKind kind)
    {
        if (Current.Kind == kind)
        {
            return NextToken();
        }

        _diagnostics.Add($"Unexpected token '{Current.Kind}', expected '{kind}'.");
        return new SyntaxToken(SyntaxKind.BadToken, Current.Position, Current.Text);
    }

    private SyntaxToken NextToken()
    {
        var current = Current;
        _position++;
        return current;
    }

    public ExpressionSyntax Parse()
    {
        var expression = ParseExpression();
        if (Current.Kind != SyntaxKind.EndOfFileToken)
        {
            _diagnostics.Add($"Expected end of file, but found {Current.Text} at position {Current.Position}.");
        }

        return expression;
    }

    private ExpressionSyntax ParseExpression()
    {
        if (Current.Kind == SyntaxKind.ConfigKeyword &&
            Peek(2).Kind == SyntaxKind.EditKeyword)
        {
            return ParseEditConfig();
        }

        return new BasicCommandExpression(NextToken());
    }

    private EditConfigExpression ParseEditConfig()
    {
        var configKeyword = MatchToken(SyntaxKind.ConfigKeyword);
        var dotToken = MatchToken(SyntaxKind.DotToken);
        var editKeyword = MatchToken(SyntaxKind.EditKeyword);
        var dt = MatchToken(SyntaxKind.DotToken);
        var id = MatchToken(SyntaxKind.IdentifierToken);
        var value = ParseValueExpression();
        return new EditConfigExpression(configKeyword, dotToken, editKeyword, dt, id, value);
    }

    private ValueExpression ParseValueExpression()
    {
        var valueToken = NextToken();
        return new ValueExpression(valueToken);
    }
}
