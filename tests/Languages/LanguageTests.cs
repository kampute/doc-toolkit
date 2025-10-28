// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Languages
{
    using Kampute.DocToolkit.Languages;
    using NUnit.Framework;

    [TestFixture]
    public class LanguageTests
    {
        [Test]
        public void IsValidIdentifier_WithNullInput_ReturnsFalse()
        {
            Assert.That(Language.IsValidIdentifier(null!), Is.False);
        }

        [TestCase("", ExpectedResult = false)]
        [TestCase("validIdentifier", ExpectedResult = true)]
        [TestCase("_validIdentifier", ExpectedResult = true)]
        [TestCase("ValidIdentifier", ExpectedResult = true)]
        [TestCase("_ValidIdentifier", ExpectedResult = true)]
        [TestCase("identifier123", ExpectedResult = true)]
        [TestCase("_identifier123", ExpectedResult = true)]
        [TestCase("identifier_123", ExpectedResult = true)]
        [TestCase("_", ExpectedResult = true)]
        [TestCase("__", ExpectedResult = true)]
        [TestCase("___identifier___", ExpectedResult = true)]
        [TestCase("1identifier", ExpectedResult = false)]
        [TestCase("123", ExpectedResult = false)]
        [TestCase("identifier-name", ExpectedResult = false)]
        [TestCase("identifier.name", ExpectedResult = false)]
        [TestCase("identifier name", ExpectedResult = false)]
        [TestCase("identifier@name", ExpectedResult = false)]
        [TestCase("identifier#name", ExpectedResult = false)]
        [TestCase("identifier$name", ExpectedResult = false)]
        [TestCase("identifier%name", ExpectedResult = false)]
        [TestCase("identifier&name", ExpectedResult = false)]
        [TestCase("identifier*name", ExpectedResult = false)]
        [TestCase("identifier+name", ExpectedResult = false)]
        [TestCase("identifier=name", ExpectedResult = false)]
        [TestCase("identifier!name", ExpectedResult = false)]
        [TestCase("identifier?name", ExpectedResult = false)]
        [TestCase("identifier<name", ExpectedResult = false)]
        [TestCase("identifier>name", ExpectedResult = false)]
        [TestCase("identifier/name", ExpectedResult = false)]
        [TestCase("identifier\\name", ExpectedResult = false)]
        [TestCase("identifier|name", ExpectedResult = false)]
        [TestCase("identifier[name", ExpectedResult = false)]
        [TestCase("identifier]name", ExpectedResult = false)]
        [TestCase("identifier{name", ExpectedResult = false)]
        [TestCase("identifier}name", ExpectedResult = false)]
        [TestCase("identifier(name", ExpectedResult = false)]
        [TestCase("identifier)name", ExpectedResult = false)]
        [TestCase("identifier:name", ExpectedResult = false)]
        [TestCase("identifier;name", ExpectedResult = false)]
        [TestCase("identifier,name", ExpectedResult = false)]
        [TestCase("identifier\"name", ExpectedResult = false)]
        [TestCase("identifier'name", ExpectedResult = false)]
        [TestCase("identifier`name", ExpectedResult = false)]
        [TestCase("identifier~name", ExpectedResult = false)]
        [TestCase("identifier\nnewline", ExpectedResult = false)]
        [TestCase("identifier\ttab", ExpectedResult = false)]
        public bool IsValidIdentifier_WithVariousInputs_ReturnsExpectedResult(string identifier)
        {
            return Language.IsValidIdentifier(identifier);
        }

        [Test]
        public void IsValidNamespace_WithNullInput_ReturnsFalse()
        {
            Assert.That(Language.IsValidNamespace(null!), Is.False);
        }

        [TestCase("", ExpectedResult = false)]
        [TestCase("System", ExpectedResult = true)]
        [TestCase("System.Collections", ExpectedResult = true)]
        [TestCase("System.Collections.Generic", ExpectedResult = true)]
        [TestCase("_Namespace", ExpectedResult = true)]
        [TestCase("_Namespace._Part", ExpectedResult = true)]
        [TestCase("Namespace123", ExpectedResult = true)]
        [TestCase("Namespace123.Part456", ExpectedResult = true)]
        [TestCase("My_Namespace.My_Part", ExpectedResult = true)]
        [TestCase("A.B.C.D.E", ExpectedResult = true)]
        [TestCase("Kampute.DocToolkit.Languages", ExpectedResult = true)]
        [TestCase(".", ExpectedResult = false)]
        [TestCase(".System", ExpectedResult = false)]
        [TestCase("System.", ExpectedResult = false)]
        [TestCase("System..Collections", ExpectedResult = false)]
        [TestCase("System.1Collections", ExpectedResult = false)]
        [TestCase("System.Collections-Generic", ExpectedResult = false)]
        [TestCase("System.Collections Generic", ExpectedResult = false)]
        [TestCase("System.Collections@Generic", ExpectedResult = false)]
        [TestCase("System.Collections#Generic", ExpectedResult = false)]
        [TestCase("System.Collections$Generic", ExpectedResult = false)]
        [TestCase("System.Collections%Generic", ExpectedResult = false)]
        [TestCase("System.Collections&Generic", ExpectedResult = false)]
        [TestCase("System.Collections*Generic", ExpectedResult = false)]
        [TestCase("System.Collections+Generic", ExpectedResult = false)]
        [TestCase("System.Collections=Generic", ExpectedResult = false)]
        [TestCase("System.Collections!Generic", ExpectedResult = false)]
        [TestCase("System.Collections?Generic", ExpectedResult = false)]
        [TestCase("System.Collections<Generic", ExpectedResult = false)]
        [TestCase("System.Collections>Generic", ExpectedResult = false)]
        [TestCase("System.Collections/Generic", ExpectedResult = false)]
        [TestCase("System.Collections\\Generic", ExpectedResult = false)]
        [TestCase("System.Collections|Generic", ExpectedResult = false)]
        [TestCase("System.Collections[Generic", ExpectedResult = false)]
        [TestCase("System.Collections]Generic", ExpectedResult = false)]
        [TestCase("System.Collections{Generic", ExpectedResult = false)]
        [TestCase("System.Collections}Generic", ExpectedResult = false)]
        [TestCase("System.Collections(Generic", ExpectedResult = false)]
        [TestCase("System.Collections)Generic", ExpectedResult = false)]
        [TestCase("System.Collections:Generic", ExpectedResult = false)]
        [TestCase("System.Collections;Generic", ExpectedResult = false)]
        [TestCase("System.Collections,Generic", ExpectedResult = false)]
        [TestCase("System.Collections\"Generic", ExpectedResult = false)]
        [TestCase("System.Collections'Generic", ExpectedResult = false)]
        [TestCase("System.Collections`Generic", ExpectedResult = false)]
        [TestCase("System.Collections~Generic", ExpectedResult = false)]
        [TestCase("System\nNewline", ExpectedResult = false)]
        [TestCase("System\tTab", ExpectedResult = false)]
        [TestCase("System.Collections\nGeneric", ExpectedResult = false)]
        [TestCase("System.Collections\tGeneric", ExpectedResult = false)]
        public bool IsValidNamespace_WithVariousInputs_ReturnsExpectedResult(string namespaceName)
        {
            return Language.IsValidNamespace(namespaceName);
        }
    }
}
