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

namespace Transmitly.Persona.Configuration
{
	public abstract class BasePersonaFactory(IEnumerable<IPersonaRegistration> personaRegistrations) : IPersonaFactory
	{
		private readonly List<IPersonaRegistration> _personaRegistrations = personaRegistrations.ToList();

		public Task<IReadOnlyCollection<IPersonaRegistration>> GetAllAsync()
		{
			return Task.FromResult<IReadOnlyCollection<IPersonaRegistration>>(_personaRegistrations.AsReadOnly());
		}

		public Task<IPersonaRegistration?> GetAsync(string personaName)
		{
			return Task.FromResult<IPersonaRegistration?>(
				_personaRegistrations.Find(f => f.Name.Equals(personaName, StringComparison.OrdinalIgnoreCase))
			);
		}

		public virtual Task<bool> AnyMatch<TPersona>(string personaName, IReadOnlyCollection<TPersona> personas)
			where TPersona : class
		{
			Guard.AgainstNullOrWhiteSpace(personaName);
			Guard.AgainstNull(personas);

			var regs = _personaRegistrations
				.Where(x =>
					x.Name.Equals(personaName, StringComparison.OrdinalIgnoreCase)
				);
			return Task.FromResult(regs.Any(r => personas.Any(x => r.IsMatch(x))));
		}
	}
}