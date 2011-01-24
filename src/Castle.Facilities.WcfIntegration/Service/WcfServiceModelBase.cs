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

namespace Castle.Facilities.WcfIntegration.Service
{
	using System;
	using System.Collections.Generic;

	using Castle.Facilities.WcfIntegration.Behaviors;
	using Castle.Facilities.WcfIntegration.Model;

	public abstract class WcfServiceModelBase : IWcfServiceModel
	{
		private ICollection<Uri> baseAddresses;
		private ICollection<IWcfEndpoint> endpoints;
		private ICollection<IWcfExtension> extensions;

		public ICollection<Uri> BaseAddresses
		{
			get
			{
				if (baseAddresses == null)
				{
					baseAddresses = new List<Uri>();
				}
				return baseAddresses;
			}
			set { baseAddresses = value; }
		}

		public ICollection<IWcfEndpoint> Endpoints
		{
			get
			{
				if (endpoints == null)
				{
					endpoints = new List<IWcfEndpoint>();
				}
				return endpoints;
			}
			set { endpoints = value; }
		}

		public ICollection<IWcfExtension> Extensions
		{
			get
			{
				if (extensions == null)
				{
					extensions = new List<IWcfExtension>();
				}
				return extensions;
			}
		}

		public bool IsHosted { get; protected set; }

		public bool? ShouldOpenEagerly { get; protected set; }
	}
}