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
#if DOTNET40
	using System;
	using System.Collections.Generic;
	using System.ServiceModel;
	using System.ServiceModel.Channels;
	using System.ServiceModel.Discovery;

	public class DiscoveredEndpointModel : WcfEndpointBase<DiscoveredEndpointModel>
	{
		internal DiscoveredEndpointModel(Type contract, Binding binding, Type searchContract)
			: base(contract)
		{
			MaxResults = 1;
			Binding = binding;
			SearchContract = searchContract;
		}

		public Binding Binding { get; private set; }

		public bool DeriveBinding { get; private set; }

		public DiscoveryEndpoint DiscoveryEndpoint { get; private set; }
		public TimeSpan? Duration { get; private set; }

		public Func<IList<EndpointDiscoveryMetadata>, EndpointDiscoveryMetadata> EndpointPreference { get; private set; }
		public EndpointIdentity Identity { get; private set; }
		public int MaxResults { get; private set; }
		public Uri ScopeMatchBy { get; private set; }
		public Type SearchContract { get; private set; }

		public DiscoveredEndpointModel IdentifiedBy(EndpointIdentity identity)
		{
			Identity = identity;
			return this;
		}

		public DiscoveredEndpointModel InferBinding()
		{
			DeriveBinding = true;
			return this;
		}

		public DiscoveredEndpointModel Limit(int maxResults)
		{
			MaxResults = maxResults;
			return this;
		}

		public DiscoveredEndpointModel ManagedBy(BindingAddressEndpointModel endpoint)
		{
			var endpointAddress = endpoint.EndpointAddress ?? new EndpointAddress(endpoint.Address);
			DiscoveryEndpoint = new DiscoveryEndpoint(endpoint.Binding, endpointAddress);
			return this;
		}

		public DiscoveredEndpointModel ManagedBy(DiscoveryEndpoint endpoint)
		{
			DiscoveryEndpoint = endpoint;
			return this;
		}

		public DiscoveredEndpointModel MatchScopeBy(Uri match)
		{
			ScopeMatchBy = match;
			return this;
		}

		public DiscoveredEndpointModel MatchScopeByLdap()
		{
			return MatchScopeBy(FindCriteria.ScopeMatchByLdap);
		}

		public DiscoveredEndpointModel MatchScopeByPrefix()
		{
			return MatchScopeBy(FindCriteria.ScopeMatchByPrefix);
		}

		public DiscoveredEndpointModel MatchScopeByUuid()
		{
			return MatchScopeBy(FindCriteria.ScopeMatchByUuid);
		}

		public DiscoveredEndpointModel MatchScopeExactly()
		{
			return MatchScopeBy(FindCriteria.ScopeMatchByExact);
		}

		public DiscoveredEndpointModel PreferEndpoint(Func<IList<EndpointDiscoveryMetadata>, EndpointDiscoveryMetadata> selector)
		{
			EndpointPreference = selector;
			return this;
		}

		public DiscoveredEndpointModel Span(TimeSpan duration)
		{
			Duration = duration;
			return this;
		}

		protected override void Accept(IWcfEndpointVisitor visitor)
		{
			visitor.VisitBindingDiscoveredEndpoint(this);
		}
	}
#endif
}