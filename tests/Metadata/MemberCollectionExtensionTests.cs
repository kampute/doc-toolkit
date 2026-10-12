// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Metadata;
    using Moq;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class MemberCollectionExtensionTests
    {
        [Test]
        public void FindByFullName_WithEmptyCollection_ReturnsNull()
        {
            var types = CreateSortedTypes();

            var result = types.FindByFullName("SomeType");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void FindByFullName_WithNoMatch_ReturnsNull()
        {
            var types = CreateSortedTypes("System.Collections.ArrayList", "System.Collections.List", "System.String", "System.Text.StringBuilder");

            var result = types.FindByFullName("System.Collections.Dictionary");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void FindByFullName_WithSingleMatch_ReturnsType()
        {
            var types = CreateSortedTypes("System.Collections.ArrayList", "System.Collections.List", "System.String", "System.Text.StringBuilder");

            var result = types.FindByFullName("System.String");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FullName, Is.EqualTo("System.String"));
        }

        [Test]
        public void FindByFullName_WithMatchAtStart_ReturnsType()
        {
            var types = CreateSortedTypes("System.Collections.ArrayList", "System.Collections.List", "System.String", "System.Text.StringBuilder");

            var result = types.FindByFullName("System.Collections.ArrayList");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FullName, Is.EqualTo("System.Collections.ArrayList"));
        }

        [Test]
        public void FindByFullName_WithMatchAtEnd_ReturnsType()
        {
            var types = CreateSortedTypes("System.Collections.ArrayList", "System.Collections.List", "System.String", "System.Text.StringBuilder");

            var result = types.FindByFullName("System.Text.StringBuilder");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FullName, Is.EqualTo("System.Text.StringBuilder"));
        }

        [Test]
        public void FindByFullName_WithFullNameBeforeFirstElement_ReturnsNull()
        {
            var types = CreateSortedTypes("System.Collections.List", "System.String", "System.Text.StringBuilder");

            var result = types.FindByFullName("System.Collections.ArrayList");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void FindByFullName_WithFullNameAfterLastElement_ReturnsNull()
        {
            var types = CreateSortedTypes("System.Collections.ArrayList", "System.Collections.List", "System.String");

            var result = types.FindByFullName("System.Xml.XmlDocument");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void FindByFullName_WithSimilarPrefixes_ReturnsCorrectType()
        {
            var types = CreateSortedTypes("MyNamespace.Type", "MyNamespace.TypeA", "MyNamespace.TypeAB", "MyNamespace.TypeB");

            var result = types.FindByFullName("MyNamespace.TypeA");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FullName, Is.EqualTo("MyNamespace.TypeA"));
        }

        [Test]
        public void FindByFullName_WithSingleElement_FindsCorrectly()
        {
            var types = CreateSortedTypes("System.String");

            var result = types.FindByFullName("System.String");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FullName, Is.EqualTo("System.String"));
        }

        [Test]
        public void FindByFullName_WithSingleElement_NoMatch_ReturnsNull()
        {
            var types = CreateSortedTypes("System.String");

            var result = types.FindByFullName("System.Int32");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void FindIndexByName_WithEmptyCollection_ReturnsMinusOne()
        {
            var members = CreateSortedMembers();

            var result = members.FindIndexByName("SomeName");

            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void FindIndexByName_WithNoMatch_ReturnsMinusOne()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Date");

            var result = members.FindIndexByName("Orange");

            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void FindIndexByName_WithSingleMatch_ReturnsCorrectIndex()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Date");

            var result = members.FindIndexByName("Cherry");

            Assert.That(result, Is.EqualTo(2));
        }

        [Test]
        public void FindIndexByName_WithMultipleMatches_ReturnsCorrectIndex()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Banana", "Banana", "Cherry");

            var result = members.FindIndexByName("Banana");

            Assert.That(result, Is.InRange(1, 3));
        }

        [Test]
        public void FindIndexByName_WithMatchAtStart_ReturnsZero()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Date");

            var result = members.FindIndexByName("Apple");

            Assert.That(result, Is.Zero);
        }

        [Test]
        public void FindIndexByName_WithMatchAtEnd_ReturnsLastIndex()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Date");

            var result = members.FindIndexByName("Date");

            Assert.That(result, Is.EqualTo(3));
        }

        [Test]
        public void FindIndexByName_WithNameBeforeFirstElement_ReturnsMinusOne()
        {
            var members = CreateSortedMembers("Banana", "Cherry", "Date");

            var result = members.FindIndexByName("Apple");

            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void FindIndexByName_WithNameAfterLastElement_ReturnsMinusOne()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry");

            var result = members.FindIndexByName("Zebra");

            Assert.That(result, Is.EqualTo(-1));
        }

        [Test]
        public void FindByName_WithEmptyCollection_ReturnsNull()
        {
            var members = CreateSortedMembers();

            var result = members.FindByName("SomeName");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void FindByName_WithNoMatch_ReturnsNull()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Date");

            var result = members.FindByName("Orange");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void FindByName_WithSingleMatch_ReturnsMember()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Date");

            var result = members.FindByName("Cherry");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Cherry"));
        }

        [Test]
        public void FindByName_WithMultipleMatches_ReturnsOneMember()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Banana", "Banana", "Cherry");

            var result = members.FindByName("Banana");

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo("Banana"));
        }

        [Test]
        public void FindByName_WithMatchAtStart_ReturnsMember()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Date");

            var result = members.FindByName("Apple");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Apple"));
        }

        [Test]
        public void FindByName_WithMatchAtEnd_ReturnsMember()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Date");

            var result = members.FindByName("Date");

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Date"));
        }

        [Test]
        public void FindByName_WithNameBeforeFirstElement_ReturnsNull()
        {
            var members = CreateSortedMembers("Banana", "Cherry", "Date");

            var result = members.FindByName("Apple");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void FindByName_WithNameAfterLastElement_ReturnsNull()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry");

            var result = members.FindByName("Zebra");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void WhereName_WithEmptyCollection_ReturnsEmpty()
        {
            var members = CreateSortedMembers();

            var result = members.WhereName("SomeName");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void WhereName_WithNoMatch_ReturnsEmpty()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Date");

            var result = members.WhereName("Orange");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void WhereName_WithSingleMatch_ReturnsSingleMember()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Date");

            var result = members.WhereName("Cherry").ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Select(static e => e.Name), Is.EqualTo(["Cherry"]));
        }

        [Test]
        public void WhereName_WithMultipleMatches_ReturnsAllMatches()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Banana", "Banana", "Cherry");

            var result = members.WhereName("Banana").ToList();

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result.All(static m => m.Name == "Banana"), Is.True);
        }

        [Test]
        public void WhereName_WithMatchAtStart_ReturnsMatches()
        {
            var members = CreateSortedMembers("Apple", "Apple", "Banana", "Cherry");

            var result = members.WhereName("Apple").ToList();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(static m => m.Name == "Apple"), Is.True);
        }

        [Test]
        public void WhereName_WithMatchAtEnd_ReturnsMatches()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Cherry");

            var result = members.WhereName("Cherry").ToList();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.All(static m => m.Name == "Cherry"), Is.True);
        }

        [Test]
        public void WhereName_WithAllSameName_ReturnsAllMembers()
        {
            var members = CreateSortedMembers("Method", "Method", "Method", "Method");

            var result = members.WhereName("Method").ToList();

            Assert.That(result, Has.Count.EqualTo(4));
        }

        [Test]
        public void WhereName_WithNameBeforeFirstElement_ReturnsEmpty()
        {
            var members = CreateSortedMembers("Banana", "Cherry", "Date");

            var result = members.WhereName("Apple");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void WhereName_WithNameAfterLastElement_ReturnsEmpty()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry");

            var result = members.WhereName("Zebra");

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void WhereName_PreservingOrder_WithEmptyCollection_ReturnsEmpty()
        {
            var members = CreateSortedMembers();

            var result = members.WhereName("SomeName", preserveOrder: true);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void WhereName_PreservingOrder_WithNoMatch_ReturnsEmpty()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Date");

            var result = members.WhereName("Orange", preserveOrder: true);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void WhereName_PreservingOrder_WithSingleMatch_ReturnsSingleMember()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry", "Date");

            var result = members.WhereName("Cherry", preserveOrder: true).ToList();

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result.Select(static e => e.Name), Is.EqualTo(["Cherry"]));
        }

        [Test]
        public void WhereName_PreservingOrder_WithMultipleMatches_ReturnsMatchesInOrder()
        {
            var members = CreateSortedMembersWithIds
            (
                ("Apple", 1),
                ("Banana", 2),
                ("Banana", 3),
                ("Banana", 4),
                ("Cherry", 5)
            );

            var result = members.WhereName("Banana", preserveOrder: true).ToList();

            Assert.That(result, Has.Count.EqualTo(3));
            Assert.That(result.Select(GetMemberId), Is.EqualTo([2, 3, 4]));
        }

        [Test]
        public void WhereName_PreservingOrder_WithMatchAtStart_ReturnsMatchesInOrder()
        {
            var members = CreateSortedMembersWithIds
            (
                ("Apple", 1),
                ("Apple", 2),
                ("Banana", 3),
                ("Cherry", 4)
            );

            var result = members.WhereName("Apple", preserveOrder: true).ToList();

            Assert.That(result, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.All(static m => m.Name == "Apple"), Is.True);
                Assert.That(result.Select(GetMemberId), Is.EqualTo([1, 2]));
            }
        }

        [Test]
        public void WhereName_PreservingOrder_WithMatchAtEnd_ReturnsMatchesInOrder()
        {
            var members = CreateSortedMembersWithIds
            (
                ("Apple", 1),
                ("Banana", 2),
                ("Cherry", 3),
                ("Cherry", 4)
            );

            var result = members.WhereName("Cherry", preserveOrder: true).ToList();

            Assert.That(result, Has.Count.EqualTo(2));
            using (Assert.EnterMultipleScope())
            {
                Assert.That(result.All(static m => m.Name == "Cherry"), Is.True);
                Assert.That(result.Select(GetMemberId), Is.EqualTo([3, 4]));
            }
        }

        [Test]
        public void WhereName_PreservingOrder_WithAllSameName_ReturnsAllMembersInOrder()
        {
            var members = CreateSortedMembersWithIds
            (
                ("Method", 1),
                ("Method", 2),
                ("Method", 3),
                ("Method", 4)
            );

            var result = members.WhereName("Method", preserveOrder: true).ToList();

            Assert.That(result, Has.Count.EqualTo(4));
            Assert.That(result.Select(GetMemberId), Is.EqualTo([1, 2, 3, 4]));
        }

        [Test]
        public void WhereName_PreservingOrder_WithNameBeforeFirstElement_ReturnsEmpty()
        {
            var members = CreateSortedMembers("Banana", "Cherry", "Date");

            var result = members.WhereName("Apple", preserveOrder: true);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void WhereName_PreservingOrder_WithNameAfterLastElement_ReturnsEmpty()
        {
            var members = CreateSortedMembers("Apple", "Banana", "Cherry");

            var result = members.WhereName("Zebra", preserveOrder: true);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public void FindByFullName_WithNullArguments_ThrowsArgumentNullException()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(static () => ((IReadOnlyList<IType>)null!).FindByFullName("A"), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("types"));
                Assert.That(static () => CreateSortedTypes("A").FindByFullName(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("fullName"));
            }
        }

        [Test]
        public void FindIndexByName_WithNullArguments_ThrowsArgumentNullException()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(static () => ((IReadOnlyList<IMember>)null!).FindIndexByName("A"), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("members"));
                Assert.That(static () => CreateSortedMembers("A").FindIndexByName(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("name"));
            }
        }

        [Test]
        public void WhereName_WithNullArguments_ThrowsBeforeEnumeration()
        {
            using (Assert.EnterMultipleScope())
            {
                Assert.That(static () => ((IReadOnlyList<IMember>)null!).WhereName("A"), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("members"));
                Assert.That(static () => CreateSortedMembers("A").WhereName(null!), Throws.ArgumentNullException.With.Property("ParamName").EqualTo("name"));
            }
        }

        [Test]
        public void OrderByFullName_UsesOrdinalComparison()
        {
            var types = CreateSortedTypes("a.Type", "B.Type", "A.Type");

            var result = types.OrderByFullName().Select(static t => t.FullName);

            Assert.That(result, Is.EqualTo(new[] { "A.Type", "B.Type", "a.Type" }));
        }

        [Test]
        public void OrderByParameterCount_OrdersConstructorsByParameterCount()
        {
            var constructors = new[] { CreateConstructor(2), CreateConstructor(0), CreateConstructor(1) };

            var result = constructors.OrderByParameterCount().Select(static c => c.Parameters.Count);

            Assert.That(result, Is.EqualTo(new[] { 0, 1, 2 }));
        }

        [Test]
        public void OrderByName_OrdersMethodsByNameThenTypeParameterCountThenParameterCount()
        {
            var methods = new[]
            {
                CreateMethod("b", 0, 0),
                CreateMethod("a", 1, 0),
                CreateMethod("a", 0, 2),
                CreateMethod("a", 0, 1),
                CreateMethod("B", 0, 0),
            };

            var result = methods.OrderByName().Select(static m => $"{m.Name}<{m.TypeParameters.Count}>({m.Parameters.Count})");

            Assert.That(result, Is.EqualTo(new[] { "B<0>(0)", "a<0>(1)", "a<0>(2)", "a<1>(0)", "b<0>(0)" }));
        }

        [Test]
        public void OrderByName_OrdersPropertiesByNameThenIndexParameterCount()
        {
            var properties = new[] { CreateProperty("Item", 2), CreateProperty("Count", 0), CreateProperty("Item", 1) };

            var result = properties.OrderByName().Select(static p => $"{p.Name}({p.Parameters.Count})");

            Assert.That(result, Is.EqualTo(new[] { "Count(0)", "Item(1)", "Item(2)" }));
        }

        [Test]
        public void OrderByName_OrdersOperatorsByNameThenParameterCount()
        {
            var operators = new[] { CreateOperator("op_Subtraction", 2), CreateOperator("op_Addition", 2), CreateOperator("op_Subtraction", 1) };

            var result = operators.OrderByName().Select(static o => $"{o.Name}({o.Parameters.Count})");

            Assert.That(result, Is.EqualTo(new[] { "op_Addition(2)", "op_Subtraction(1)", "op_Subtraction(2)" }));
        }

        private static IReadOnlyList<IParameter> CreateParameters(int count)
            => [.. Enumerable.Range(0, count).Select(static _ => Mock.Of<IParameter>())];

        private static IConstructor CreateConstructor(int parameterCount)
        {
            var mock = new Mock<IConstructor>();
            mock.SetupGet(static c => c.Parameters).Returns(CreateParameters(parameterCount));
            return mock.Object;
        }

        private static IMethod CreateMethod(string name, int typeParameterCount, int parameterCount)
        {
            var mock = new Mock<IMethod>();
            mock.SetupGet(static m => m.Name).Returns(name);
            mock.SetupGet(static m => m.TypeParameters).Returns([.. Enumerable.Range(0, typeParameterCount).Select(static _ => Mock.Of<ITypeParameter>())]);
            mock.SetupGet(static m => m.Parameters).Returns(CreateParameters(parameterCount));
            return mock.Object;
        }

        private static IProperty CreateProperty(string name, int parameterCount)
        {
            var mock = new Mock<IProperty>();
            mock.SetupGet(static p => p.Name).Returns(name);
            mock.SetupGet(static p => p.Parameters).Returns(CreateParameters(parameterCount));
            return mock.Object;
        }

        private static IOperator CreateOperator(string name, int parameterCount)
        {
            var mock = new Mock<IOperator>();
            mock.SetupGet(static o => o.Name).Returns(name);
            mock.SetupGet(static o => o.Parameters).Returns(CreateParameters(parameterCount));
            return mock.Object;
        }

        private static List<IType> CreateSortedTypes(params string[] fullNames)
        {
            var types = new List<IType>();
            foreach (var fullName in fullNames)
            {
                var mock = new Mock<IType>();
                mock.SetupGet(static t => t.FullName).Returns(fullName);
                types.Add(mock.Object);
            }
            return types;
        }

        private static List<IMember> CreateSortedMembers(params string[] names)
        {
            var members = new List<IMember>();
            foreach (var name in names)
            {
                var mock = new Mock<IMember>();
                mock.SetupGet(static m => m.Name).Returns(name);
                members.Add(mock.Object);
            }
            return members;
        }

        private static List<IMember> CreateSortedMembersWithIds(params (string Name, int Id)[] items)
        {
            var members = new List<IMember>();
            foreach (var (name, id) in items)
            {
                var mock = new Mock<IMember>();
                mock.SetupGet(static m => m.Name).Returns(name);
                mock.SetupGet(static m => m.CodeReference).Returns($"M:{id}");
                members.Add(mock.Object);
            }
            return members;
        }

        private static int GetMemberId(IMember member) => int.Parse(member.CodeReference[2..]);
    }
}
