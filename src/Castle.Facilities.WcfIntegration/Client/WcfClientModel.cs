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

namespace Castle.Facilities.WcfIntegration.Client
{
	using System;
	using System.Collections.Generic;

	using Castle.Facilities.WcfIntegration.Behaviors;
	using Castle.Facilities.WcfIntegration.Behaviors.Security;

	public abstract class WcfClientModelBase : IWcfClientModel
	{
		protected bool wantsAsync;
		private IWcfEndpoint endpoint;
		private List<IWcfExtension> extensions;

		protected WcfClientModelBase()
		{
			wantsAsync = true;
		}

		protected WcfClientModelBase(IWcfEndpoint endpoint) : this()
		{
			Endpoint = endpoint;
		}

		public Type Contract
		{
			get { return endpoint.Contract; }
		}

		public IWcfEndpoint Endpoint
		{
			get { return endpoint; }
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("value");
				}
				endpoint = value;
			}
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

		public bool WantsAsyncCapability
		{
			get { return wantsAsync; }
		}

		public virtual IWcfClientModel ForEndpoint(IWcfEndpoint endpoint)
		{
			var copy = (WcfClientModelBase)MemberwiseClone();
			copy.endpoint = endpoint;
			if (extensions != null)
			{
				copy.extensions = new List<IWcfExtension>(extensions);
			}
			return copy;
		}
	}

	public abstract class WcfClientModel<T> : WcfClientModelBase
		where T : WcfClientModel<T>
	{
		protected WcfClientModel()
		{
		}

		protected WcfClientModel(IWcfEndpoint endpoint)
			: base(endpoint)
		{
		}

		public T AddExtensions(params object[] extensions)
		{
			foreach (var extension in extensions)
			{
				Extensions.Add(WcfExplicitExtension.CreateFrom(extension));
			}
			return (T)this;
		}

		public T Credentials(AbstractCredentials credentials)
		{
			return AddExtensions(credentials);
		}

		public T WithoutAsyncCapability()
		{
			wantsAsync = false;
			return (T)this;
		}
	}
}