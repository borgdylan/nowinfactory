using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting.Server;
using Microsoft.AspNet.Owin;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.AspNet.FeatureModel;
using Microsoft.AspNet.Http.Interfaces;
using Nowin;
using Newtonsoft.Json;

namespace NowinServerFactory
{
	// dependencies
	// "Nowin": "0.11.0",
	// "Microsoft.AspNet.Hosting": "0.1-alpha-*",
	// "Microsoft.AspNet.Owin": "0.1-alpha-*"
	public class ServerFactory : IServerFactory
	{
		private Func<IFeatureCollection, Task> _callback;

		private Task HandleRequest(IDictionary<string, object> env)
		{
			var ofc = new OwinFeatureCollection(env);
			var t = _callback.Invoke(ofc);
			
			return TaskHelpers.Await(t, () => {Console.WriteLine("done");}, (ex) => {Console.WriteLine("failed with " + ex.ToString());});
		}

		public IServerInformation Initialize(IConfiguration configuration)
		{
			// TODO: Parse config
			var builder = ServerBuilder.New()
				.SetAddress(IPAddress.Any)
				.SetPort(configuration.Get("port") != null ? Int32.Parse(configuration.Get("port")) : 8080)
				.SetOwinApp(HandleRequest);
				//.SetOwinApp(OwinWebSocketAcceptAdapter.AdaptWebSockets(HandleRequest));

			return new ServerInformation(builder);
		}

		public IDisposable Start(IServerInformation serverInformation, Func<IFeatureCollection, Task> application)
		{
			var information = (ServerInformation)serverInformation;
			_callback = application;
			INowinServer server = information.Builder.Build();
			server.Start();
			return server;
		}

		public class ServerInformation : IServerInformation
		{
			public ServerInformation(ServerBuilder builder)
			{
				Builder = builder;
			}

			public ServerBuilder Builder { get; private set; }

			public string Name
			{
				get
				{
					return "Nowin";
				}
			}
		}
	}
}
