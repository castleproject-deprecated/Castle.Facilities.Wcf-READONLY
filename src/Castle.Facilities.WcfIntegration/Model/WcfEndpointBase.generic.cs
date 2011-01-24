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
	using System.Linq;
	using System.ServiceModel.Discovery;
	using System.Xml.Linq;

	using Castle.Facilities.WcfIntegration.Behaviors;
	using Castle.Facilities.WcfIntegration.Behaviors.Logging;
	using Castle.Facilities.WcfIntegration.Internal;

	public abstract class WcfEndpointBase<T> : WcfEndpointBase
		where T : WcfEndpointBase<T>
	{
		protected WcfEndpointBase(Type contract)
			: base(contract)
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

		public T PreserveObjectReferences()
		{
			return AddExtensions(typeof(PreserveObjectReferenceBehavior));
		}

#if DOTNET40

		#region Discovery and Metadata

		public T InScope(params Uri[] scopes)
		{
			var discovery = GetDiscoveryInstance();
			discovery.Scopes.AddAll(scopes);
			return (T)this;
		}

		public T InScope(params string[] scopes)
		{
			return InScope(scopes.Select(scope => new Uri(scope)).ToArray());
		}

		public T WithMetadata(params XElement[] metadata)
		{
			var discovery = GetDiscoveryInstance();
			discovery.Extensions.AddAll(metadata);
			return (T)this;
		}

		private EndpointDiscoveryBehavior GetDiscoveryInstance()
		{
			var discovery = Extensions.OfType<WcfInstanceExtension>()
				.Select(extension => extension.Instance)
				.OfType<EndpointDiscoveryBehavior>()
				.FirstOrDefault();

			if (discovery == null)
			{
				discovery = new EndpointDiscoveryBehavior();
				AddExtensions(WcfExplicitExtension.CreateFrom(discovery));
			}

			return discovery;
		}

		#endregion

#endif

		#region Logging

		public T LogMessages()
		{
			return AddExtensions(typeof(LogMessageEndpointBehavior));
		}

		public T LogMessages<TFormatProfider>()
			where TFormatProfider : IFormatProvider, new()
		{
			return LogMessages<TFormatProfider>(null);
		}

		public T LogMessages<TFormatProfider>(string format)
			where TFormatProfider : IFormatProvider, new()
		{
			return LogMessages(new TFormatProfider(), format);
		}

		public T LogMessages(IFormatProvider formatter)
		{
			return LogMessages(formatter, null);
		}

		public T LogMessages(IFormatProvider formatter, string format)
		{
			return LogMessages().AddExtensions(new LogMessageFormat(formatter, format));
		}

		public T LogMessages(string format)
		{
			return LogMessages().AddExtensions(new LogMessageFormat(format));
		}

		#endregion
	}
}