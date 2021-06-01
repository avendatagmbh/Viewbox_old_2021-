/**
 * OttoArchiveWeb.java
 *
 * This file was auto-generated from WSDL
 * by the Apache Axis 1.4 Apr 22, 2006 (06:55:48 PDT) WSDL2Java emitter.
 */

package DefaultNamespace;

public interface OttoArchiveWeb extends java.rmi.Remote {
    public long getCount(java.lang.String connection, java.lang.String search) throws java.rmi.RemoteException;
    public java.lang.String sayHello() throws java.rmi.RemoteException;
    public java.lang.String getDocumentPath(java.lang.String connection, java.lang.String id, java.lang.String userId) throws java.rmi.RemoteException;
    public void getDocuments(java.lang.String connection, java.lang.String search) throws java.rmi.RemoteException;
}
