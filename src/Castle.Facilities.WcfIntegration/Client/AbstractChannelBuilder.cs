﻿// Copyright 2004-2009 Castle Project - http://www.castleproject.org/
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
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Description;
	using System.ServiceModel.Discovery;
	using Castle.MicroKernel;

	public abstract class AbstractChannelBuilder : IWcfEndpointVisitor
	{
		private Type contract;
		private ChannelCreator channelCreator;

		protected AbstractChannelBuilder(IKernel kernel)
		{
			Kernel = kernel;
		}

		protected IKernel Kernel { get; private set; }

		public WcfClientExtension Clients { get; set; }
	
		protected void ConfigureChannelFactory(ChannelFactory channelFactory, IWcfClientModel clientModel, IWcfBurden burden)
		{
			var extensions = new ChannelFactoryExtensions(channelFactory, Kernel)
				.Install(burden, new WcfChannelExtensions());

			var endpointExtensions = new ServiceEndpointExtensions(channelFactory.Endpoint, true, Kernel)
				.Install(burden, new WcfEndpointExtensions(WcfExtensionScope.Clients));

			if (clientModel != null)
			{
				extensions.Install(clientModel.Extensions, burden);
				endpointExtensions.Install(clientModel.Extensions, burden);
				endpointExtensions.Install(clientModel.Endpoint.Extensions, burden);
			}

			burden.Add(new ChannelFactoryHolder(channelFactory));
		}

		protected ChannelCreator GetEndpointChannelCreator(IWcfEndpoint endpoint)
		{
			return GetEndpointChannelCreator(endpoint, null);
		}

		protected ChannelCreator GetEndpointChannelCreator(IWcfEndpoint endpoint, Type contract)
		{
			this.contract = contract ?? endpoint.Contract;
			endpoint.Accept(this);
			return channelCreator;
		}

		protected abstract ChannelCreator GetChannel(Type contract);
		protected abstract ChannelCreator GetChannel(Type contract, ServiceEndpoint endpoint);
		protected abstract ChannelCreator GetChannel(Type contract, string configurationName);
		protected abstract ChannelCreator GetChannel(Type contract, Binding binding, string address);
		protected abstract ChannelCreator GetChannel(Type contract, Binding binding, EndpointAddress address);

		#region IWcfEndpointVisitor Members

		void IWcfEndpointVisitor.VisitContractEndpoint(ContractEndpointModel model)
		{
			channelCreator = GetChannel(contract);
		}
        
		void IWcfEndpointVisitor.VisitServiceEndpoint(ServiceEndpointModel model)
		{
			channelCreator = GetChannel(contract, model.ServiceEndpoint);
		}

		void IWcfEndpointVisitor.VisitConfigurationEndpoint(ConfigurationEndpointModel model)
		{
			channelCreator = GetChannel(contract, model.EndpointName);
		}

		void IWcfEndpointVisitor.VisitBindingEndpoint(BindingEndpointModel model)
		{
			channelCreator = GetChannel(contract, GetEffectiveBinding(model.Binding, null), string.Empty);
		}

		void IWcfEndpointVisitor.VisitBindingAddressEndpoint(BindingAddressEndpointModel model)
		{
			if (model.HasViaAddress)
			{
				var address = model.EndpointAddress ?? new EndpointAddress(model.Address);
				var description = ContractDescription.GetContract(contract);
				var binding = GetEffectiveBinding(model.Binding, address.Uri);
				var endpoint = new ServiceEndpoint(description, binding, address);
				endpoint.Behaviors.Add(new ClientViaBehavior(model.ViaAddress));
				channelCreator = GetChannel(contract, endpoint);
			}
			else
			{
				if (model.EndpointAddress != null)
				{
					var binding = GetEffectiveBinding(model.Binding, model.EndpointAddress.Uri);
					channelCreator = GetChannel(contract, binding, model.EndpointAddress);
				}
				else
				{
					var binding = GetEffectiveBinding(model.Binding, new Uri(model.Address));
					channelCreator = GetChannel(contract, binding, model.Address);
				}
			}
		}

		void IWcfEndpointVisitor.VisitBindingDiscoveredEndpoint(DiscoveredEndpointModel model)
		{
			var discoveryEndpoint = model.DiscoveryEndpoint ?? new UdpDiscoveryEndpoint();

			if (model.UseMetadata)
			{
				DiscoverEndpointFromMetadata(discoveryEndpoint, model);
			}
			else
			{
				DiscoverEndpointAddress(discoveryEndpoint, model);
			}
		}

		private void DiscoverEndpointAddress(DiscoveryEndpoint discoveryEndpoint, DiscoveredEndpointModel model)
		{
			using (var discover = new DiscoveryClient(discoveryEndpoint))
			{
				var criteria = CreateSearchCriteria(model);

				var discovered = discover.Find(criteria);
				if (discovered.Endpoints.Count > 0)
				{
					var address = discovered.Endpoints[0].Address;
					var binding = GetEffectiveBinding(model.Binding, address.Uri);
					channelCreator = GetChannel(contract, binding, address);
				}
				else
				{
					throw new EndpointNotFoundException(string.Format(
						"Unable to discover the endpoint address for contract {0}.  " + 
						"Either no service exists or it does not support discovery.",
						contract.FullName));
				}
			}
		}

		private void DiscoverEndpointFromMetadata(DiscoveryEndpoint discoveryEndpoint, DiscoveredEndpointModel model)
		{
			using (var discover = new DiscoveryClient(discoveryEndpoint))
			{
				var criteria = FindCriteria.CreateMetadataExchangeEndpointCriteria(contract);
				criteria.MaxResults = 1;

				var discovered = discover.Find(criteria);
				if (discovered.Endpoints.Count > 0)
				{
					var mexAddress = discovered.Endpoints[0].Address;
					var endpoints = MetadataResolver.Resolve(contract, mexAddress);
					if (endpoints.Count > 0)
					{
						channelCreator = GetChannel(contract, endpoints[0].Binding, endpoints[0].Address);
					}
				}
			}
		}

		#endregion

		protected virtual Binding InferBinding(Uri address)
		{
			if (Clients != null)
			{
				return Clients.InferBinding(address) ?? Clients.DefaultBinding;
			}
			return null;
		}

		private FindCriteria CreateSearchCriteria(DiscoveredEndpointModel model)
		{
			var criteria = new FindCriteria(contract) { MaxResults = 1 };

			if (model.Duration.HasValue)
			{
				criteria.Duration = model.Duration.Value;
			}

			foreach (var scope in model.Scopes)
			{
				criteria.Scopes.Add(scope);
			}

			if (model.ScopeMatchBy != null)
			{
				criteria.ScopeMatchBy = model.ScopeMatchBy;
			}

			foreach (var filter in model.Filters)
			{
				criteria.Extensions.Add(filter);
			}

			return criteria;
		}

		private Binding GetEffectiveBinding(Binding binding, Uri address)
		{
			return binding ?? InferBinding(address);
		}
	}
}