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

namespace Transmitly
{
	internal sealed class ContentModel : IContentModel
	{
		public ContentModel(IContentModel? contentModel, IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities)
			: this(contentModel?.Model, platformIdentities, contentModel?.Resources, contentModel?.LinkedResources)
		{

		}

		public ContentModel(ITransactionModel? transactionModel, IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities)
			: this(transactionModel?.Model, platformIdentities, transactionModel?.Resources, transactionModel?.LinkedResources)
		{

		}

		private ContentModel(object? model, IReadOnlyCollection<IPlatformIdentityProfile> platformIdentities, IReadOnlyList<Resource>? resources, IReadOnlyList<LinkedResource>? linkedResources)
		{
			Resources = resources ?? [];
			LinkedResources = linkedResources ?? [];
			Model = new DynamicContentModel(model, platformIdentities, resources, linkedResources);
		}

		public object Model { get; }

		public IReadOnlyList<Resource> Resources { get; }

		public IReadOnlyList<LinkedResource> LinkedResources { get; }
	}
}