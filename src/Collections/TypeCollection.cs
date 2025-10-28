namespace Kampute.DocToolkit.Collections
{
    using Kampute.DocToolkit.Models;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a read-only collection of type models in a documentation context.
    /// </summary>
    /// <remarks>
    /// This class provides immutable access to type model instances in a documentation context. It categorizes types into classes,
    /// structs, interfaces, enums, and delegates for easy retrieval and enumeration.
    /// </remarks>
    public class TypeCollection : IReadOnlyTypeCollection
    {
        private readonly IEnumerable<TypeModel> types;
        private readonly List<ClassModel> classModels = [];
        private readonly List<StructModel> structModels = [];
        private readonly List<InterfaceModel> interfaceModels = [];
        private readonly List<EnumModel> enumModels = [];
        private readonly List<DelegateModel> delegateModels = [];

        /// <summary>
        /// Gets an empty <see cref="TypeCollection"/> instance.
        /// </summary>
        /// <value>
        /// An empty <see cref="TypeCollection"/>.
        /// </value>
        public static readonly TypeCollection Empty = new([]);

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeCollection"/> class.
        /// </summary>
        /// <param name="types">The collection of type models to include in this collection.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="types"/> is <see langword="null"/>.</exception>
        public TypeCollection(IEnumerable<TypeModel> types)
        {
            this.types = types switch
            {
                null => throw new ArgumentNullException(nameof(types)),
                IReadOnlyCollection<TypeModel> readOnlyCollection => readOnlyCollection,
                ICollection<TypeModel> collection => collection,
                _ => [.. types],
            };

            var count = 0;
            foreach (var type in this.types)
            {
                count++;
                switch (type)
                {
                    case ClassModel classModel:
                        classModels.Add(classModel);
                        break;
                    case StructModel structModel:
                        structModels.Add(structModel);
                        break;
                    case InterfaceModel interfaceModel:
                        interfaceModels.Add(interfaceModel);
                        break;
                    case EnumModel enumModel:
                        enumModels.Add(enumModel);
                        break;
                    case DelegateModel delegateModel:
                        delegateModels.Add(delegateModel);
                        break;
                }
            }
            Count = count;
        }

        /// <summary>
        /// Gets the total number of type models in the collection.
        /// </summary>
        /// <value>
        /// The total count of type models in the collection.
        /// </value>
        public int Count { get; }

        /// <summary>
        /// Gets all class type models in the collection.
        /// </summary>
        /// <value>
        /// A read-only collection <see cref="ClassModel"/> containing all class type models in the collection.
        /// </value>
        public IReadOnlyCollection<ClassModel> Classes => classModels;

        /// <summary>
        /// Gets all struct type models in the collection.
        /// </summary>
        /// <value>
        /// A read-only collection <see cref="StructModel"/> containing all struct type models in the collection.
        /// </value>
        public IReadOnlyCollection<StructModel> Structs => structModels;

        /// <summary>
        /// Gets all interface type models in the collection.
        /// </summary>
        /// <value>
        /// A read-only collection <see cref="InterfaceModel"/> containing all interface type models in the collection.
        /// </value>
        public IReadOnlyCollection<InterfaceModel> Interfaces => interfaceModels;

        /// <summary>
        /// Gets all enum type models in the collection.
        /// </summary>
        /// <value>
        /// A read-only collection <see cref="EnumModel"/> containing all enum type models in the collection.
        /// </value>
        public IReadOnlyCollection<EnumModel> Enums => enumModels;

        /// <summary>
        /// Gets all delegate type models in the collection.
        /// </summary>
        /// <value>
        /// A read-only collection <see cref="DelegateModel"/> containing all delegate type models in the collection.
        /// </value>
        public IReadOnlyCollection<DelegateModel> Delegates => delegateModels;

        /// <summary>
        /// Returns an enumerator that iterates through the collection of type models.
        /// </summary>
        /// <returns>An enumerator for the collection of type models.</returns>
        public IEnumerator<TypeModel> GetEnumerator() => types.GetEnumerator();

        /// <summary>
        /// Returns an enumerator that iterates through the collection of type models.
        /// </summary>
        /// <returns>An enumerator for the collection of type models.</returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
