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

namespace Castle.Facilities.WcfIntegration.Model.Lifestyles
{
	using System;
	using System.ServiceModel;

	/// <summary>
	///   Manages object instances in the context of WCF session. This means that when a component 
	///   with this lifestyle is requested multiple times during WCF session, the same instance will be provided.
	///   If no WCF session is available falls back to the default behavior of transient.
	/// </summary>
	public class PerWcfSessionLifestyle : AbstractWcfLifestyleManager<IContextChannel, PerChannelCache>
	{
		private readonly IOperationContextProvider operationContextProvider;

		public PerWcfSessionLifestyle()
			: this(new OperationContextProvider())
		{
		}

		public PerWcfSessionLifestyle(IOperationContextProvider operationContextProvider)
		{
			if (operationContextProvider == null)
			{
				throw new ArgumentNullException("operationContextProvider");
			}

			this.operationContextProvider = operationContextProvider;
		}

		protected override IContextChannel GetCacheHolder()
		{
			var operation = operationContextProvider.Current;
			if (operation == null)
			{
				return null;
			}

			if (string.IsNullOrEmpty(operation.SessionId))
			{
				return null;
			}

			return operation.Channel;
		}
	}
}