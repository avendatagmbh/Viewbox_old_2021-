<%@page contentType="text/html;charset=UTF-8"%>
<%
	request.setCharacterEncoding("UTF-8");
%>
<HTML>
<HEAD>
<TITLE>Result</TITLE>
</HEAD>
<BODY>
	<H1>Result</H1>

	<jsp:useBean id="sampleOttoArchiveWebProxyid" scope="session"
		class="DefaultNamespace.OttoArchiveWebProxy" />
	<%
		if (request.getParameter("endpoint") != null
				&& request.getParameter("endpoint").length() > 0)
			sampleOttoArchiveWebProxyid.setEndpoint(request
					.getParameter("endpoint"));
	%>

	<%
		String method = request.getParameter("method");
		int methodID = 0;
		if (method == null)
			methodID = -1;

		if (methodID != -1)
			methodID = Integer.parseInt(method);
		boolean gotMethod = false;

		try {
			switch (methodID) {
			case 2:
				gotMethod = true;
				java.lang.String getEndpoint2mtemp = sampleOttoArchiveWebProxyid
						.getEndpoint();
				if (getEndpoint2mtemp == null) {
	%>
	<%=getEndpoint2mtemp%>
	<%
		} else {
					String tempResultreturnp3 = org.eclipse.jst.ws.util.JspUtils
							.markup(String.valueOf(getEndpoint2mtemp));
	%>
	<%=tempResultreturnp3%>
	<%
		}
				break;
			case 5:
				gotMethod = true;
				String endpoint_0id = request.getParameter("endpoint8");
				java.lang.String endpoint_0idTemp = null;
				if (!endpoint_0id.equals("")) {
					endpoint_0idTemp = endpoint_0id;
				}
				sampleOttoArchiveWebProxyid.setEndpoint(endpoint_0idTemp);
				break;
			case 10:
				gotMethod = true;
				DefaultNamespace.OttoArchiveWeb getOttoArchiveWeb10mtemp = sampleOttoArchiveWebProxyid
						.getOttoArchiveWeb();
				if (getOttoArchiveWeb10mtemp == null) {
	%>
	<%=getOttoArchiveWeb10mtemp%>
	<%
		} else {
					if (getOttoArchiveWeb10mtemp != null) {
						String tempreturnp11 = getOttoArchiveWeb10mtemp
								.toString();
	%>
	<%=tempreturnp11%>
	<%
		}
				}
				break;
			case 13:
				gotMethod = true;
				java.lang.String sayHello13mtemp = sampleOttoArchiveWebProxyid
						.sayHello();
				if (sayHello13mtemp == null) {
	%>
	<%=sayHello13mtemp%>
	<%
		} else {
					String tempResultreturnp14 = org.eclipse.jst.ws.util.JspUtils
							.markup(String.valueOf(sayHello13mtemp));
	%>
	<%=tempResultreturnp14%>
	<%
		}
				break;
			case 16:
				gotMethod = true;
				String connection_19 = request.getParameter("connection19");
				java.lang.String connection_19Temp = null;
				if (!connection_19.equals("")) {
					connection_19Temp = connection_19;
				}
				String id_1id = request.getParameter("id19");
				java.lang.String id_1idTemp = null;
				if (!id_1id.equals("")) {
					id_1idTemp = id_1id;
				}
				String userId_2id = request.getParameter("userId21");
				java.lang.String userId_2idTemp = null;
				if (!userId_2id.equals("")) {
					userId_2idTemp = userId_2id;
				}
				java.lang.String getDocumentPath16mtemp = sampleOttoArchiveWebProxyid
						.getDocumentPath(connection_19Temp, id_1idTemp, userId_2idTemp);
				if (getDocumentPath16mtemp == null) {
	%>
	<%=getDocumentPath16mtemp%>
	<%
		} else {
					String tempResultreturnp17 = org.eclipse.jst.ws.util.JspUtils
							.markup(String.valueOf(getDocumentPath16mtemp));
	%>
	<%=tempResultreturnp17%>
	<%
		}
				break;
			case 23:
				gotMethod = true;
				String connection_23 = request.getParameter("connection23");
				java.lang.String connection_23Temp = null;
				if (!connection_23.equals("")) {
					connection_23Temp = connection_23;
				}
				String search_3id = request.getParameter("search26");
				java.lang.String search_3idTemp = null;
				if (!search_3id.equals("")) {
					search_3idTemp = search_3id;
				}
				sampleOttoArchiveWebProxyid.getDocuments(connection_23Temp, search_3idTemp);
				break;
			case 32:
				gotMethod = true;
				String connection_32 = request.getParameter("connection32");
				java.lang.String connection_32Temp = null;
				if (!connection_32.equals("")) {
					connection_32Temp = connection_32;
				}
				String search_6id = request.getParameter("search35");
				java.lang.String search_6idTemp = null;
				if (!search_6id.equals("")) {
					search_6idTemp = search_6id;
				}
				long getCount32mtemp = sampleOttoArchiveWebProxyid
						.getCount(connection_32Temp, search_6idTemp);
				String tempResultreturnp33 = org.eclipse.jst.ws.util.JspUtils
						.markup(String.valueOf(getCount32mtemp));
	%>
	<%=tempResultreturnp33%>
	<%
		break;
			}
		} catch (Exception e) {
	%>
	Exception:
	<%=org.eclipse.jst.ws.util.JspUtils.markup(e.toString())%>
	Message:
	<%=org.eclipse.jst.ws.util.JspUtils.markup(e
						.getMessage())%>
	<%
		return;
		}
		if (!gotMethod) {
	%>
	result: N/A
	<%
		}
	%>
</BODY>
</HTML>