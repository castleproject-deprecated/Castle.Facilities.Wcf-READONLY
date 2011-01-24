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
	using System.Collections;
	using System.Linq;
	using System.Reflection;

	using Castle.Facilities.TypedFactory;
	using Castle.Facilities.WcfIntegration.Model;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	public class WcfClientFactorySelector : ITypedFactoryComponentSelector
	{
		public TypedFactoryComponent SelectComponent(MethodInfo method, Type type, object[] arguments)
		{
			return new ClientComponent(method.ReturnType, arguments);
		}

		private class ClientComponent : TypedFactoryComponent
		{
			private readonly object[] arguments;

			public ClientComponent(Type componentType, object[] arguments)
				: base(null, componentType, null)
			{
				this.arguments = arguments;
			}

			public override object Resolve(IKernel kernel)
			{
				var key = GetKey();
				var args = GetArguments();
				if (key == null)
				{
					return kernel.Resolve(ComponentType, args);
				}

				return kernel.Resolve(key, ComponentType, args);
			}

			private IDictionary GetArguments()
			{
				if (arguments.Length == 1 && arguments.Single() is string)
				{
					// in other words "if we're dealing with this method: T GetClient<T>(string name) where T : class;"
					return null;
				}
				var argument = arguments.Last();
				if (argument == null)
				{
					return null;
				}
				var args = new Arguments();
				if (argument is IWcfClientModel)
				{
					args.Insert((IWcfClientModel)argument);
				}
				else if (argument is Uri)
				{
					args.Insert<IWcfEndpoint>(WcfEndpoint.At((Uri)argument));
				}
				else if (argument is IWcfEndpoint)
				{
					args.Insert((IWcfEndpoint)argument);
				}
				return args;
			}

			private string GetKey()
			{
				string key = null;
				var argument = arguments[0];
				if (arguments.Length == 2)
				{
					key = (string)argument;
				}
				else if (argument is string)
				{
					key = (string)argument;
				}
				return key;
			}
		}
	}
}