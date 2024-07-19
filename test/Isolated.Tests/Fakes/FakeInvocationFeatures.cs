// <copyright file="FakeInvocationFeatures.cs" company="DarkLoop" author="Arturo Martinez">
//  Copyright (c) DarkLoop. All rights reserved.
// </copyright>

using Microsoft.Azure.Functions.Worker;
using System.Collections;

namespace Isolated.Tests.Fakes
{
    internal sealed class FakeInvocationFeatures : IInvocationFeatures
    {
        private Dictionary<Type, object> _underlyingSet = new Dictionary<Type, object>();

        public T? Get<T>()
        {
            _underlyingSet.TryGetValue(typeof(T), out var feature);

            return (T?)feature;
        }

        public IEnumerator<KeyValuePair<Type, object>> GetEnumerator()
        {
            return _underlyingSet.GetEnumerator();
        }

        public void Set<T>(T instance)
        {
            _underlyingSet.Add(typeof(T), instance);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _underlyingSet.GetEnumerator();
        }
    }
}
