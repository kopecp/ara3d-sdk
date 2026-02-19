    namespace Ara3D.Parakeet.Grammars
{
    /// <summary>
    /// IFC-focused EXPRESS grammar:
    /// - Parses ENTITY name, optional SUBTYPE OF (...), and explicit attributes.
    /// - Skips FUNCTION and RULE blocks entirely.
    /// - Robust WS/comment handling.
    /// </summary>
    public class ExpressGrammar : BaseCommonGrammar
    {
        public static readonly ExpressGrammar Instance = new ExpressGrammar();

        // IFC EXPRESS uses (* ... *) block comments (often multiline)
        public Rule Comment => Named("(*" + AnyCharUntilPast("*)"));

        // -----------------------------
        // Blocks 
        // These are used for parsing out chunks of the express file at a time. 
        // -----------------------------

        public Rule EntityBlock => Node("ENTITY" + AnyCharUntilPast("END_ENTITY;"));
        public Rule TypeBlock => Node("TYPE" + AnyCharUntilPast("END_TYPE;"));
        public Rule EntityBlocks => Named(ZeroOrMore(EntityBlock | AnyChar));
        public Rule TypeBlocks => Named(ZeroOrMore(TypeBlock | AnyChar));

        // -----------------------------
        // Whitespace / Skipping
        // -----------------------------
        // Treat FUNCTION/RULE blocks as "skippable noise" (like comments).
        // This makes the file parse succeed even if we don't model those constructs.

        // Skippable should never generate a "Node"
        public Rule FunctionBlock => Named(KeywordNoWS("FUNCTION") + AnyCharUntilPast("END_FUNCTION;"));
        public Rule RuleBlock => Named(KeywordNoWS("RULE") + AnyCharUntilPast("END_RULE;"));
        public Rule SkippedSection => Named(FunctionBlock | RuleBlock);

        public override Rule WS => Named((SpaceChars | Comment | SkippedSection).ZeroOrMore());

        // -----------------------------
        // Tokens / identifiers
        // -----------------------------

        public new Rule Identifier => Node(IdentifierFirstChar + IdentifierChar.ZeroOrMore());
        public Rule Eos => Named(Sym(";")); 

        // -----------------------------
        // Bounds and aggregation
        // -----------------------------

        public Rule QMark => Named(Sym("?")); 
        public Rule Bound => Node(Integer | QMark);

        // [lower:upper]
        public Rule Dim => Node(Sym("[") + Bound + Sym(":") + Bound + Sym("]"));

        public Rule AggregationKeyword => Named(Keywords("SET", "LIST", "BAG", "ARRAY"));
        public Rule UniqueKeyword => Named(Keyword("UNIQUE"));
        public Rule OptionalKeyword => Named(Keyword("OPTIONAL"));
        public Rule OfKeyword => Named(Keyword("OF"));
        public Rule NamedType => Node(Identifier);

        // Aggregation: SET [1:?] OF <TypeExpr>
        // Also supports "SET OF ..." (no dimension) which appears in local declarations sometimes.
        // Also supports "OF UNIQUE <TypeExpr>" which EXPRESS allows in some contexts.
        public Rule AggregationType => Node(
            AggregationKeyword
            + Dim.Optional()
            + OfKeyword
            + UniqueKeyword.Optional()
            + Recursive(nameof(TypeExpr))
        );

        // Type expression we care about for IFC entities:
        // OPTIONAL? (AggregationType | NamedType)
        public Rule TypeExpr => Node(OptionalKeyword.Optional() + (AggregationType | NamedType));

        // -----------------------------
        // ENTITY parsing (what you want)
        // -----------------------------

        // Header clauses WITHOUT trailing semicolon
        public Rule SubtypeHeader => Node(Keyword("SUBTYPE") + Keyword("OF") + IdentifierList);

        // SUPERTYPE OF(ONEOF (A, B))
        public Rule OneOfSupertype => Node(Parenthesized(Keyword("ONEOF") + WS + IdentifierList));

        public Rule SupertypeHeader => Node(Keyword("ABSTRACT").Optional() + Keyword("SUPERTYPE")
            + Keyword("OF")
            + (OneOfSupertype | IdentifierList) // allow SUPERTYPE OF(SomeList) too
        );

        // Entity header ends at the FIRST semicolon after ENTITY name and optional headers.
        public Rule EntityHeader => Node(
            Keyword("ENTITY")
            + Identifier 
            + SupertypeHeader.Optional()
            + SubtypeHeader.Optional() 
            + Eos
        );

        // (A, B, C)
        public Rule IdentifierList => Node(ParenthesizedList(Identifier));

        // SUBTYPE OF (A, B);
        public Rule SubtypeClause => Node(Keyword("SUBTYPE") + Keyword("OF") + IdentifierList + Eos);

        // ABSTRACT SUPERTYPE OF (ONEOF(...)); appears in IFC sometimes, but you said you mainly need subtypes.
        // We'll parse it as "noise" so it doesn't break the entity.
        public Rule SupertypeNoise => Node(
            Keyword("ABSTRACT").Optional()
            + Keyword("SUPERTYPE")
            + Keyword("OF")
            + AnyCharUntilPast(Eos));

        // Attribute declaration:
        //   Name : TypeExpr;
        public Rule AttributeDecl => Node(Identifier + Sym(":") + AbortOnFail + TypeExpr + Eos);

        // Sections we mostly ignore (but must not break parse):
        // DERIVE ... WHERE ... INVERSE ... UNIQUE ... etc.
        // We'll just "eat" them safely until END_ENTITY.
        public Rule EntityInnerNoiseSectionHeader => Named(
            Keyword("DERIVE") | Keyword("WHERE") | Keyword("INVERSE") | Keyword("UNIQUE") | Keyword("LOCAL"));

        public Rule EntityInnerNoiseSection => Node(
            EntityInnerNoiseSectionHeader
            + AnyCharUntilAt(Keyword("END_ENTITY") | EntityInnerNoiseSectionHeader));

        // Explicit attribute lines appear before DERIVE/WHERE/INVERSE (typically).
        // We'll parse as many AttributeDecl as possible, then skip other sections until END_ENTITY.
        public Rule EntityBody => Node(AttributeDecl.ZeroOrMore() + EntityInnerNoiseSection.ZeroOrMore());

        public Rule EndEntity => Node(Keyword("END_ENTITY") + Eos);

        // ENTITY IfcFoo; [SubtypeClause]? [SupertypeNoise]? <AttributeDecl...> END_ENTITY;
        public Rule Entity => Node(EntityHeader + EntityBody + EndEntity);

        // -----------------------------
        // Top-level (file) parsing
        // -----------------------------
        // We *can* optionally parse SCHEMA header, but IFC files sometimes include it.
        public Rule SchemaHeader => Node(Keyword("SCHEMA") + Identifier + Eos);

        public Rule EndSchema => Node(Keyword("END_SCHEMA") + Eos);

        // Type and other declarations exist; we can skip them safely.
        // If later you want ENUM/TYPE extraction, we can add it without changing Entity parsing.
        public Rule TypeDeclNoise => Node(Keyword("TYPE") + AnyCharUntilPast(Keyword("END_TYPE") + Eos));

        public Rule OtherTopLevelNoise => Named(TypeDeclNoise | SkippedSection | Comment);

        public Rule TopLevelDecl => Named(Entity | OtherTopLevelNoise);

        public Rule File => Node(WS
            + SchemaHeader.Optional() + WS
            + TopLevelDecl.ZeroOrMore() + WS
            + EndSchema.Optional() + WS
            + EndOfInput);

        public override Rule StartRule => File;
    }
}
