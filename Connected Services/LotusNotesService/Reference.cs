﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace LotusNotesService
{
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30422-0661")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="urn:com.cotiviti.ws.cdm", ConfigurationName="LotusNotesService.NotesWebServicesCDM")]
    public interface NotesWebServicesCDM
    {
        
        [System.ServiceModel.OperationContractAttribute(Action="CREATECDMPROJECTREQUEST", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults=true)]
        [return: System.ServiceModel.MessageParameterAttribute(Name="CREATECDMPROJECTREQUESTReturn")]
        System.Threading.Tasks.Task<object> CREATECDMPROJECTREQUESTAsync(string USERID, string PRESENTATIONID, string CDMURL, string CATEGORY, string CREATORNAME, string REQUESTDATE, string DCD, string PAYERS, string SUMMARY, string DESCRIPTION);
        
        [System.ServiceModel.OperationContractAttribute(Action="CREATEPROJECTREQUEST", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults=true)]
        [return: System.ServiceModel.MessageParameterAttribute(Name="CREATEPROJECTREQUESTReturn")]
        System.Threading.Tasks.Task<LotusNotesService.NOTESRETURN> CREATEPROJECTREQUESTAsync(string USERID, string PRESENTATIONID, string CDMURL, string CATEGORY, string CREATORNAME, string REQUESTDATE, string DCD, string PAYERS, string SUMMARY, string DESCRIPTION);
        
        [System.ServiceModel.OperationContractAttribute(Action="GETPRSTATUS", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(Style=System.ServiceModel.OperationFormatStyle.Rpc, SupportFaults=true)]
        [return: System.ServiceModel.MessageParameterAttribute(Name="GETPRSTATUSReturn")]
        System.Threading.Tasks.Task<LotusNotesService.PRDETAILS> GETPRSTATUSAsync(string SID);
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30422-0661")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:com.cotiviti.ws.cdm")]
    public partial class PRDETAILS
    {
        
        private string pRSTATUSField;
        
        private string pRURLField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
        public string PRSTATUS
        {
            get
            {
                return this.pRSTATUSField;
            }
            set
            {
                this.pRSTATUSField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
        public string PRURL
        {
            get
            {
                return this.pRURLField;
            }
            set
            {
                this.pRURLField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30422-0661")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:com.cotiviti.ws.cdm")]
    public partial class NOTESRETURN
    {
        
        private string rEQIDField;
        
        private short sUCCESSFLAGField;
        
        private string uRLField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
        public string REQID
        {
            get
            {
                return this.rEQIDField;
            }
            set
            {
                this.rEQIDField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
        public short SUCCESSFLAG
        {
            get
            {
                return this.sUCCESSFLAGField;
            }
            set
            {
                this.sUCCESSFLAGField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=2)]
        public string URL
        {
            get
            {
                return this.uRLField;
            }
            set
            {
                this.uRLField = value;
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30422-0661")]
    public interface NotesWebServicesCDMChannel : LotusNotesService.NotesWebServicesCDM, System.ServiceModel.IClientChannel
    {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.1-preview-30422-0661")]
    public partial class NotesWebServicesCDMClient : System.ServiceModel.ClientBase<LotusNotesService.NotesWebServicesCDM>, LotusNotesService.NotesWebServicesCDM
    {
        
        /// <summary>
        /// Implement this partial method to configure the service endpoint.
        /// </summary>
        /// <param name="serviceEndpoint">The endpoint to configure</param>
        /// <param name="clientCredentials">The client credentials</param>
        static partial void ConfigureEndpoint(System.ServiceModel.Description.ServiceEndpoint serviceEndpoint, System.ServiceModel.Description.ClientCredentials clientCredentials);
        
        public NotesWebServicesCDMClient() : 
                base(NotesWebServicesCDMClient.GetDefaultBinding(), NotesWebServicesCDMClient.GetDefaultEndpointAddress())
        {
            this.Endpoint.Name = EndpointConfiguration.Domino.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public NotesWebServicesCDMClient(EndpointConfiguration endpointConfiguration) : 
                base(NotesWebServicesCDMClient.GetBindingForEndpoint(endpointConfiguration), NotesWebServicesCDMClient.GetEndpointAddress(endpointConfiguration))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public NotesWebServicesCDMClient(EndpointConfiguration endpointConfiguration, string remoteAddress) : 
                base(NotesWebServicesCDMClient.GetBindingForEndpoint(endpointConfiguration), new System.ServiceModel.EndpointAddress(remoteAddress))
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public NotesWebServicesCDMClient(EndpointConfiguration endpointConfiguration, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(NotesWebServicesCDMClient.GetBindingForEndpoint(endpointConfiguration), remoteAddress)
        {
            this.Endpoint.Name = endpointConfiguration.ToString();
            ConfigureEndpoint(this.Endpoint, this.ClientCredentials);
        }
        
        public NotesWebServicesCDMClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
        }
        
        public System.Threading.Tasks.Task<object> CREATECDMPROJECTREQUESTAsync(string USERID, string PRESENTATIONID, string CDMURL, string CATEGORY, string CREATORNAME, string REQUESTDATE, string DCD, string PAYERS, string SUMMARY, string DESCRIPTION)
        {
            return base.Channel.CREATECDMPROJECTREQUESTAsync(USERID, PRESENTATIONID, CDMURL, CATEGORY, CREATORNAME, REQUESTDATE, DCD, PAYERS, SUMMARY, DESCRIPTION);
        }
        
        public System.Threading.Tasks.Task<LotusNotesService.NOTESRETURN> CREATEPROJECTREQUESTAsync(string USERID, string PRESENTATIONID, string CDMURL, string CATEGORY, string CREATORNAME, string REQUESTDATE, string DCD, string PAYERS, string SUMMARY, string DESCRIPTION)
        {
            return base.Channel.CREATEPROJECTREQUESTAsync(USERID, PRESENTATIONID, CDMURL, CATEGORY, CREATORNAME, REQUESTDATE, DCD, PAYERS, SUMMARY, DESCRIPTION);
        }
        
        public System.Threading.Tasks.Task<LotusNotesService.PRDETAILS> GETPRSTATUSAsync(string SID)
        {
            return base.Channel.GETPRSTATUSAsync(SID);
        }
        
        public virtual System.Threading.Tasks.Task OpenAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginOpen(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndOpen));
        }
        
        public virtual System.Threading.Tasks.Task CloseAsync()
        {
            return System.Threading.Tasks.Task.Factory.FromAsync(((System.ServiceModel.ICommunicationObject)(this)).BeginClose(null, null), new System.Action<System.IAsyncResult>(((System.ServiceModel.ICommunicationObject)(this)).EndClose));
        }
        
        private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.Domino))
            {
                System.ServiceModel.BasicHttpBinding result = new System.ServiceModel.BasicHttpBinding();
                result.MaxBufferSize = int.MaxValue;
                result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                result.MaxReceivedMessageSize = int.MaxValue;
                result.AllowCookies = true;
                return result;
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration)
        {
            if ((endpointConfiguration == EndpointConfiguration.Domino))
            {
                return new System.ServiceModel.EndpointAddress("http://domtst/IT/WebServices/NotesWebservices.nsf/NotesWebservicesCDM?OpenWebServ" +
                        "ice");
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
        
        private static System.ServiceModel.Channels.Binding GetDefaultBinding()
        {
            return NotesWebServicesCDMClient.GetBindingForEndpoint(EndpointConfiguration.Domino);
        }
        
        private static System.ServiceModel.EndpointAddress GetDefaultEndpointAddress()
        {
            return NotesWebServicesCDMClient.GetEndpointAddress(EndpointConfiguration.Domino);
        }
        
        public enum EndpointConfiguration
        {
            
            Domino,
        }
    }
}