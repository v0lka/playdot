using System;
using System.Collections.Generic;
using System.Linq;

namespace PlayDot.Utils.Vdf.Linq;

public class VConditional : VToken
{
    public enum TokenType
    {
        Constant,
        Not,
        Or,
        And
    }

    public const string X360 = "X360";
    public const string Ps3 = "PS3";
    public const string Win32 = "WIN32";
    public const string OsX = "OSX";
    public const string Windows = "WINDOWS";
    public const string Linux = "LINUX";
    public const string Posix = "POSIX";

    private readonly List<Token> tokens;

    public VConditional()
    {
        tokens = new List<Token>();
    }

    public override VTokenType Type => VTokenType.Conditional;
    public IReadOnlyList<Token> Tokens => tokens;

    public override VToken DeepClone()
    {
        var newCond = new VConditional();

        foreach (var token in tokens) newCond.Add(token.DeepClone());

        return newCond;
    }

    public override void WriteTo(VdfWriter writer)
    {
        writer.WriteConditional(tokens);
    }

    protected override bool DeepEquals(VToken token)
    {
        if (token is not VConditional otherCond)
            return false;

        return tokens.Count == otherCond.tokens.Count && Enumerable.Range(0, tokens.Count)
            .All(x => Token.DeepEquals(tokens[x], otherCond.tokens[x]));
    }

    public void Add(Token token)
    {
        tokens.Add(token);
    }

    public bool Evaluate(IReadOnlyList<string> definedConditionals)
    {
        var index = 0;

        bool EvaluateToken()
        {
            if (tokens[index].TokenType != TokenType.Not && tokens[index].TokenType != TokenType.Constant)
                throw new Exception($"Unexpected conditional token type ({tokens[index].TokenType}).");

            var isNot = false;

            if (tokens[index].TokenType == TokenType.Not)
            {
                isNot = true;
                index++;
            }

            if (tokens[index].TokenType != TokenType.Constant)
                throw new Exception($"Unexpected conditional token type ({tokens[index].TokenType}).");

            return isNot ^ definedConditionals.Contains(tokens[index++].Name!);
        }

        var runningResult = EvaluateToken();
        while (index < tokens.Count)
        {
            var tokenType = tokens[index++].TokenType;

            if (tokenType == TokenType.Or)
                runningResult |= EvaluateToken();
            else if (tokenType == TokenType.And)
                runningResult &= EvaluateToken();
            else
                throw new Exception($"Unexpected conditional token type ({tokenType}).");
        }

        return runningResult;
    }

    public readonly struct Token
    {
        public TokenType TokenType { get; }
        public string? Name { get; }

        public Token(TokenType tokenType, string? name = null)
        {
            TokenType = tokenType;
            Name = name;
        }

        public Token DeepClone()
        {
            return new Token(TokenType, Name);
        }

        public static bool DeepEquals(Token t1, Token t2)
        {
            return t1.TokenType == t2.TokenType && t1.Name == t2.Name;
        }
    }
}