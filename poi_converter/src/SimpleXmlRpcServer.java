import org.apache.xmlrpc.server.PropertyHandlerMapping;
import org.apache.xmlrpc.server.XmlRpcServer;
import org.apache.xmlrpc.server.XmlRpcServerConfigImpl;
import org.apache.xmlrpc.webserver.WebServer;
 
/**
 * A simple implementation of XML-RPC server for apache ws-xmlrpc
 * Version 1.2 - support single instance handler object, but not yet multi-threaded
 * Zen Sugiarto
 */
public class SimpleXmlRpcServer
{
	/* ======================================== runtime attr ========================================== */
 
	private int port;
	private WebServer webServer = null;
	private PropertyHandlerMapping phm = null;
	private SimpleXmlRpcRequestHandlerFactory handler = null;
	private XmlRpcServer xmlRpcServer = null;
 
	/* ======================================== init/destroy ========================================== */
 
	/**
	 * Creates an instance of xml rpc server, using :handler as the class that
	 * will handle all request. Note that a new instance of :handler will be
	 * created at every xml-rpc request.
	 *
	 * @param name
	 * @param port
	 * @param handler
	 * @throws Exception
	 */
	public SimpleXmlRpcServer(int port) throws Exception
	{
		this.port = port;
		// bind
		this.webServer = new WebServer(this.port);
		this.xmlRpcServer = this.webServer.getXmlRpcServer();
		this.handler = new SimpleXmlRpcRequestHandlerFactory();
 
		this.phm = new PropertyHandlerMapping();
		this.phm.setRequestProcessorFactoryFactory(this.handler);
 
	}
 
	/* ======================================== services ========================================== */
 
	/**
	 * Adds a handler instance (which CAN be stateful) for each request. Note that
	 * every public method in this object will be callable via xml-rpc client
	 *
	 * @param name - handler name
	 * @param requestHandler - handler obj instance
	 * @throws Exception
	 */
	public void addHandler(String name, Object requestHandler)
	throws Exception
	{
		this.handler.setHandler(name, requestHandler);
		this.phm.addHandler(name, requestHandler.getClass());
	}
 
	/**
	 * Start the xml-rpc server forever. In the rare case of fatal exception, the
	 * web server will be restarted automagically. This is a blocking call (not
	 * thread-based).
	 */
	public void serve_forever() throws Exception
	{
		// init
		this.xmlRpcServer.setHandlerMapping(phm);
		XmlRpcServerConfigImpl serverConfig = (XmlRpcServerConfigImpl) xmlRpcServer.getConfig();
		serverConfig.setEnabledForExtensions(true);
		this.webServer.start();
	}
 
	/* ====================================== getter/setter ======================================== */
 
	public int getPort() { return port; }
	public void setPort(int port) { this.port = port; }
	public WebServer getWebServer() { return webServer; }
	public void setWebServer(WebServer webServer) { this.webServer = webServer; }
	public PropertyHandlerMapping getPhm() { return phm; }
	public void setPhm(PropertyHandlerMapping phm) { this.phm = phm; }
	public SimpleXmlRpcRequestHandlerFactory getHandler() { return handler; }
	public void setHandler(SimpleXmlRpcRequestHandlerFactory handler) { this.handler = handler; }
	public XmlRpcServer getXmlRpcServer() { return xmlRpcServer; }
	public void setXmlRpcServer(XmlRpcServer xmlRpcServer) { this.xmlRpcServer = xmlRpcServer; } 
 
}