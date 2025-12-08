// Copyright (C) 2025 Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata.Adapters
{
    using Kampute.DocToolkit.Metadata;
    using Kampute.DocToolkit.Metadata.Adapters;
    using Moq;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;

    [TestFixture]
    public class AdapterHelperTests
    {
        [TestCase("IFoo.Bar", "IFoo", "Bar")]
        [TestCase("System.IDisposable.Dispose", "System.IDisposable", "Dispose")]
        [TestCase("Namespace.IInterface.Method", "Namespace.IInterface", "Method")]
        [TestCase("IFoo<T>.Bar", "IFoo`1", "Bar")]
        [TestCase("IFoo<T,U>.Bar", "IFoo`2", "Bar")]
        [TestCase("IFoo<T,U,V>.Method", "IFoo`3", "Method")]
        [TestCase("IFoo<T<U>>.Bar", "IFoo`1", "Bar")]
        [TestCase("IFoo<T<U,V>>.Bar", "IFoo`1", "Bar")]
        [TestCase("IFoo<T<U>,V>.Method", "IFoo`2", "Method")]
        [TestCase("IFoo<T<U<W>>,V>.Method", "IFoo`2", "Method")]
        public void SplitExplicitName_ReturnsCorrectComponents(string explicitName, string expectedInterface, string expectedMember)
        {
            var (interfaceName, memberName) = AdapterHelper.SplitExplicitName(explicitName);

            using (Assert.EnterMultipleScope())
            {
                Assert.That(interfaceName, Is.EqualTo(expectedInterface));
                Assert.That(memberName, Is.EqualTo(expectedMember));
            }
        }

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
