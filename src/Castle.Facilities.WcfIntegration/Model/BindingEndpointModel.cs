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
	using System.ServiceModel;
	using System.ServiceModel.Channels;

	public class BindingEndpointModel : WcfEndpointBase<BindingEndpointModel>
	{
		internal BindingEndpointModel(Type contract, Binding binding)
			: base(contract)
		{
			Binding = binding;
		}

		public Binding Binding { get; private set; }

		public BindingAddressEndpointModel At(string address)
		{
			if (string.IsNullOrEmpty(address))
			{
				throw new ArgumentException("address cannot be null or empty");
			}
			return new BindingAddressEndpointModel(Contract, Binding, address);
		}

		public BindingAddressEndpointModel At(Uri address)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return new BindingAddressEndpointModel(Contract, Binding, address.AbsoluteUri);
		}

		public BindingAddressEndpointModel At(EndpointAddress address)
		{
			if (address == null)
			{
				throw new ArgumentNullException("address");
			}
			return new BindingAddressEndpointModel(Contract, Binding, address);
		}

#if DOTNET40
		public DiscoveredEndpointModel Discover()
		{
			return new DiscoveredEndpointModel(Contract, Binding, null);
		}

		public DiscoveredEndpointModel Discover(Type searchContract)
		{
			return new DiscoveredEndpointModel(Contract, Binding, searchContract);
		}
#endif

		protected override void Accept(IWcfEndpointVisitor visitor)
		{
			visitor.VisitBindingEndpoint(this);
		}
	}
}