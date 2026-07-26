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

using Transmitly.Template.Configuration;

namespace Transmitly.Channel.Configuration.WhatsApp;

internal sealed class WhatsAppContactConfiguration : IWhatsAppContactConfiguration
{
	private readonly List<IWhatsAppContactPhoneConfiguration> _phones = [];
	private readonly List<IWhatsAppContactEmailConfiguration> _emails = [];

	public IContentTemplateConfiguration FormattedName { get; } = new ContentTemplateConfiguration();

	public IContentTemplateConfiguration FirstName { get; } = new ContentTemplateConfiguration();

	public IContentTemplateConfiguration LastName { get; } = new ContentTemplateConfiguration();

	public IContentTemplateConfiguration Organization { get; } = new ContentTemplateConfiguration();

	public IReadOnlyCollection<IWhatsAppContactPhoneConfiguration> Phones => _phones.AsReadOnly();

	public IReadOnlyCollection<IWhatsAppContactEmailConfiguration> Emails => _emails.AsReadOnly();

	public IWhatsAppContactConfiguration AddPhone(Action<IWhatsAppContactPhoneConfiguration> phone)
	{
		Guard.AgainstNull(phone);
		var config = new WhatsAppContactPhoneConfiguration();
		phone(config);
		_phones.Add(config);
		return this;
	}

	public IWhatsAppContactConfiguration AddEmail(Action<IWhatsAppContactEmailConfiguration> email)
	{
		Guard.AgainstNull(email);
		var config = new WhatsAppContactEmailConfiguration();
		email(config);
		_emails.Add(config);
		return this;
	}
}
