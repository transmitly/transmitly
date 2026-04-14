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

using System.Reflection;

namespace Transmitly;

internal static class DefaultActivator
{
	public static T? CreateInstance<T>(Type implementationType, ILoggerFactory loggerFactory, object? configuration = null)
		where T : class =>
		CreateInstance(implementationType, loggerFactory, configuration) as T;

	public static object? CreateInstance(Type implementationType, ILoggerFactory loggerFactory, object? configuration = null)
	{
		Guard.AgainstNull(implementationType);
		Guard.AgainstNull(loggerFactory);

		var constructors = implementationType
			.GetConstructors()
			.OrderByDescending(c => c.GetParameters().Length)
			.ToArray();

		foreach (var constructor in constructors)
		{
			if (TryResolveArguments(constructor.GetParameters(), loggerFactory, configuration, out var args))
				return constructor.Invoke(args);
		}

		return null;
	}

	private static bool TryResolveArguments(
		ParameterInfo[] parameters,
		ILoggerFactory loggerFactory,
		object? configuration,
		out object?[] arguments)
	{
		arguments = new object?[parameters.Length];

		for (var i = 0; i < parameters.Length; i++)
		{
			var parameter = parameters[i];
			if (typeof(ILoggerFactory).IsAssignableFrom(parameter.ParameterType))
			{
				arguments[i] = loggerFactory;
				continue;
			}

			if (configuration != null && parameter.ParameterType.IsInstanceOfType(configuration))
			{
				arguments[i] = configuration;
				continue;
			}

			if (TryCreateDefault(parameter.ParameterType, out var defaultValue))
			{
				arguments[i] = defaultValue;
				continue;
			}

			return false;
		}

		return true;
	}

	private static bool TryCreateDefault(Type type, out object? value)
	{
		if (!type.IsAbstract && !type.IsInterface)
		{
			try
			{
				value = Activator.CreateInstance(type);
				return value != null || Nullable.GetUnderlyingType(type) != null || !type.IsValueType;
			}
			catch
			{
				//intentionally ignored - if we can't create an instance, we'll just return false
			}
		}

		value = null;
		return false;
	}
}
