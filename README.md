<h3>CRMODataGateway</h3>

this gateway routing crm dynamics rest/Odata requests to crm from out of newtowrk or AD domain. 
(This methods is not recomended. You should use for spesific scenarios.) <br>

<h3>Benefit</h3> 
<ul>
<li>No extra endpoints at backend. </li>
<li>Hihger performance than custom endpoints with xrm.sdk</li>
</ul>

<h3>Alternative and easier ways to do that</h3> 
<ul>
<li>useing Azure AD </br></li>
<li>Active Directory Factory Service.</br></li>
</ul>
Also you can always upgrade your crm to cloud. 
<h3>Scenarios</h3>
But you have lack of newtwork/domain permissions or limitation of network users at on-premise versions(2015 release 1 or higher) this is the third options.
Also you can always upgrade your crm to cloud. 
<br><br>

Note that : "prefer", "return=representation" header available on 2016 release 1 or higher.
For 2015 rel. 1  to 2016 this header doesnt cause error but not show the query result.  

<br><br>
<h3>Basic Examples (Same as Crm Rest Services only base addres will point our gate way )</h3>

GET [Router URI instead of Organization URI]/api/data/v9.2/accounts?$select=name,revenue&$top=3  <br>

Request <br>
GET [Router URI instead of Organization URI]/api/data/v9.2/accounts?$select=name<br>
&$top=3 HTTP/1.1  
Accept: application/json  
OData-MaxVersion: 4.0  
OData-Version: 4.0  <br><br>
Response<br>
HTTP/1.1 200 OK  <br>
Content-Type: application/json; odata.metadata=minimal  <br>
OData-Version: 4.0    <br>
{  
   "@odata.context":"[Organization URI]/api/data/v9.2/$metadata#accounts(name)",
   "value":[  
      {  
         "@odata.etag":"W/\"501097\"",
         "name":"Fourth Coffee (sample)",
         "accountid":"89390c24-9c72-e511-80d4-00155d2a68d1"
      },
      {  
         "@odata.etag":"W/\"501098\"",
         "name":"Litware, Inc. (sample)",
         "accountid":"8b390c24-9c72-e511-80d4-00155d2a68d1"
      },
      {  
         "@odata.etag":"W/\"501099\"",
         "name":"Adventure Works (sample)",
         "accountid":"8d390c24-9c72-e511-80d4-00155d2a68d1"
      }
   ]
}
</br></br></br>


<img src="./CRMODataGateway/Docs/OldArc.png" width="950" title="">
previous architecture
<br><br>
<img src="./CRMODataGateway/Docs/newArc.png" width="950" title="">
new architecture<br>

</br></br></br>

•	CRUD support is handled through HTTP verb support for POST, PATCH, PUT, and DELETE. (in this version only have post and get but its possible to add PATCH, PUT, and DELETE )</br>
•	Available query options are:</br>
o	$filter</br>
o	$count</br>
o	$orderby</br>
o	$skip</br>
o	$top</br>
o	$expand (only first-level expansion is supported)</br>
o	$select</br>
•	The OData service supports serving driven paging with a maximum page size of 10,000.</br></br>
Filter details</br>
There are built-in operators for $filter:</br>
•	Equals (eq)</br>
•	Not equals (ne)</br>
•	Greater than (gt)</br>
•	Greater than or equal (ge)</br>
•	Less than (lt)</br>
•	Less than or equal (le)</br>
•	And</br>
•	Or</br>
•	Not</br>
•	Addition (add)</br>
•	Subtraction (sub)</br>
•	Multiplication (mul)</br>
•	Division (div)</br>
•	Decimal division (divby)</br>
•	Modulo (mod)</br>
•	Precedence grouping ({ })</br>



Validate methods</br>
The following table summarizes the validate methods that the OData stack calls implicitly on the corresponding data entity.</br>
OData	Methods (listed in the order in which they are called)</br>
Create	1.	Clear()</br>
2.	Initvalue()</br>
3.	PropertyInfo.SetValue() for all specified fields in the request</br>
4.	Validatefield()</br>
5.	Defaultrow</br>
6.	Validatewrite()</br>
7.	Write()</br>
Update	1.	Forupdate()</br>
2.	Reread()</br>
3.	Clear()</br>
4.	Initvalue()</br>
5.	PropertyInfo.SetValue() for all specified fields in the request</br>
6.	Validatefield()</br>
7.	Defaultrow()</br>
8.	Validatewrite()</br>
9.	Write()</br>
Delete	1.	Forupdate()</br>
2.	Reread()</br>
3.	checkRestrictedDeleteActions()</br>
4.	Validatedelete()</br>
5.	Delete()</br>
