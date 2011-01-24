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

	using Castle.Facilities.TypedFactory;
	using Castle.Facilities.WcfIntegration.Client;
	using Castle.Facilities.WcfIntegration.Model;

	/// <summary>
	///   Factory interface for pulling wcf client-side proxies on the fly from the client side.
	/// </summary>
	/// <remarks>
	///   Most of the time you will take dependency in your code on the client-side proxy interface and Windsor with the facility will construct and provide a running proxy to you as your objects get constructed.
	///   In cases when you need to obtain the proxy at some later point, and some additional input is required (like the address to connect to) not known outright, you can use this interface to pull the proxies.
	///   The interface is automatically registered by the <see cref = "WcfFacility" /> if upon initialization it detects that  <see
	///     cref = "TypedFactoryFacility" /> had been added to the container prior. Notice that means that the <see
	///    cref = "TypedFactoryFacility" /> has to be added to the container first, <see cref = "WcfFacility" /> second.
	/// </remarks>
	public interface IWcfClientFactory
	{
		T GetClient<T>(string name) where T : class;

		T GetClient<T>(IWcfClientModel model) where T : class;

		T GetClient<T>(string name, IWcfClientModel model) where T : class;

		T GetClient<T>(IWcfEndpoint endpoint) where T : class;

		T GetClient<T>(string name, IWcfEndpoint endpoint) where T : class;

		T GetClient<T>(Uri address) where T : class;

		T GetClient<T>(string name, Uri address) where T : class;

		void Release(object client);
	}
}