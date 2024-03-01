﻿// ﻿﻿Copyright (c) Code Impressions, LLC. All Rights Reserved.
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

using System.Buffers;
using System.Text.Json;

namespace Transmitly.KitchenSink.AspNetCoreWebApi.Controllers
{
	public partial class CommunicationsController
	{
		public class DispatchContentModel : IContentModel
		{
			private object _model;

			public object Model
			{
				get => _model; set
				{
					if (value is JsonElement)
						_model = JsonSerializer.Deserialize<System.Dynamic.ExpandoObject>(JsonSerializer.Serialize(value)) ?? new System.Dynamic.ExpandoObject();
					else
						_model = value;

				}
			}
			public IReadOnlyList<Resource>? Resources { get; set; } = null;

			public IReadOnlyList<LinkedResource>? LinkedResources { get; set; } = null;
		}
	}
}