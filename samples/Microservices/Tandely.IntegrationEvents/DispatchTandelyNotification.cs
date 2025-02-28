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

using System.Text.Json;
using Transmitly;

namespace Tandely.IntegrationEvents
{
    public sealed class DispatchTandelyNotification
    {
        public IReadOnlyCollection<string> AllowedChannels { get; set; } = [];
        public string? CommunicationId { get; set; }
        public IEnumerable<string> PersonFilters { get; set; } = [];
        public string? ExternalInstanceReferenceId { get; set; }
        public TandelyTransactionalModel? TransactionalModel { get; set; }
        public List<TandelyPlatformIdentity> PlatformIdentities { get; set; } = [];
    }
    public sealed class TandelyTransactionalModel : ITransactionModel
    {
        public TandelyTransactionalModel()
        {
            
        }
        public TandelyTransactionalModel(ITransactionModel transactionModel)
        {
            if (transactionModel == null)
                return;
            Model = transactionModel.Model;
            Resources = transactionModel.Resources;
            LinkedResources = transactionModel.LinkedResources;
        }
        private object _model = new { };

        public object Model
        {
            get => _model; 
            set
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


    public sealed class TandelyPlatformIdentity : IPlatformIdentity, IIdentityReference
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string[] Personas { get; set; } = [];
        public List<TandelyIdentityAddress> Addresses { get; set; } = [];
        IReadOnlyCollection<IIdentityAddress> IPlatformIdentity.Addresses { get => Addresses; set => throw new NotImplementedException(); }
    }
    public class TandelyIdentityAddress : IIdentityAddress
    {
        public string? Type { get; set; }
        public string Value { get; set; }
        public IDictionary<string, string?> AddressParts { get; set; } = new Dictionary<string, string?>();
        public IDictionary<string, string?> Attributes { get; set; } = new Dictionary<string, string?>();
        public string? Display { get; set; }
    }
}
