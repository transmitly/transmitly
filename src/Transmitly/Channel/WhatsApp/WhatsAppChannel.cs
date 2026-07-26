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

using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using Transmitly.Channel.Configuration;
using Transmitly.Channel.Configuration.WhatsApp;
using Transmitly.Exceptions;
using Transmitly.Template.Configuration;

namespace Transmitly.Channel.WhatsApp;

#if FEATURE_SOURCE_GEN
internal sealed partial class WhatsAppChannel(IWhatsAppChannelConfiguration configuration) : IChannel<IWhatsApp>
#else
internal sealed class WhatsAppChannel(IWhatsAppChannelConfiguration configuration) : IChannel<IWhatsApp>
#endif
{
	private const string Pattern = @"^\+?[1-9]\d{1,14}$";
	private const RegexOptions Options = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture;
	private const string WhatsAppPrefix = "whatsapp:";

	private readonly IWhatsAppChannelConfiguration _configuration = Guard.AgainstNull(configuration);
	private static readonly Regex _whatsAppMatchRegex = CreateRegEx();

	public Type CommunicationType => typeof(IWhatsApp);

	public string Id => Transmitly.Id.Channel.WhatsApp();

	public IEnumerable<string> AllowedChannelProviderIds => _configuration.ChannelProviderFilter ?? Array.Empty<string>();

	public IExtendedProperties ExtendedProperties => _configuration.ExtendedProperties;

	public async Task<IWhatsApp> GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
	{
		Guard.AgainstNull(communicationContext);

		var body = await _configuration.Message.RenderAsync(communicationContext, false).ConfigureAwait(false);
		var attachments = ConvertAttachments(communicationContext);
		var location = await RenderLocationAsync(communicationContext).ConfigureAwait(false);
		var contacts = await RenderContactsAsync(communicationContext).ConfigureAwait(false);
		var template = await RenderTemplateAsync(communicationContext).ConfigureAwait(false);

		if (string.IsNullOrWhiteSpace(body) &&
			attachments.Count == 0 &&
			location == null &&
			contacts.Count == 0 &&
			template == null)
		{
			throw new CommunicationsException("WhatsApp communication requires content.");
		}

		return new WhatsAppCommunication(ExtendedProperties)
		{
			From = GetSenderFromAddress(communicationContext),
			Message = body,
			Attachments = attachments,
			Location = location,
			Contacts = contacts,
			Template = template,
			Priority = communicationContext.MessagePriority,
			TransportPriority = communicationContext.TransportPriority,
			To = [.. communicationContext.PlatformIdentities.SelectMany(m => m.Addresses)],
			DeliveryReportCallbackUrlResolver = _configuration.DeliveryReportCallbackUrlResolver
		};
	}

	public bool SupportsIdentityAddress(IPlatformIdentityAddress identityAddress)
	{
		if (identityAddress == null || string.IsNullOrWhiteSpace(identityAddress.Value))
		{
			return false;
		}

		var normalizedValue = NormalizeValue(identityAddress.Value, out var isPrefixed);
		if (normalizedValue == null || !_whatsAppMatchRegex.IsMatch(normalizedValue))
		{
			return false;
		}

		return identityAddress.IsType(PlatformIdentityAddress.Types.WhatsApp()) || isPrefixed;
	}

	async Task<object> IChannel.GenerateCommunicationAsync(IDispatchCommunicationContext communicationContext)
	{
		return await GenerateCommunicationAsync(communicationContext).ConfigureAwait(false);
	}

	private IPlatformIdentityAddress? GetSenderFromAddress(IDispatchCommunicationContext communicationContext)
	{
		return _configuration.FromAddressResolver != null ? _configuration.FromAddressResolver(communicationContext) : null;
	}

	private static ReadOnlyCollection<IWhatsAppAttachment> ConvertAttachments(IDispatchCommunicationContext communicationContext)
	{
		if (communicationContext.ContentModel?.Resources?.Count > 0)
		{
			List<IWhatsAppAttachment> attachments = new(communicationContext.ContentModel.Resources.Count);
			foreach (var resource in communicationContext.ContentModel.Resources)
			{
				attachments.Add(new WhatsAppAttachment(resource));
			}

			return attachments.AsReadOnly();
		}

		return new ReadOnlyCollection<IWhatsAppAttachment>(Array.Empty<IWhatsAppAttachment>());
	}

	private async Task<IWhatsAppLocation?> RenderLocationAsync(IDispatchCommunicationContext communicationContext)
	{
		if (_configuration.Location == null)
		{
			return null;
		}

		var latitude = await _configuration.Location.Latitude.RenderAsync(communicationContext, false).ConfigureAwait(false);
		var longitude = await _configuration.Location.Longitude.RenderAsync(communicationContext, false).ConfigureAwait(false);
		var name = await _configuration.Location.Name.RenderAsync(communicationContext, false).ConfigureAwait(false);
		var address = await _configuration.Location.Address.RenderAsync(communicationContext, false).ConfigureAwait(false);

		if (string.IsNullOrWhiteSpace(latitude) &&
			string.IsNullOrWhiteSpace(longitude) &&
			string.IsNullOrWhiteSpace(name) &&
			string.IsNullOrWhiteSpace(address))
		{
			return null;
		}

		if (string.IsNullOrWhiteSpace(latitude) || string.IsNullOrWhiteSpace(longitude))
		{
			throw new CommunicationsException("WhatsApp location requires both latitude and longitude.");
		}

		if (!double.TryParse(latitude, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsedLatitude))
		{
			throw new CommunicationsException("WhatsApp location latitude is invalid.");
		}

		if (!double.TryParse(longitude, NumberStyles.Float | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var parsedLongitude))
		{
			throw new CommunicationsException("WhatsApp location longitude is invalid.");
		}

		return new WhatsAppLocation
		{
			Latitude = parsedLatitude,
			Longitude = parsedLongitude,
			Name = string.IsNullOrWhiteSpace(name) ? null : name,
			Address = string.IsNullOrWhiteSpace(address) ? null : address
		};
	}

	private async Task<IReadOnlyCollection<IWhatsAppContact>> RenderContactsAsync(IDispatchCommunicationContext communicationContext)
	{
		if (_configuration.Contacts.Count == 0)
		{
			return Array.Empty<IWhatsAppContact>();
		}

		List<IWhatsAppContact> contacts = new(_configuration.Contacts.Count);
		foreach (var contactConfiguration in _configuration.Contacts)
		{
			var contact = await RenderContactAsync(contactConfiguration, communicationContext).ConfigureAwait(false);
			if (contact != null)
			{
				contacts.Add(contact);
			}
		}

		return contacts.Count == 0 ? Array.Empty<IWhatsAppContact>() : contacts.AsReadOnly();
	}

	private static async Task<IWhatsAppContact?> RenderContactAsync(IWhatsAppContactConfiguration contactConfiguration, IDispatchCommunicationContext communicationContext)
	{
		var formattedName = await contactConfiguration.FormattedName.RenderAsync(communicationContext, false).ConfigureAwait(false);
		var firstName = await contactConfiguration.FirstName.RenderAsync(communicationContext, false).ConfigureAwait(false);
		var lastName = await contactConfiguration.LastName.RenderAsync(communicationContext, false).ConfigureAwait(false);
		var organization = await contactConfiguration.Organization.RenderAsync(communicationContext, false).ConfigureAwait(false);

		List<IWhatsAppContactPhone> phones = [];
		foreach (var phoneConfiguration in contactConfiguration.Phones)
		{
			var phone = await RenderPhoneAsync(phoneConfiguration, communicationContext).ConfigureAwait(false);
			if (phone != null)
			{
				phones.Add(phone);
			}
		}

		List<IWhatsAppContactEmail> emails = [];
		foreach (var emailConfiguration in contactConfiguration.Emails)
		{
			var email = await RenderEmailAsync(emailConfiguration, communicationContext).ConfigureAwait(false);
			if (email != null)
			{
				emails.Add(email);
			}
		}

		if (string.IsNullOrWhiteSpace(formattedName))
		{
			var nameParts = new[] { firstName, lastName }
				.Where(value => !string.IsNullOrWhiteSpace(value));
			formattedName = string.Join(" ", nameParts);
		}

		if (string.IsNullOrWhiteSpace(formattedName) &&
			string.IsNullOrWhiteSpace(firstName) &&
			string.IsNullOrWhiteSpace(lastName) &&
			string.IsNullOrWhiteSpace(organization) &&
			phones.Count == 0 &&
			emails.Count == 0)
		{
			return null;
		}

		return new WhatsAppContact
		{
			FormattedName = string.IsNullOrWhiteSpace(formattedName) ? null : formattedName,
			FirstName = string.IsNullOrWhiteSpace(firstName) ? null : firstName,
			LastName = string.IsNullOrWhiteSpace(lastName) ? null : lastName,
			Organization = string.IsNullOrWhiteSpace(organization) ? null : organization,
			Phones = phones.Count == 0 ? Array.Empty<IWhatsAppContactPhone>() : phones.AsReadOnly(),
			Emails = emails.Count == 0 ? Array.Empty<IWhatsAppContactEmail>() : emails.AsReadOnly()
		};
	}

	private static async Task<IWhatsAppContactPhone?> RenderPhoneAsync(IWhatsAppContactPhoneConfiguration phoneConfiguration, IDispatchCommunicationContext communicationContext)
	{
		var phoneNumber = await phoneConfiguration.PhoneNumber.RenderAsync(communicationContext, false).ConfigureAwait(false);
		var type = await phoneConfiguration.Type.RenderAsync(communicationContext, false).ConfigureAwait(false);
		var whatsAppId = await phoneConfiguration.WhatsAppId.RenderAsync(communicationContext, false).ConfigureAwait(false);

		if (string.IsNullOrWhiteSpace(phoneNumber) &&
			string.IsNullOrWhiteSpace(type) &&
			string.IsNullOrWhiteSpace(whatsAppId))
		{
			return null;
		}

		return new WhatsAppContactPhone
		{
			PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
			Type = string.IsNullOrWhiteSpace(type) ? null : type,
			WhatsAppId = string.IsNullOrWhiteSpace(whatsAppId) ? null : whatsAppId
		};
	}

	private static async Task<IWhatsAppContactEmail?> RenderEmailAsync(IWhatsAppContactEmailConfiguration emailConfiguration, IDispatchCommunicationContext communicationContext)
	{
		var emailAddress = await emailConfiguration.EmailAddress.RenderAsync(communicationContext, false).ConfigureAwait(false);
		var type = await emailConfiguration.Type.RenderAsync(communicationContext, false).ConfigureAwait(false);

		if (string.IsNullOrWhiteSpace(emailAddress) &&
			string.IsNullOrWhiteSpace(type))
		{
			return null;
		}

		return new WhatsAppContactEmail
		{
			EmailAddress = string.IsNullOrWhiteSpace(emailAddress) ? null : emailAddress,
			Type = string.IsNullOrWhiteSpace(type) ? null : type
		};
	}

	private async Task<IWhatsAppTemplate?> RenderTemplateAsync(IDispatchCommunicationContext communicationContext)
	{
		if (_configuration.Template == null)
		{
			return null;
		}

		var templateName = Guard.AgainstNullOrWhiteSpace(_configuration.Template.Name);
		var language = Guard.AgainstNullOrWhiteSpace(_configuration.Template.Language);
		var headerParameters = await RenderTemplateParametersAsync(_configuration.Template.HeaderParameters, communicationContext).ConfigureAwait(false);
		var bodyParameters = await RenderTemplateParametersAsync(_configuration.Template.BodyParameters, communicationContext).ConfigureAwait(false);
		var actions = await RenderTemplateActionsAsync(_configuration.Template.Actions, communicationContext).ConfigureAwait(false);

		return new WhatsAppTemplate
		{
			Name = templateName,
			Language = language,
			HeaderParameters = headerParameters,
			BodyParameters = bodyParameters,
			Actions = actions
		};
	}

	private static async Task<IReadOnlyCollection<string>> RenderTemplateParametersAsync(IReadOnlyCollection<IContentTemplateConfiguration> parameters, IDispatchCommunicationContext communicationContext)
	{
		if (parameters.Count == 0)
		{
			return Array.Empty<string>();
		}

		List<string> renderedParameters = new(parameters.Count);
		foreach (var parameter in parameters)
		{
			var renderedValue = await parameter.RenderAsync(communicationContext, false).ConfigureAwait(false);
			renderedParameters.Add(renderedValue ?? string.Empty);
		}

		return renderedParameters.AsReadOnly();
	}

	private static async Task<IReadOnlyCollection<IWhatsAppTemplateAction>> RenderTemplateActionsAsync(IReadOnlyCollection<IWhatsAppTemplateActionConfiguration> actions, IDispatchCommunicationContext communicationContext)
	{
		if (actions.Count == 0)
		{
			return Array.Empty<IWhatsAppTemplateAction>();
		}

		List<IWhatsAppTemplateAction> renderedActions = new(actions.Count);
		foreach (var action in actions.OrderBy(action => action.Index))
		{
			var text = await action.Text.RenderAsync(communicationContext, false).ConfigureAwait(false);
			var value = await action.Value.RenderAsync(communicationContext, false).ConfigureAwait(false);
			renderedActions.Add(new WhatsAppTemplateAction
			{
				ActionType = action.ActionType,
				Index = action.Index,
				Text = string.IsNullOrWhiteSpace(text) ? null : text,
				Value = string.IsNullOrWhiteSpace(value) ? null : value
			});
		}

		return renderedActions.AsReadOnly();
	}

	private static string? NormalizeValue(string? value, out bool isPrefixed)
	{
		isPrefixed = false;
		if (string.IsNullOrWhiteSpace(value))
		{
			return null;
		}

		var trimmed = value!.Trim();
		if (trimmed.StartsWith(WhatsAppPrefix, StringComparison.OrdinalIgnoreCase))
		{
			isPrefixed = true;
			return trimmed[WhatsAppPrefix.Length..];
		}

		return trimmed;
	}

#if FEATURE_SOURCE_GEN
	[GeneratedRegex(Pattern, Options, 2000)]
	private static partial Regex DefaultRegEx();
#endif

	private static Regex CreateRegEx()
	{
#if FEATURE_SOURCE_GEN
		return DefaultRegEx();
#else
		TimeSpan matchTimeout = TimeSpan.FromSeconds(2);

		try
		{
			var domainTimeout = AppDomain.CurrentDomain.GetData("REGEX_DEFAULT_MATCH_TIMEOUT");
			if (domainTimeout is not TimeSpan)
			{
				return new Regex(Pattern, Options, matchTimeout);
			}
		}
		catch
		{
			// Fallback on error
		}

		return new Regex(Pattern, Options);
#endif
	}
}
