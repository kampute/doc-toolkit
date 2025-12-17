// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Languages
{
    using Kampute.DocToolkit.Metadata;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.CompilerServices;

    public partial class CSharp
    {
        /// <summary>
        /// The URL of the C# keywords reference page.
        /// </summary>
        private static readonly Uri KeywordsUrl = new("https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/");

        /// <summary>
        /// A dictionary of C# keywords to their corresponding relative documentation links.
        /// </summary>
        private static readonly Dictionary<string, string> KeywordLinks = new(StringComparer.Ordinal)
        {
            // Reserved keywords
            ["abstract"] = "abstract",
            ["as"] = "../operators/type-testing-and-cast#as-operator",
            ["base"] = "base",
            ["bool"] = "../builtin-types/bool",
            ["break"] = "../statements/jump-statements#the-break-statement",
            ["byte"] = "../builtin-types/integral-numeric-types",
            ["case"] = "../statements/selection-statements#the-switch-statement",
            ["catch"] = "../statements/exception-handling-statements#the-try-catch-statement",
            ["char"] = "../builtin-types/char",
            ["checked"] = "../statements/checked-and-unchecked",
            ["class"] = "class",
            ["const"] = "const",
            ["continue"] = "../statements/jump-statements#the-continue-statement",
            ["decimal"] = "../builtin-types/floating-point-numeric-types",
            ["default"] = "default",
            ["delegate"] = "../builtin-types/reference-types",
            ["do"] = "../statements/iteration-statements#the-do-statement",
            ["double"] = "../builtin-types/floating-point-numeric-types",
            ["else"] = "../statements/selection-statements#the-if-statement",
            ["enum"] = "../builtin-types/enum",
            ["event"] = "event",
            ["explicit"] = "../operators/user-defined-conversion-operators",
            ["extern"] = "extern",
            ["false"] = "../builtin-types/bool",
            ["finally"] = "../statements/exception-handling-statements#the-try-finally-statement",
            ["fixed"] = "../statements/fixed",
            ["float"] = "../builtin-types/floating-point-numeric-types",
            ["for"] = "../statements/iteration-statements#the-for-statement",
            ["foreach"] = "../statements/iteration-statements#the-foreach-statement",
            ["goto"] = "../statements/jump-statements#the-goto-statement",
            ["if"] = "../statements/selection-statements#the-if-statement",
            ["implicit"] = "../operators/user-defined-conversion-operators",
            ["in"] = "in",
            ["int"] = "../builtin-types/integral-numeric-types",
            ["interface"] = "interface",
            ["internal"] = "internal",
            ["is"] = "../operators/is",
            ["lock"] = "../statements/lock",
            ["long"] = "../builtin-types/integral-numeric-types",
            ["namespace"] = "namespace",
            ["new"] = "new",
            ["null"] = "null",
            ["object"] = "../builtin-types/reference-types",
            ["operator"] = "../operators/operator-overloading",
            ["out"] = "out",
            ["override"] = "override",
            ["params"] = "method-parameters#params-modifier",
            ["private"] = "private",
            ["protected"] = "protected",
            ["public"] = "public",
            ["readonly"] = "readonly",
            ["ref"] = "ref",
            ["return"] = "../statements/jump-statements#the-return-statement",
            ["sbyte"] = "../builtin-types/integral-numeric-types",
            ["sealed"] = "sealed",
            ["short"] = "../builtin-types/integral-numeric-types",
            ["sizeof"] = "../operators/sizeof",
            ["stackalloc"] = "../operators/stackalloc",
            ["static"] = "static",
            ["string"] = "../builtin-types/reference-types",
            ["struct"] = "../builtin-types/struct",
            ["switch"] = "../operators/switch-expression",
            ["this"] = "this",
            ["throw"] = "../statements/exception-handling-statements#the-throw-statement",
            ["true"] = "../builtin-types/bool",
            ["try"] = "../statements/exception-handling-statements#the-try-statement",
            ["typeof"] = "../operators/type-testing-and-cast#typeof-operator",
            ["uint"] = "../builtin-types/integral-numeric-types",
            ["ulong"] = "../builtin-types/integral-numeric-types",
            ["unchecked"] = "../statements/checked-and-unchecked",
            ["unsafe"] = "unsafe",
            ["ushort"] = "../builtin-types/integral-numeric-types",
            ["using"] = "using",
            ["virtual"] = "virtual",
            ["void"] = "../builtin-types/void",
            ["volatile"] = "volatile",
            ["while"] = "../statements/iteration-statements#the-while-statement",
            // Contextual keywords
            ["add"] = "add",
            ["allows"] = "where-generic-type-constraint",
            ["alias"] = "extern-alias",
            ["and"] = "../operators/patterns#logical-patterns",
            ["ascending"] = "ascending",
            ["args"] = "../../fundamentals/program-structure/top-level-statements#args",
            ["async"] = "async",
            ["await"] = "../operators/await",
            ["by"] = "by",
            ["descending"] = "descending",
            ["dynamic"] = "../builtin-types/reference-types",
            ["equals"] = "equals",
            ["extension"] = "extension",
            ["field"] = "field",
            ["file"] = "file",
            ["from"] = "from-clause",
            ["get"] = "get",
            ["global"] = "../operators/namespace-alias-qualifier",
            ["group"] = "group-clause",
            ["init"] = "init",
            ["into"] = "into",
            ["join"] = "join-clause",
            ["let"] = "let-clause",
            ["managed"] = "../unsafe-code#function-pointers",
            ["nameof"] = "../operators/nameof",
            ["nint"] = "../builtin-types/integral-numeric-types",
            ["not"] = "../operators/patterns#logical-patterns",
            ["notnull"] = "../../programming-guide/generics/constraints-on-type-parameters#notnull-constraint",
            ["nuint"] = "../builtin-types/integral-numeric-types",
            ["on"] = "on",
            ["or"] = "../operators/patterns#logical-patterns",
            ["orderby"] = "orderby-clause",
            ["partial"] = "partial-type",
            ["record"] = "../../fundamentals/types/records",
            ["remove"] = "remove",
            ["required"] = "required",
            ["scoped"] = "../statements/declarations#scoped-ref",
            ["select"] = "select-clause",
            ["set"] = "set",
            ["unmanaged"] = "../unsafe-code#function-pointers",
            ["value"] = "value",
            ["var"] = "../statements/declarations#implicitly-typed-local-variables",
            ["when"] = "when",
            ["where"] = "where-generic-type-constraint",
            ["with"] = "../operators/with-expression",
            ["yield"] = "../statements/yield"
        };

        /// <summary>
        /// A dictionary of system types names to their corresponding C# simplified names.
        /// </summary>
        private static readonly Dictionary<string, string> SystemTypeNames = new(StringComparer.Ordinal)
        {
            ["Void"] = "void",
            ["Boolean"] = "bool",
            ["Char"] = "char",
            ["String"] = "string",
            ["Byte"] = "byte",
            ["SByte"] = "sbyte",
            ["UInt16"] = "ushort",
            ["Int16"] = "short",
            ["UInt32"] = "uint",
            ["Int32"] = "int",
            ["UInt64"] = "ulong",
            ["Int64"] = "long",
            ["Single"] = "float",
            ["Double"] = "double",
            ["Decimal"] = "decimal",
            ["Object"] = "object",
        };

        /// <summary>
        /// A dictionary of operator method names to their corresponding operator symbols.
        /// </summary>
        private static readonly Dictionary<string, string> Operators = new(StringComparer.Ordinal)
        {
            ["Equality"] = "==",
            ["Inequality"] = "!=",
            ["GreaterThan"] = ">",
            ["LessThan"] = "<",
            ["GreaterThanOrEqual"] = ">=",
            ["LessThanOrEqual"] = "<=",
            ["BitwiseAnd"] = "&",
            ["BitwiseOr"] = "|",
            ["Addition"] = "+",
            ["Subtraction"] = "-",
            ["Division"] = "/",
            ["Modulus"] = "%",
            ["Multiply"] = "*",
            ["LeftShift"] = "<<",
            ["RightShift"] = ">>",
            ["ExclusiveOr"] = "^",
            ["UnaryNegation"] = "-",
            ["UnaryPlus"] = "+",
            ["LogicalNot"] = "!",
            ["OnesComplement"] = "~",
            ["False"] = "false",
            ["True"] = "true",
            ["Increment"] = "++",
            ["Decrement"] = "--",
            ["Implicit"] = "implicit",
            ["Explicit"] = "explicit",
            ["UnsignedRightShift"] = ">>>",
            ["AdditionAssignment"] = "+=",
            ["SubtractionAssignment"] = "-=",
            ["MultiplyAssignment"] = "*=",
            ["DivisionAssignment"] = "/=",
            ["ModulusAssignment"] = "%=",
            ["AndAssignment"] = "&=",
            ["OrAssignment"] = "|=",
            ["ExclOrAssignment"] = "^=",
            ["LeftShiftAssignment"] = "<<=",
            ["RightShiftAssignment"] = ">>=",
            ["UnsignedRightShiftAssignment"] = ">>>=",
            ["IncrementAssignment"] = "++",
            ["DecrementAssignment"] = "--",
        };

        /// <summary>
        /// Determines if the specified value is a C# keyword.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns><see langword="true"/> if the value is a C# keyword; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsLanguageKeyword(string value) => value is not null && KeywordLinks.ContainsKey(value);

        /// <summary>
        /// Attempts to get the documentation URL for a given C# keyword.
        /// </summary>
        /// <param name="keyword">The C# keyword to look up.</param>
        /// <param name="url">The documentation URL if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the keyword is found; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetKeywordUrl(string keyword, [NotNullWhen(true)] out Uri? url)
        {
            if (keyword is not null && KeywordLinks.TryGetValue(keyword, out var relativePath))
            {
                url = new Uri(KeywordsUrl, relativePath);
                return true;
            }

            url = null;
            return false;
        }

        /// <summary>
        /// Attempts to get the operator symbol for a given operator name.
        /// </summary>
        /// <param name="operatorName">The operator name to look up (such as "Equality" or "Addition").</param>
        /// <param name="symbol">When this method returns, contains the operator symbol if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the operator name is found; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetOperatorSymbol(string operatorName, [NotNullWhen(true)] out string? symbol)
        {
            if (operatorName is not null)
                return Operators.TryGetValue(operatorName, out symbol);

            symbol = null;
            return false;
        }

        /// <summary>
        /// Attempts to retrieve the simplified name of a system type.
        /// </summary>
        /// <param name="type">The system type to retrieve the simplified name for (such as "Int32" or "Boolean").</param>
        /// <param name="simplifiedName">When this method returns, contains the simplified name of the type, if found; otherwise, <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the simplified name is found; otherwise, <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetSimplifiedName(IType type, [NotNullWhen(true)] out string? simplifiedName)
        {
            if (type is not null && type.Namespace is nameof(System))
                return SystemTypeNames.TryGetValue(type.Name, out simplifiedName);

            simplifiedName = null;
            return false;
        }
    }
}
