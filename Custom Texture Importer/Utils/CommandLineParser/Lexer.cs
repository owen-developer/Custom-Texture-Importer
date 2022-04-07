using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Custom_Texture_Importer.Utils.CommandLineParser;
internal sealed class Lexer
{
    private readonly List<string> _diagnostics;
    private readonly string _text;
    private int _position;

    public Lexer(string text)
    {
        _diagnostics = new List<string>();
        _text = text;
        _position = 0;
    }

    public IEnumerable<string> Diagnostics => _diagnostics;

    private char Peek(int offset = 0)
    {
        var index = _position + offset;
        if (index >= _text.Length)
            return '\0';
        
        return _text[index];
    }

    private char Current => Peek();

    private void Advance()
    {
        _position++;
    }
    
    public SyntaxToken Lex()
    {
        if (Current == '\0')
        {
            return new SyntaxToken(SyntaxKind.EndOfFileToken, _position, "\0");
        }

        if (char.IsWhiteSpace(Current))
        {
            return LexWhiteSpace();
        }

        if (char.IsLetter(Current))
        {
            return LexIdentifier();
        }

        switch (Current)
        {
            case '\"':
                return LexString();
            case '.':
                return new SyntaxToken(SyntaxKind.DotToken, _position++, ".");
        }

        _diagnostics.Add($"Unexpected character '{Current}' at position {_position}");
        return new SyntaxToken(SyntaxKind.BadToken, _position++, Current.ToString());
    }

    private SyntaxToken LexWhiteSpace()
    {
        var start = _position;
        while (char.IsWhiteSpace(Current))
        {
            Advance();
        }

        return new SyntaxToken(SyntaxKind.WhitespaceToken, start, _text[start.._position]);
    }

    private SyntaxToken LexIdentifier()
    {
        var start = _position;
        while (char.IsLetter(Current))
        {
            Advance();
        }

        var text = _text[start.._position];
        var kind = SyntaxFacts.GetKeywordKind(text);
        return new SyntaxToken(kind, start, text);
    }

    private SyntaxToken LexString()
    {
        Advance();
        var start = _position;
        while (Current != '\"')
        {
            if (Current == '\0')
            {
                _diagnostics.Add($"Unterminated string starting at position {start}.");
                return new SyntaxToken(SyntaxKind.EndOfFileToken, start, "\0");
            }

            Advance();
        }

        var text = _text[start.._position];
        Advance();
        return new SyntaxToken(SyntaxKind.StringToken, start, text);
    }
}
