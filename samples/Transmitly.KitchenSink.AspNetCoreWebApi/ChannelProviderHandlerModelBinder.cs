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

using EllipticCurve;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using Transmitly.ChannelProvider;
using Transmitly.ChannelProvider.Configuration;
using Transmitly.Delivery;

namespace Transmitly.KitchenSink.AspNetCoreWebApi
{
	//Todo: move to new AspNetCore.Mvc package

	//Source=https://github.com/aspnet/AspNetIdentity/blob/main/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs
	internal static class AsyncHelper
	{
		private static readonly TaskFactory _myTaskFactory = new TaskFactory(CancellationToken.None,
			TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

		public static TResult RunSync<TResult>(Func<Task<TResult>> func)
		{
			var cultureUi = CultureInfo.CurrentUICulture;
			var culture = CultureInfo.CurrentCulture;
			return _myTaskFactory.StartNew(() =>
			{
				Thread.CurrentThread.CurrentCulture = culture;
				Thread.CurrentThread.CurrentUICulture = cultureUi;
				return func();
			}).Unwrap().GetAwaiter().GetResult();
		}

		public static void RunSync(Func<Task> func)
		{
			var cultureUi = CultureInfo.CurrentUICulture;
			var culture = CultureInfo.CurrentCulture;
			_myTaskFactory.StartNew(() =>
			{
				Thread.CurrentThread.CurrentCulture = culture;
				Thread.CurrentThread.CurrentUICulture = cultureUi;
				return func();
			}).Unwrap().GetAwaiter().GetResult();
		}
	}
	class DefaultRequestAdaptorContext(IValueProvider valueProvider, string requestBody) : IRequestAdaptorContext
	{
		private readonly IValueProvider _valueProvider = valueProvider;

		public string? GetValue(string key)
		{
			return _valueProvider.GetValue(key).FirstValue;
		}

		public string? Content { get; } = requestBody;

		public string? PipelineName => GetValue("pipelineName");

		public string? ResourceId => GetValue("resourceId");
	}

	class ChannelProviderDeliveryReportRequestModelBinder : IModelBinder
	{
		private readonly List<Lazy<IChannelProviderDeliveryReportRequestAdaptor>> _adaptorInstances;

		public ChannelProviderDeliveryReportRequestModelBinder(IChannelProviderFactory adaptor)
		{
			var adaptors = AsyncHelper.RunSync(adaptor.GetAllDeliveryReportRequestAdaptorsAsync);
			_adaptorInstances = adaptors.Select(s => new Lazy<IChannelProviderDeliveryReportRequestAdaptor>(AsyncHelper.RunSync(() => adaptor.ResolveDeliveryReportRequestAdaptorAsync(s)))).ToList();
		}

		public async Task BindModelAsync(ModelBindingContext bindingContext)
		{

			var str = await new StreamReader(bindingContext.ActionContext.HttpContext.Request.Body).ReadToEndAsync();
			foreach (var adaptor in _adaptorInstances)
			{
				try
				{
					var handled = await adaptor.Value.AdaptAsync(new DefaultRequestAdaptorContext(bindingContext.ValueProvider, str));
					if (handled != null)
					{
						bindingContext.Result = ModelBindingResult.Success(new ChannelProviderDeliveryReportRequest(handled));
						return;
					}
				}
				catch
				{
					//Eat any unexpected adaptor exceptions
				}
			}
			bindingContext.Result = ModelBindingResult.Failed();
		}
	}

	sealed class ChannelProviderAdaptorModelBinder(IChannelProviderFactory channelProviderFactory) : IConfigureOptions<MvcOptions>
	{
		private readonly IChannelProviderFactory _channelProviderFactory = Guard.AgainstNull(channelProviderFactory);

		public void Configure(MvcOptions options)
		{
			options.ModelBinderProviders.Insert(0, new ChannelProviderModelBinderProvider(_channelProviderFactory));
		}
	}

	sealed class ChannelProviderModelBinderProvider(IChannelProviderFactory channelProviderFactory) : IModelBinderProvider
	{
		private readonly IChannelProviderFactory _channelProviderFactory = Guard.AgainstNull(channelProviderFactory);

		public IModelBinder? GetBinder(ModelBinderProviderContext context)
		{
			Guard.AgainstNull(context);
			if (context.Metadata.ModelType == typeof(ChannelProviderDeliveryReportRequest))
				return new ChannelProviderDeliveryReportRequestModelBinder(_channelProviderFactory);
			return null;
		}
	}
}