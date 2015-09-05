using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
//using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting.Server;
using Microsoft.AspNet.Owin;
using Microsoft.Framework.Configuration;
using Microsoft.AspNet.Http.Features;
//using Microsoft.AspNet.Http;
using Nowin;
//using Newtonsoft.Json;

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
			return t;
			//return TaskHelpers.Await(t, () => {Console.WriteLine("done");}, (ex) => {Console.WriteLine("failed with " + ex.ToString());});
		}

		public IFeatureCollection Initialize(IConfiguration configuration)
		{
			// TODO: Parse config
			var builder = ServerBuilder.New()
				.SetAddress(IPAddress.Any)
				.SetPort(configuration["port"] != null ? Int32.Parse(configuration["port"]) : 8080)
				.SetOwinApp(OwinWebSocketAcceptAdapter.AdaptWebSockets(HandleRequest));
			var serverFeatures = new FeatureCollection();
            serverFeatures.Set<NowinServerInformation>(new NowinServerInformation(builder));
            return serverFeatures;
		}

		public IDisposable Start(IFeatureCollection serverFeatures, Func<IFeatureCollection, Task> application)
		{
			var information = (NowinServerInformation)serverFeatures.Get<NowinServerInformation>();
			_callback = application;
			INowinServer server = information.Builder.Build();
			server.Start();
			return server;
		}

		public class NowinServerInformation
		{
			public NowinServerInformation(ServerBuilder builder)
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
