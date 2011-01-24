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

	public class BindingAddressEndpointModel : WcfEndpointBase<BindingAddressEndpointModel>
	{
		private readonly string address;
		private readonly EndpointAddress endpointAddress;
		private string via;

		internal BindingAddressEndpointModel(Type contract, Binding binding, string address)
			: base(contract)
		{
			Binding = binding;
			this.address = address;
		}

		internal BindingAddressEndpointModel(Type contract, Binding binding, EndpointAddress address)
			: base(contract)
		{
			Binding = binding;
			endpointAddress = address;
		}

		public string Address
		{
			get { return address ?? endpointAddress.Uri.AbsoluteUri; }
		}

		public Binding Binding { get; private set; }

		public EndpointAddress EndpointAddress
		{
			get { return endpointAddress; }
		}

		public bool HasViaAddress
		{
			get { return !string.IsNullOrEmpty(via); }
		}

		public Uri ViaAddress
		{
			get { return new Uri(via, UriKind.Absolute); }
		}

		public BindingAddressEndpointModel Via(string physicalAddress)
		{
			via = physicalAddress;
			return this;
		}

		protected override void Accept(IWcfEndpointVisitor visitor)
		{
			visitor.VisitBindingAddressEndpoint(this);
		}
	}
}