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

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text;
using Transmitly.ChannelProvider.ProviderResponse;

namespace Transmitly.KitchenSink.AspNetCoreWebApi
{
	sealed class ChannelProviderHandlerModelBinder : IModelBinder
	{
		public async Task BindModelAsync(ModelBindingContext bindingContext)
		{
			using var reader = new StreamReader(bindingContext.ActionContext.HttpContext.Request.Body, Encoding.UTF8);
			string? str = await reader.ReadToEndAsync();

			var handlerFactory = DefaultChannelProviderResponseHandlerFactory.Instance;
			var supportedHandlers = handlerFactory.Handlers.Where(h => h.Handles(str));
			foreach (var handler in supportedHandlers)
			{
				var reports = handler.Handle(str);
				if (reports != null)
				{
					bindingContext.Result = ModelBindingResult.Success(reports);
					return;
				}
			}
			bindingContext.Result = ModelBindingResult.Failed();
		}
	}
}