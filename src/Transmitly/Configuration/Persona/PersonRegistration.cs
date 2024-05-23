// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
//  
//  Licensed under the Apache License, Version 2.0 (the "License")
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//  
//      http://www.apache.org/licenses/LICENSE-2.0
//  
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.

using System.Linq.Expressions;

namespace Transmitly.Persona.Configuration
{
    internal sealed class PersonRegistration<TPersona>(string name, string platformIdentityType, Expression<Func<TPersona, bool>> predicate) : IPersonaRegistration<TPersona>
        where TPersona : class
    {
        private readonly Func<TPersona, bool> _compiledExpression = Guard.AgainstNull(predicate).Compile();

        public string Name { get; } = Guard.AgainstNullOrWhiteSpace(name);
        public string PlatformIdentityType { get; } = Guard.AgainstNullOrWhiteSpace(platformIdentityType);

        public Type PersonaType => typeof(TPersona);

        public bool IsMatch(TPersona persona) => _compiledExpression(persona);

        public bool IsMatch(object persona) => IsMatch((TPersona)persona);
    }
}
