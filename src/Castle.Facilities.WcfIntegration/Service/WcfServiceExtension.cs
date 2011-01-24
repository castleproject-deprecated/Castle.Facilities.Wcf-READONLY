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
#if DOTNET40
	using System.Collections.Concurrent;
#endif
	using System.Collections.Generic;
	using System.Reflection;
	using System.ServiceModel;
	using System.ServiceModel.Activation;
	using System.ServiceModel.Channels;

	using Castle.Core;
	using Castle.Facilities.WcfIntegration.Behaviors;
	using Castle.Facilities.WcfIntegration.Internal;
	using Castle.Facilities.WcfIntegration.Model;
	using Castle.Facilities.WcfIntegration.Service.Rest;
	using Castle.MicroKernel;
	using Castle.MicroKernel.Registration;

	public class WcfServiceExtension : IDisposable
	{
		internal static IKernel GlobalKernel;

		private static readonly ConcurrentDictionary<Type, CreateServiceHostDelegate>
			createServiceHostCache = new ConcurrentDictionary<Type, CreateServiceHostDelegate>();

		private static readonly MethodInfo createServiceHostMethod =
			typeof(WcfServiceExtension).GetMethod("CreateServiceHostInternal",
			                                      BindingFlags.NonPublic | BindingFlags.Static, null,
			                                      new[]
			                                      {
			                                      	typeof(IKernel), typeof(IWcfServiceModel),
			                                      	typeof(ComponentModel), typeof(Uri[])
			                                      }, null
				);

		private Action _afterInit;
		private AspNetCompatibilityRequirementsMode? aspNetCompat;
		private TimeSpan? closeTimeout;
		private Binding defaultBinding;
		private WcfFacility facility;
		private IKernel kernel;

		public AspNetCompatibilityRequirementsMode? AspNetCompatibility
		{
			get { return aspNetCompat; }
			set { aspNetCompat = value; }
		}

		public TimeSpan? CloseTimeout
		{
			get { return closeTimeout ?? facility.CloseTimeout; }
			set { closeTimeout = value; }
		}

		public Binding DefaultBinding
		{
			get { return defaultBinding ?? facility.DefaultBinding; }
			set { defaultBinding = value; }
		}

		public bool OpenServiceHostsEagerly { get; set; }

		public WcfServiceExtension AddServiceHostBuilder<TBuilder>()
			where TBuilder : IServiceHostBuilder
		{
			return AddServiceHostBuilder(typeof(TBuilder));
		}

		public WcfServiceExtension AddServiceHostBuilder(Type builder)
		{
			AddServiceHostBuilder(builder, true);
			return this;
		}

		public void Dispose()
		{
		}

		internal void AddServiceHostBuilder(Type builder, bool force)
		{
			if (typeof(IServiceHostBuilder).IsAssignableFrom(builder) == false)
			{
				throw new ArgumentException(string.Format(
					"The type {0} does not represent an IServiceHostBuilder.",
					builder.FullName), "builder");
			}

			var serviceHostBuilder = WcfUtils.GetClosedGenericDefinition(typeof(IServiceHostBuilder<>), builder);

			if (serviceHostBuilder == null)
			{
				throw new ArgumentException(string.Format(
					"The service model cannot be inferred from the builder {0}.  Did you implement IServiceHostBuilder<>?",
					builder.FullName), "builder");
			}

			if (kernel == null)
			{
				_afterInit += () => RegisterServiceHostBuilder(serviceHostBuilder, builder, force);
			}
			else
			{
				RegisterServiceHostBuilder(serviceHostBuilder, builder, force);
			}
		}

		internal void Init(IKernel kernel, WcfFacility facility)
		{
			this.kernel = kernel;
			this.facility = facility;

			ConfigureAspNetCompatibility();
			AddDefaultServiceHostBuilders();
			DefaultServiceHostFactory.RegisterContainer(kernel);

			kernel.ComponentModelCreated += Kernel_ComponentModelCreated;
			kernel.ComponentRegistered += Kernel_ComponentRegistered;
			kernel.ComponentUnregistered += Kernel_ComponentUnregistered;

			if (_afterInit != null)
			{
				_afterInit();
				_afterInit = null;
			}
		}

		private void AddDefaultServiceHostBuilders()
		{
			AddServiceHostBuilder(typeof(DefaultServiceHostBuilder), false);
			AddServiceHostBuilder(typeof(RestServiceHostBuilder), false);
		}

		private void ConfigureAspNetCompatibility()
		{
			if (aspNetCompat.HasValue)
			{
				kernel.Register(
					Component.For<AspNetCompatibilityRequirementsAttribute>()
						.Instance(new AspNetCompatibilityRequirementsAttribute
						{
							RequirementsMode = aspNetCompat.Value
						})
					);
			}
		}

		private void CreateAndOpenServiceHost(IWcfServiceModel serviceModel, ComponentModel model)
		{
			var serviceHost = CreateServiceHost(kernel, serviceModel, model);
			var serviceHosts = model.ExtendedProperties[WcfConstants.ServiceHostsKey] as IList<ServiceHost>;

			if (serviceHosts == null)
			{
				serviceHosts = new List<ServiceHost>();
				model.ExtendedProperties[WcfConstants.ServiceHostsKey] = serviceHosts;
			}

			serviceHosts.Add(serviceHost);

			serviceHost.Open();
		}

		private void CreateServiceHostWhenHandlerIsValid(IHandler handler, IWcfServiceModel serviceModel, ComponentModel model)
		{
			if (serviceModel.ShouldOpenEagerly.GetValueOrDefault(OpenServiceHostsEagerly) ||
			    handler.CurrentState == HandlerState.Valid)
			{
				CreateAndOpenServiceHost(serviceModel, model);
			}
			else
			{
				HandlersChangedDelegate onStateChanged = null;
				onStateChanged = (ref bool stateChanged) =>
				{
					if (handler.CurrentState == HandlerState.Valid && onStateChanged != null)
					{
						kernel.HandlersChanged -= onStateChanged;
						onStateChanged = null;
						CreateAndOpenServiceHost(serviceModel, model);
					}
				};
				kernel.HandlersChanged += onStateChanged;
			}
		}

		private void Kernel_ComponentModelCreated(ComponentModel model)
		{
			ExtensionDependencies dependencies = null;

			foreach (var serviceModel in ResolveServiceModels(model))
			{
				if (dependencies == null)
				{
					dependencies = new ExtensionDependencies(model, kernel)
						.Apply(new WcfServiceExtensions())
						.Apply(new WcfEndpointExtensions(WcfExtensionScope.Services));
				}

				if (serviceModel != null)
				{
					dependencies.Apply(serviceModel.Extensions);

					foreach (var endpoint in serviceModel.Endpoints)
					{
						dependencies.Apply(endpoint.Extensions);
					}
				}
			}
		}

		private void Kernel_ComponentRegistered(string key, IHandler handler)
		{
			var model = handler.ComponentModel;

			foreach (var serviceModel in ResolveServiceModels(model))
			{
				if (serviceModel.IsHosted == false)
				{
					CreateServiceHostWhenHandlerIsValid(handler, serviceModel, model);
				}
			}
		}

		private void Kernel_ComponentUnregistered(string key, IHandler handler)
		{
			var serviceHosts = handler.ComponentModel
			                   	.ExtendedProperties[WcfConstants.ServiceHostsKey] as IList<ServiceHost>;

			if (serviceHosts != null)
			{
				foreach (var serviceHost in serviceHosts)
				{
					foreach (var cleanUp in serviceHost.Extensions.FindAll<IWcfCleanUp>())
					{
						cleanUp.CleanUp();
					}
					WcfUtils.ReleaseCommunicationObject(serviceHost, CloseTimeout);
				}
			}
		}

		private void RegisterServiceHostBuilder(Type serviceHostBuilder, Type builder, bool force)
		{
			if (force || kernel.HasComponent(serviceHostBuilder) == false)
			{
				kernel.Register(Component.For(serviceHostBuilder).ImplementedBy(builder));
			}
		}

		public static ServiceHost CreateServiceHost(IKernel Kernel, IWcfServiceModel serviceModel,
		                                            ComponentModel model, params Uri[] baseAddresses)
		{
			var createServiceHost = createServiceHostCache.GetOrAdd(serviceModel.GetType(), serviceModelType =>
			{
				return (CreateServiceHostDelegate)Delegate.CreateDelegate(typeof(CreateServiceHostDelegate),
				                                                          createServiceHostMethod.MakeGenericMethod(serviceModelType));
			});

			return createServiceHost(Kernel, serviceModel, model, baseAddresses);
		}

		internal static ServiceHost CreateServiceHostInternal<M>(IKernel kernel, IWcfServiceModel serviceModel,
		                                                         ComponentModel model, params Uri[] baseAddresses)
			where M : IWcfServiceModel
		{
			var serviceHostBuilder = kernel.Resolve<IServiceHostBuilder<M>>();
			return serviceHostBuilder.Build(model, (M)serviceModel, baseAddresses);
		}

		private static IEnumerable<IWcfServiceModel> ResolveServiceModels(ComponentModel model)
		{
			var foundOne = false;

			if (model.Implementation.IsClass && !model.Implementation.IsAbstract)
			{
				foreach (var serviceModel in WcfUtils.FindDependencies<IWcfServiceModel>(model.CustomDependencies))
				{
					foundOne = true;
					yield return serviceModel;
				}

				if (!foundOne && model.Configuration != null &&
				    "true" == model.Configuration.Attributes[WcfConstants.ServiceHostEnabled])
				{
					yield return new DefaultServiceModel();
				}
			}
		}

		private delegate ServiceHost CreateServiceHostDelegate(
			IKernel Kernel, IWcfServiceModel serviceModel, ComponentModel model,
			Uri[] baseAddresses);
	}
}