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

using System.Diagnostics;

namespace Transmitly
{
	/// <summary>
	/// Content Model for Dispatching data related to dispatching communications using a <see cref="ICommunicationsClient"/>
	/// </summary>
	[DebuggerStepThrough]
	public class ContentModel : IContentModel
	{
		private readonly object _model;
		private readonly Resource[]? _attachments;
		private readonly LinkedResource[]? _linkedResource;

		private ContentModel(object model, Resource[]? attachments, LinkedResource[]? linkedResources)
		{
			_model = model;
			_attachments = attachments;
			_linkedResource = linkedResources;
		}

		public object Model => _model;

		public IReadOnlyList<Resource> Resources => _attachments ?? [];

		public IReadOnlyList<LinkedResource> LinkedResources => _linkedResource ?? [];

		public static IContentModel Create(object model)
		{
			return new ContentModel(model, null, null);
		}

		public static IContentModel Create(object model, params Resource[] resources)
		{
			var linkedResources = resources.OfType<LinkedResource>().ToArray();
			var attachments = resources.Where(x => x is not LinkedResource).ToArray();
			return new ContentModel(model, attachments, linkedResources);
		}
	}
}