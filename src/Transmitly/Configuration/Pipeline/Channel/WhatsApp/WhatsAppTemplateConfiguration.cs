// Copyright (c) Code Impressions, LLC. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License")
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Transmitly.Channel.WhatsApp;
using Transmitly.Template.Configuration;

namespace Transmitly.Channel.Configuration.WhatsApp;

internal sealed class WhatsAppTemplateConfiguration : IWhatsAppTemplateConfiguration
{
	private readonly List<IContentTemplateConfiguration> _headerParameters = [];
	private readonly List<IContentTemplateConfiguration> _bodyParameters = [];
	private readonly List<IWhatsAppTemplateActionConfiguration> _actions = [];

	public string? Name { get; private set; }

	public string? Language { get; private set; }

	public IReadOnlyCollection<IContentTemplateConfiguration> HeaderParameters => _headerParameters.AsReadOnly();

	public IReadOnlyCollection<IContentTemplateConfiguration> BodyParameters => _bodyParameters.AsReadOnly();

	public IReadOnlyCollection<IWhatsAppTemplateActionConfiguration> Actions => _actions.AsReadOnly();

	public IWhatsAppTemplateConfiguration AddName(string name)
	{
		Name = Guard.AgainstNullOrWhiteSpace(name);
		return this;
	}

	public IWhatsAppTemplateConfiguration AddLanguage(string language)
	{
		Language = Guard.AgainstNullOrWhiteSpace(language);
		return this;
	}

	public IWhatsAppTemplateConfiguration AddHeaderParameter(string? value)
	{
		return AddParameter(_headerParameters, template => template.AddStringTemplate(value ?? string.Empty));
	}

	public IWhatsAppTemplateConfiguration AddHeaderParameter(Action<IContentTemplateConfiguration> parameter)
	{
		return AddParameter(_headerParameters, parameter);
	}

	public IWhatsAppTemplateConfiguration AddHeaderParameter(Func<IDispatchCommunicationContext, Task<string?>> parameterResolver)
	{
		return AddParameter(_headerParameters, template => template.AddTemplateResolver(parameterResolver));
	}

	public IWhatsAppTemplateConfiguration AddBodyParameter(string? value)
	{
		return AddParameter(_bodyParameters, template => template.AddStringTemplate(value ?? string.Empty));
	}

	public IWhatsAppTemplateConfiguration AddBodyParameter(Action<IContentTemplateConfiguration> parameter)
	{
		return AddParameter(_bodyParameters, parameter);
	}

	public IWhatsAppTemplateConfiguration AddBodyParameter(Func<IDispatchCommunicationContext, Task<string?>> parameterResolver)
	{
		return AddParameter(_bodyParameters, template => template.AddTemplateResolver(parameterResolver));
	}

	public IWhatsAppTemplateConfiguration AddAction(WhatsAppTemplateActionType actionType, int index, Action<IWhatsAppTemplateActionConfiguration> action)
	{
		Guard.AgainstNull(action);
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}

		var config = new WhatsAppTemplateActionConfiguration(actionType, index);
		action(config);
		_actions.Add(config);
		return this;
	}

	private IWhatsAppTemplateConfiguration AddParameter(ICollection<IContentTemplateConfiguration> target, Action<IContentTemplateConfiguration> configure)
	{
		Guard.AgainstNull(configure);
		var template = new ContentTemplateConfiguration();
		configure(template);
		target.Add(template);
		return this;
	}
}
