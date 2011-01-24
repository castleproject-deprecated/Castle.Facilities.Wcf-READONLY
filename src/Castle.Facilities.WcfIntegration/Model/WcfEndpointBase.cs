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
	using System.Collections.Generic;

	using Castle.Facilities.WcfIntegration.Behaviors;

	public abstract class WcfEndpointBase : IWcfEndpoint
	{
		private List<IWcfExtension> extensions;

		protected WcfEndpointBase(Type contract)
		{
			Contract = contract;
		}

		public Type Contract { get; set; }

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

		protected abstract void Accept(IWcfEndpointVisitor visitor);

		void IWcfEndpoint.Accept(IWcfEndpointVisitor visitor)
		{
			Accept(visitor);
		}
	}
}