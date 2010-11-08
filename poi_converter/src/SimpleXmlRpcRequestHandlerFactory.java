import java.util.HashMap;
import java.util.Map;

import org.apache.xmlrpc.XmlRpcException;
import org.apache.xmlrpc.XmlRpcRequest;
import org.apache.xmlrpc.server.RequestProcessorFactoryFactory;
import org.apache.xmlrpc.server.RequestProcessorFactoryFactory.RequestProcessorFactory;

/**
 * Simple XMLRPC request handler factory - this little magic class allows
 * for a request INSTANCE to be set as the handler for the xml rpc server
 * Ideally, a client shouldn't even NEED to know what this is doing.
 */
public class SimpleXmlRpcRequestHandlerFactory implements RequestProcessorFactoryFactory,RequestProcessorFactory
{
	private Map handlerMap = new HashMap();
	public void setHandler(String name, Object handler) { this.handlerMap.put(name, handler); }
	public Object getHandler(String name) { return this.handlerMap.get(name); }
 
	public RequestProcessorFactory getRequestProcessorFactory(Class arg0)
	throws XmlRpcException { return (RequestProcessorFactory) this; }
	public Object getRequestProcessor(XmlRpcRequest request) throws XmlRpcException
	{
		String handlerName = request.getMethodName().substring(0,request.getMethodName().lastIndexOf("."));
		if( !handlerMap.containsKey(handlerName)) throw new XmlRpcException("Unknown handler: "+handlerName);
		return handlerMap.get(handlerName);
	}
}
