using System;
using System.IO;
using Nwc.XmlRpc;
using Nini.Config;

namespace GridImageService
{
    class Program
    {		
       		
        static void Main(string[] args)
        {
			

			IConfigSource config = new IniConfigSource("../../ppt2os.ini");
			int port = config.Configs["uploader"].GetInt("port");			
			XmlRpcServer server = new XmlRpcServer(port);
			server.Add("exposed", new XMLRPC_Service(config));
			Console.WriteLine("Web Server Running on port {0} ... Press ^C to Stop...", port);   
			server.Start();
		}	
	}
}
 