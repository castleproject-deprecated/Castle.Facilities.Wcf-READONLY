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

namespace Castle.Facilities.WcfIntegration
{
	using System;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using Castle.Facilities.WcfIntegration.Model;

	public static class WcfEndpoint
	{
		public static ServiceEndpointModel FromEndpoint(ServiceEndpoint endpoint)
		{
			return new ContractEndpointModel().FromEndpoint(endpoint);
		}

		public static ConfigurationEndpointModel FromConfiguration(string endpointName)
		{
			return new ContractEndpointModel().FromConfiguration(endpointName);
		}

		public static BindingEndpointModel BoundTo(Binding binding)
		{
			return new ContractEndpointModel().BoundTo(binding);
		}

		public static BindingAddressEndpointModel At(string address)
		{
			return new ContractEndpointModel().At(address);
		}

		public static BindingAddressEndpointModel At(Uri address)
		{
			return new ContractEndpointModel().At(address);
		}

		public static ContractEndpointModel ForContract(Type contract)
		{
			return new ContractEndpointModel(contract);
		}

		public static ContractEndpointModel ForContract<TContract>()
			where TContract : class
		{
			return ForContract(typeof(TContract));
		}

#if DOTNET40
		public static DiscoveredEndpointModel Discover()
		{
			return new ContractEndpointModel().Discover();
		}

		public static DiscoveredEndpointModel Discover(Type searchContract)
		{
			return new ContractEndpointModel().Discover(searchContract);
		}
#endif
	}
}