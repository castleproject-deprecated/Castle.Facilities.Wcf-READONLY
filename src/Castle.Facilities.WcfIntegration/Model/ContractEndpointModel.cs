// Copyright 2004-2011 Castle Project - http://www.castleproject.org/
// 
// Licensed under the Apache License, Version 2.0 (the "License");
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

namespace Castle.Facilities.WcfIntegration.Model
{
	using System;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;

	public class ContractEndpointModel : WcfEndpointBase<ContractEndpointModel>
	{
		internal ContractEndpointModel()
			: this(null)
		{
		}

		internal ContractEndpointModel(Type contract)
			: base(contract)
		{
		}

		public ServiceEndpointModel FromEndpoint(ServiceEndpoint endpoint)
		{
			if (endpoint == null)
			{
				throw new ArgumentNullException("endpoint");
			}
			return new ServiceEndpointModel(Contract, endpoint);
		}

		public ConfigurationEndpointModel FromConfiguration(string endpointName)
		{
			if (string.IsNullOrEmpty(endpointName))
			{
				throw new ArgumentException("endpointName cannot be nul or empty");
			}
			return new ConfigurationEndpointModel(Contract, endpointName);
		}

		public BindingEndpointModel BoundTo(Binding binding)
		{
			if (binding == null)
			{
				throw new ArgumentNullException("binding");
			}
			return new BindingEndpointModel(Contract, binding);
		}

		public BindingAddressEndpointModel At(string address)
		{
			return new BindingEndpointModel(Contract, null).At(address);
		}

		public BindingAddressEndpointModel At(Uri address)
		{
			return new BindingEndpointModel(Contract, null).At(address);
		}

#if DOTNET40
		public DiscoveredEndpointModel Discover()
		{
			return new BindingEndpointModel(Contract, null).Discover();
		}

		public DiscoveredEndpointModel Discover(Type searchContract)
		{
			return new BindingEndpointModel(Contract, null).Discover(searchContract);
		}
#endif

		protected override void Accept(IWcfEndpointVisitor visitor)
		{
			visitor.VisitContractEndpoint(this);
		}
	}
}