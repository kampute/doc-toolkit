// Copyright (C) Kampute
//
// Released under the terms of the MIT license.
// See the LICENSE file in the project root for the full license text.

namespace Kampute.DocToolkit.Test.Metadata
{
    using Kampute.DocToolkit.Languages;
    using Kampute.DocToolkit.Metadata;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Documentation is generated from assemblies loaded through a Metadata Load Context, while most tests use runtime reflection.
    /// These tests use the runtime metadata, whose rendering the other tests pin down, as the reference for the loaded metadata.
    /// </summary>
    [TestFixture]
    public class MetadataLoadContextParityTests
    {
        [TestCase("Process", false, ExpectedResult = true)]
        [TestCase("Process", true, ExpectedResult = false)]
        [TestCase("ProcessArray", false, ExpectedResult = true)]
        [TestCase("ProcessArray", true, ExpectedResult = false)]
        public bool ImplementedMethod_FromMetadataLoadContext_MatchesActualInterfaceSignature(string name, bool stringOverload)
        {
            var assemblyPath = typeof(Acme.SampleListProcessor<>).Assembly.Location;
            using var universe = new MetadataUniverse([assemblyPath]);
            var type = universe.LoadFromPath(assemblyPath).GetType(typeof(Acme.SampleListProcessor<>).FullName!)!;
            var method = type.GetMethods(Acme.Bindings.AllDeclared).Single(m => m.Name == name
                && m.GetParameters()[0].ParameterType.ContainsGenericParameters != stringOverload);

            return ((IMethod)method.GetMetadata()).ImplementedMethod is not null;
        }

        [Test]
        public void AcmeMembers_FromMetadataLoadContext_HaveSameMembersAndDefinitionsAsFromRuntime()
        {
            var language = new CSharp();
            var assemblyPath = typeof(Acme.SampleMethods).Assembly.Location;
            using var universe = new MetadataUniverse([assemblyPath]);
            var loadedAssembly = universe.LoadFromPath(assemblyPath).GetMetadata();
            var runtimeAssembly = typeof(Acme.SampleMethods).Assembly.GetMetadata();

            var differences = new List<string>();
            foreach (var loadedType in loadedAssembly.ExportedTypes.Where(static t => t.Namespace == "Acme"))
            {
                Assert.That(runtimeAssembly.TryGetType(loadedType.FullName, out var runtimeType), Is.True, loadedType.FullName);

                var loaded = loadedType.GetMembersIncludingNested().ToDictionary(static m => m.CodeReference, m => language.FormatDefinition(m));
                var runtime = runtimeType!.GetMembersIncludingNested().ToDictionary(static m => m.CodeReference, m => language.FormatDefinition(m));

                differences.AddRange(runtime.Keys.Except(loaded.Keys).Select(static cref => $"missing: {cref}"));
                differences.AddRange(loaded.Keys.Except(runtime.Keys).Select(static cref => $"unexpected: {cref}"));
                differences.AddRange(loaded.Where(p => runtime.TryGetValue(p.Key, out var definition) && definition != p.Value)
                    .Select(p => $"{p.Key}: '{p.Value}' instead of '{runtime[p.Key]}'"));
            }

            Assert.That(differences, Is.Empty);
        }
    }
}
