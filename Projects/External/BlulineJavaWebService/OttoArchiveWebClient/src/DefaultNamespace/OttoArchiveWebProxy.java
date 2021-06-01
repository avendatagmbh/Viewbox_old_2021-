package DefaultNamespace;

public class OttoArchiveWebProxy implements DefaultNamespace.OttoArchiveWeb {
  private String _endpoint = null;
  private DefaultNamespace.OttoArchiveWeb ottoArchiveWeb = null;
  
  public OttoArchiveWebProxy() {
    _initOttoArchiveWebProxy();
  }
  
  public OttoArchiveWebProxy(String endpoint) {
    _endpoint = endpoint;
    _initOttoArchiveWebProxy();
  }
  
  private void _initOttoArchiveWebProxy() {
    try {
      ottoArchiveWeb = (new DefaultNamespace.OttoArchiveWebServiceLocator()).getOttoArchiveWeb();
      if (ottoArchiveWeb != null) {
        if (_endpoint != null)
          ((javax.xml.rpc.Stub)ottoArchiveWeb)._setProperty("javax.xml.rpc.service.endpoint.address", _endpoint);
        else
          _endpoint = (String)((javax.xml.rpc.Stub)ottoArchiveWeb)._getProperty("javax.xml.rpc.service.endpoint.address");
      }
      
    }
    catch (javax.xml.rpc.ServiceException serviceException) {}
  }
  
  public String getEndpoint() {
    return _endpoint;
  }
  
  public void setEndpoint(String endpoint) {
    _endpoint = endpoint;
    if (ottoArchiveWeb != null)
      ((javax.xml.rpc.Stub)ottoArchiveWeb)._setProperty("javax.xml.rpc.service.endpoint.address", _endpoint);
    
  }
  
  public DefaultNamespace.OttoArchiveWeb getOttoArchiveWeb() {
    if (ottoArchiveWeb == null)
      _initOttoArchiveWebProxy();
    return ottoArchiveWeb;
  }
  
  public java.lang.String sayHello() throws java.rmi.RemoteException{
    if (ottoArchiveWeb == null)
      _initOttoArchiveWebProxy();
    return ottoArchiveWeb.sayHello();
  }
  
  public java.lang.String getDocumentPath(java.lang.String connection, java.lang.String id, java.lang.String userId) throws java.rmi.RemoteException{
    if (ottoArchiveWeb == null)
      _initOttoArchiveWebProxy();
    return ottoArchiveWeb.getDocumentPath(connection, id, userId);
  }
  
  public void getDocuments(java.lang.String connection, java.lang.String search) throws java.rmi.RemoteException{
    if (ottoArchiveWeb == null)
      _initOttoArchiveWebProxy();
    ottoArchiveWeb.getDocuments(connection, search);
  }
  
  public long getCount(java.lang.String connection, java.lang.String search) throws java.rmi.RemoteException{
    if (ottoArchiveWeb == null)
      _initOttoArchiveWebProxy();
    return ottoArchiveWeb.getCount(connection, search);
  }
  
  
}