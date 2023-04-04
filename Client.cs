/*————————————————————————————————————————————————————————————————————————————
    ————————————————————————————————————————
    |  Client : An all in one Http Client  |
    ————————————————————————————————————————

© Copyright 2023 İhsan Volkan Töre.

Author              : IVT.  (İhsan Volkan Töre)
Version             : 202303162206
License             : MIT.

History             :
202303162206: IVT   : Renamed as Client. Now in Tore.Http namespace.
202003101700: IVT   : Complete rewrite. Now in Tore.Core namespace.
————————————————————————————————————————————————————————————————————————————*/
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using Tore.Core;
using static Tore.Core.Sys;


namespace Tore.Http {
    /**——————————————————————————————————————————————————————————————————————————— 
        CLASS:  Client.                                                 <summary>
        TASKS:  Manages Http httpClient requests and responses.             <para/>
        USAGE:                                                          <br/>
                Since httpClient requests and responses differ dramatically,
                Client objects give both standard and micro managed 
                request types and response handling.                    <para/>
                For standard http requests use STATIC functions: 
                Client.Send(...) and Client.SendAsync(...)              <para/>
                Otherwise, create a Client object and manipulate the
                request via request, request headers via headers 
                (maps to request.headers), etc. then use INSTANCE Send() 
                and SendAsync() routines.                               <para/>
                IMPORTANT:                                              <br/>
                content, accept and mediaType properties are 
                transferred to request just before sending it.          <para/>
                Tricky assignments must be done via                     <br/>
                    request.Content,                                    <br/>
                    request.Content.Headers.Accept,                     <br/>
                    request.Content.Headers.ContentType.MediaType       <br/>
                properties, And Respective accept, mediaType of 
                Client instance properties must be <b>empty</b>.        <para/>
                Please read the comments on the code at least once for 
                using this class efficiently.                           </summary>
    ————————————————————————————————————————————————————————————————————————————*/
    public class Client {

        #region Private properties.
        /**——————————————————————————————————————————————————————————————————————————— 
          PROP: HttpClient : httpClient [private]                           <summary>
          WARN:                                                             <br/>
            Microsoft recommends usage of a single HttpClient for all requests.
            HttpClient is thread safe and manages all requests and responses to
            the socket level.                                               </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        private static HttpClient httpClient { get; set; } = new HttpClient();

        /// <summary>
        /// Storage for query.
        /// </summary>
        private StrLst? qryLst { get; set; }
        #endregion

        #region Public properties.
        /*————————————————————————————————————————————————————————————————————————————
            ———————————————————————
            |  Public properties  |
            ———————————————————————
        ————————————————————————————————————————————————————————————————————————————*/

        /**———————————————————————————————————————————————————————————————————————————
          PROP: request.                                                <summary>
          GET : Returns HttpRequestMessage object.                      <br/>
          INFO: For cases requiring direct manipulation.                </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public HttpRequestMessage request { get; private set; }

        /**———————————————————————————————————————————————————————————————————————————
          PROP: response.                                               <summary>
          GET : Returns HttpResponseMessage object.                     <br/>
          INFO: For cases requiring direct manipulation.                </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public HttpResponseMessage? response { get; private set; }

        /**———————————————————————————————————————————————————————————————————————————
          PROP: headers.                                                <summary>
          GET : Returns HttpRequestHeaders object.                      <br/>
          INFO: For cases requiring direct manipulation.                </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public HttpRequestHeaders headers { get => request.Headers; }

        /**———————————————————————————————————————————————————————————————————————————
          PROP: method.                                                 <summary>
          GET : Returns current request method.                         <br/>
          SET : Sets request method.                                    </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public HttpMethod method {   // Request method  GET, POST etc.
            get => request.Method;
            set => request.Method = value;
        }

        /**———————————————————————————————————————————————————————————————————————————
          PROP: url                                                     <summary>
          GET : Returns request url as string.                          <br/>
          SET : Sets request url by string                              </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public string? url {           // Request url.
            get => request.RequestUri?.ToString();
            set {
                if (value != null)
                    request.RequestUri = new Uri(value);
            }
        }

        /**———————————————————————————————————————————————————————————————————————————
          PROP: encoding.                                                   <summary>
          GET : Returns request encoding to set during request.             <br/>
          SET : Sets request encoding to set during request.                </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public Encoding encoding { get; set; }

        /**———————————————————————————————————————————————————————————————————————————
          PROP: accept.                                                     <summary>
          GET : Returns accept mime type to set during request.             <br/>
          SET : Sets accept mime type to set during request.                <br/>
          INFO: Must be left null when direct manipulation required.        </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public string? accept { get; set; } // Req. accept  MIME.

        /**———————————————————————————————————————————————————————————————————————————
          PROP: mediaType.                                                  <summary>
          GET : Returns content mime type to set during request.            <br/>
          SET : Sets content mime type to set during request.               <br/>
          INFO: Must be left null when direct manipulation required.        </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public string? mediaType { get; set; } // Req. content MIME.

        /**———————————————————————————————————————————————————————————————————————————
          PROP: content  .                                                  <summary>
          GET : Returns content to set during request.                      <br/>
          SET : Sets content to set during request.                         <br/>
          INFO: Ignored when request.content is directly assigned(non null).</summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public object? content { get; set; } // Req. content.

        /**———————————————————————————————————————————————————————————————————————————
          PROP: isForm.                                                     <summary>
          GET : Returns if content must be form url encoded.                <br/>
          SET : Sets if content must be form url encoded.                   <br/>
          INFO: Irrelevant when content is directly assigned.               </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public bool isForm { get; set; }

        /**———————————————————————————————————————————————————————————————————————————
          PROP: queryList.                                                  <summary>
          GET : Returns a Clone of query list.                              </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public StrLst? queryList => qryLst?.Clone();

        /**———————————————————————————————————————————————————————————————————————————
          PROP: responseAsString.                                           <summary>
          GET : Returns response as string.                                 </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public string responseAsString {
            get => (response == null) ?
                        "" :
                        response
                            .Content
                            .ReadAsStringAsync()
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
        }

        /**———————————————————————————————————————————————————————————————————————————
          PROP: responseAsByteArray.                                        <summary>
          GET : Returns response as byte[].                                 </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public byte[]? responseAsByteArray {
            get => response?
                    .Content
                    .ReadAsByteArrayAsync()
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
        }
        #endregion

        #region Static utility methods.
        /*————————————————————————————————————————————————————————————————————————————
            ——————————————————————————————
            |   Static utility methods   |
            ——————————————————————————————
        ————————————————————————————————————————————————————————————————————————————*/

        /**———————————————————————————————————————————————————————————————————————————
          FUNC: Send [static]                                               <summary>
          TASK:                                                             <br/>
                Static method to prepare and send a request, blocking       <br/>
                until a response or an exception.[Synchronous].             <br/>
                This routine is for standard requests which does not        <br/>
                need header manipulations etc.                              <para/>
          ARGS:                                                             <br/>
            url       : string     : Target url.                            <br/>
            content   : object     : Object holding the content.            <br/>
            query     : StrLst        : Query list.  :DEF: null.               <br/>
            method    : HttpMethod : Http method. :DEF: null -> POST.       <br/>
            mediaType : string     : Media type.  :DEF: "application/json". <br/>
            encoding  : Encoding   : Encoding.    :DEF: null -> UTF8.       <br/>
            isForm    : bool       : If true content will be FormUrlEncoded.
                                                  :DEF: false.              <para/>
          RETV:                                                             <br/>
                      : Client        : Client object with response.              <para/>
          WARN:                                                             <br/>
                Rethrows all exceptions coming from HttpClient sendAsync.   </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public static Client Send(string url, object content, 
                StrLst? query = null,
                HttpMethod? method = null, 
                string mediaType = "application/json",
                Encoding? encoding = null, 
                bool isForm = false) {

            Client c = MakeCom(url, content, query, method, mediaType, encoding, isForm);
            return c.Send();
        }

        /**———————————————————————————————————————————————————————————————————————————
          FUNC: SendAsync [static]                                          <summary>
          TASK:                                                             <br/>
                Static method to prepare and send a request,                <br/>
                without blocking [Asynchronous].                            <br/>
                This routine is for standard requests which does not        <br/>
                need header manipulations etc.                              <para/>
          ARGS:                                                             <br/>
            url       : string     : Target url.                            <br/>
            content   : object     : Object holding the content.            <br/>
            query     : StrLst     : Query list.  :DEF: null.               <br/>
            method    : HttpMethod : Http method. :DEF: null -> POST.       <br/>
            mediaType : string     : Media type.  :DEF: "application/json". <br/>
            encoding  : Encoding   : Encoding.    :DEF: null -> UTF8.       <br/>
            bool      : isForm     : If true content will be FormUrlEncoded.
                                                  :DEF: false.              <para/>
          RETV:                                                             <br/>
                      : Client        : Client object with response.              <para/>
          WARN:                                                             <br/>
                Rethrows all exceptions coming from HttpClient sendAsync.   </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public static async Task<Client> SendAsync(string url, object content, 
                StrLst? query = null,
                HttpMethod? method = null,
                string mediaType = "application/json",
                Encoding? encoding = null, 
                bool isForm = false) {

            Client c = MakeCom(url, content, query, method, mediaType, encoding, isForm);
            return await c.SendAsync();
        }

        /**———————————————————————————————————————————————————————————————————————————
          FUNC: Talk [static]                                               <summary>
          TASK:                                                             <br/>
                Http <b> POST </b> content as an Utf-8 json request         <br/>
                and return response as an instance of T [Synchronous].      <br/>
                This routine is for standard requests which                 <br/>
                does not need header manipulations etc.                     <para/>
          ARGS:                                                             <br/>
            T           : Type      : Type of expected response object.     <br/>
            url         : string    : Target url.                           <br/>
            content     : object    : Request content object.               <br/>
            query       : StrLst    : Query list.       :DEF: null          <para/>
          RETV:                                                             <br/>
                        : T         : Json decoded response object.         <para/>
          WARN:                                                             <br/>
                Rethrows all exceptions coming from HttpClient sendAsync.   <br/>
                Throws E_INV_ARG  if url or content is null.                <br/>
                Throws E_COM_FAIL if not successful.                        <br/>
                Throws E_COM_INV_OBJ if response object invalid             </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public static T? Talk<T>(string url, object content, StrLst? query = null) {
            string json;

            Chk(url, nameof(url));
            Chk(content, nameof(content));
            json = JsonConvert.SerializeObject(content, Formatting.Indented);
            return Client.Send(url, json, query).ResponseObjectByJson<T>();
        }

        /**———————————————————————————————————————————————————————————————————————————
          FUNC: TalkAsync [static]                                          <summary>
          TASK:                                                             <br/>
                Http <b> POST </b> content as an Utf-8 json request         <br/>
                and return response as an instance of T [Asynchronous].     <br/>
                This routine is for standard requests which                 <br/>
                does not need header manipulations etc.                     <para/>
          ARGS:                                                             <br/>
            T           : Type      : Type of expected response object.     <br/>
            url         : string    : Target url.                           <br/>
            content     : object    : Request content object.               <br/>
            query       : StrLst       : Query list.       :DEF: null          <para/>
          RETV:                                                             <br/>
                        : T         : Json decoded response object.         <para/>
          WARN:                                                             <br/>
                Rethrows all exceptions coming from HttpClient sendAsync.   <br/>
                Throws E_INV_ARG  if url or content is null.                <br/>
                Throws E_COM_FAIL if not successful.                        <br/>
                Throws E_COM_INV_OBJ if response object invalid             </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public static async Task<T?> TalkAsync<T>(string url, object content, StrLst? query = null) {
            Client com;
            string jsn;

            Chk(url, nameof(url));
            Chk(content, nameof(content));
            jsn = JsonConvert.SerializeObject(content, Formatting.Indented);
            com = await Client.SendAsync(url, jsn, query);
            return com.ResponseObjectByJson<T>();
        }

        // Client builder for static utilities.
        private static Client MakeCom(string url, object content, 
                StrLst? query = null,
                HttpMethod? method = null,
                string mediaType = "application/json",
                Encoding? encoding = null, 
                bool isForm = false) {

            Client c = new() {
                url = url,
                content = content,
                mediaType = mediaType,
                isForm = isForm,
                encoding = encoding ?? Encoding.UTF8,
                method = method ?? HttpMethod.Post
            };
            if (query != null)
                c.AddQuery(query);
            return c;
        }
        #endregion

        #region Instance methods.
        /*————————————————————————————————————————————————————————————————————————————
            ————————————————————————
            |   Instance methods   |
            ————————————————————————
        ————————————————————————————————————————————————————————————————————————————*/

        /**———————————————————————————————————————————————————————————————————————————
          CTOR: Client.                                                    <summary>
          TASK:                                                         <br/>
                Constructs a Client object.                                <para/>
          INFO:                                                         <br/>
                Sets request to a new HttpRequestMessage.               <br/>
                Sets method to POST.                                    <br/>
                Sets encoding to UTF8.                                  <br/>
                Sets mediaType (content type) to "application/json".    </summary>                                  
        ————————————————————————————————————————————————————————————————————————————*/
        public Client() {
            request = new HttpRequestMessage();
            method = HttpMethod.Post;
            encoding = Encoding.UTF8;
            mediaType = "application/json";
        }

        /**———————————————————————————————————————————————————————————————————————————
          FUNC: Send                                                        <summary>
          TASK: Sends request in a blocking fashion. [Synchronous].         <br/>
          RETV:     : Client : this object.                                    </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public Client Send() {
            SendRequestAsync(false).GetAwaiter().GetResult();
            return this;
        }

        /**———————————————————————————————————————————————————————————————————————————
          FUNC: SendAsync.                                                  <summary>
          TASK: Sends request asynchronously.                               <br/>
          RETV:     : Client : this object.                                    </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public async Task<Client> SendAsync() {
            await SendRequestAsync(true);
            return this;
        }

        /**———————————————————————————————————————————————————————————————————————————
          FUNC: CheckResult.                                                <summary>
          TASK: Raises exception if request is not successfull.             <para/>
          RETV:     : Client : this object.                                 </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public Client CheckResult() {
            if (!response!.IsSuccessStatusCode) {
                Exc("E_CLIENT_FAIL",
                    $"Url        : {url}\n" +
                    $"Status Code: {response.StatusCode}\n" +
                    $"Reason     : {response.ReasonPhrase}");
            }
            return this;
        }

        /**———————————————————————————————————————————————————————————————————————————
          FUNC: ResponseObjectByJson                                        <summary>
          TASK:                                                             <br/>
                Checks and deserializes response json into Type T.          <para/>
          ARGS:                                                             <br/>
                T   : Type : Type of response expected.                     <para/>
          RETV:                                                             <br/>
                    : T    : Response object.                               <para/>
          WARN:                                                             <br/>
                Throws E_COM_FAIL if request was not successful.            <br/>
                Throws E_COM_INV_OBJ if response object invalid             </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public T? ResponseObjectByJson<T>() {
            CheckResult();
            try {
                return JsonConvert.DeserializeObject<T>(responseAsString, StrLst.strLstJsonSettings);
            } catch (Exception e) {
                Exc("E_CLIENT_INV_OBJ",
                    $"Url        : {url}\n" +
                    $"Class      : {typeof(T).Name}",
                    e);
                throw;
            }
        }

        /**———————————————————————————————————————————————————————————————————————————
          FUNC: AddQuery                                                    <summary>
          TASK:                                                             <br/>
                Collects Key value pairs to prepare a request uri query     <br/>
                information. Key duplication allowed.                       <para/>
          ARGS:                                                             <br/>
                query : StrLst : List containing keys and values.           <para/>
          WARN:                                                             <br/>
                Throws all exceptions coming from StrLst.                   </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public void AddQuery(StrLst query) {
            if (qryLst == null)
                qryLst = new StrLst(false, false, false);
            qryLst.Append(query);
        }

        /**———————————————————————————————————————————————————————————————————————————
          FUNC: AddQuery                                                    <summary>
          TASK:                                                             <br/>
                Collects Key value pairs to prepare a request uri query     <br/>
                information. Key duplication allowed.                       <para/>
          ARGS:                                                             <br/>
                Key     : string : A query Key.                             <br/>
                value   : string : A value corresponding to the Key.        <para/>
          WARN:                                                             <br/>
                Throws all exceptions coming from StrLst.                      </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public void AddQuery(string key, string value) {
            if (qryLst == null)
                qryLst = new StrLst(false, false, false);
            qryLst.Add(key, value);
        }

        /**———————————————————————————————————————————————————————————————————————————
          FUNC: ClearQuery.                                                 <summary>
          TASK: Clears query list.                                          <para/>
          INFO: Throws all exceptions coming from StrLst.                      </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public void ClearQuery() {
            if (qryLst != null)
                qryLst.Clear();
        }

        /**———————————————————————————————————————————————————————————————————————————
          FUNC: BuildQuery                                                  <summary>
          TASK:                                                             <br/>
                Converts the Key value pairs in query list to a get query.  <para/>
          INFO:                                                             <br/>
                Query list is a StrLst.                                        <br/>
                To populate it AddQuery methods can be used.                </summary>
        ————————————————————————————————————————————————————————————————————————————*/
        public string BuildQuery() {
            StringBuilder b;
            int i,
                l;

            if (qryLst == null)
                return "";
            l = qryLst.Count;
            if (l == 0)
                return "";
            b = new StringBuilder();
            for (i = 0; i < l; i++) {
                if (i > 0)
                    b.Append('&');
                b.AppendFormat(
                    "{0}={1}",
                    Uri.EscapeDataString(qryLst.keyLst[i]),
                    Uri.EscapeDataString((string)qryLst.objLst[i]));
            }
            return b.ToString();
        }
        #endregion

        #region Private methods.
        /*———————————————————————————————————————————————————————————————————————————
          FUNC: RequestSetup [private]
          TASK: Sets up a request right before sending.
          INFO: 
                Algorithm :
                Call ContentSetup
                Build query if exists, and Add it to url.
                If  accept is defined
                    adds it to request headers.
                If  mediaType is defined
                    adds it to request headers.

          WARN: This routine is called by SendRequestAsync().
                IT WORKS AT EVERY REQUEST.
                If accept, mediaType (content type) is
                very special, they should be assigned via request property and
                corresponding properties (accept or mediaType)
                should be left empty.
                If request.content is not null instance content will be ignored.
        ————————————————————————————————————————————————————————————————————————————*/
        private void RequestSetup() {
            ContentSetup();
            if ((qryLst != null) && (qryLst.Count > 0))
                url += "?" + BuildQuery();
            if (!accept.IsNullOrWhiteSpace())
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept!));
            if (!mediaType.IsNullOrWhiteSpace())
                request.Content!.Headers.ContentType!.MediaType = mediaType!;
        }

        /*———————————————————————————————————————————————————————————————————————————
          FUNC: ContentSetup [private]
          TASK: Sets up a request right before sending.
          INFO: 
                Algorithm :
                If  request.content is not null (some special request)
                    return, ignoring everything.
                If  content is null (request.content is also null here)
                    set request.content to "";
                    return;
                If  content is byte array
                    request content is loaded with it as byte array content.
                If  content is string
                    request content is loaded with it as string content
                    with encoding and mediaType settings.
                If  content is not StrLst (String associated object list)
                    content object is converted to StrLst. 
                    That means public properties are loaded into StrLst as
                    Key string and value objects.
                If  isForm is flagged
                    StrLst is converted to List<KeyValuePair<string, string>>
                    request content is loaded with it as FormUrlEncodedContent.
                Otherwise,
                    StrLst is converted to json string.
                    request content is loaded with it as string content
                    with encoding and mediaType settings.

          WARN: This routine is called by SendRequestAsync().
                IT WORKS AT EVERY REQUEST.
                If you set request.content manually,
                leave the Client object content property empty.
        ————————————————————————————————————————————————————————————————————————————*/
        private void ContentSetup() {
            StrLst StrLst;

            if (request.Content != null)
                return;
            if (content == null) {
                request.Content = new StringContent("");
                return;
            }
            if (content is byte[] bytArr) {
                request.Content = new ByteArrayContent(bytArr);
                return;
            }
            if (content is string str) {
                request.Content = new StringContent(str, encoding, mediaType!);
                return;
            }
            StrLst = (content is StrLst lst) ? lst : new StrLst(content);
            if (isForm) {
                request.Content = new FormUrlEncodedContent(StrLst.ToListOfKeyValuePairsOfString());
                return;
            }
            request.Content = new StringContent(StrLst.ToJson(), encoding, mediaType!);
        }

        /*———————————————————————————————————————————————————————————————————————————
          FUNC: SendRequestAsync [private].
          TASK: Chooses completion context and sends request asynchronously.
          ARGS: onCaptured : bool : If true,    await lands on calling context.
                                    If false,   await lands on any context available.
          INFO: For synchronous calls onCapture should be false.
          WARN: Rethrows all exceptions coming from HttpClient sendAsync.
        ————————————————————————————————————————————————————————————————————————————*/
        private async Task SendRequestAsync(bool onCaptured) {
            string exs;
            try {
                RequestSetup();
                response = await httpClient
                            .SendAsync(request)
                            .ConfigureAwait(onCaptured);
            } catch (Exception e) {
                if (e.InnerException != null)
                    e = e.InnerException;
                exs = (onCaptured) ? "E_CLIENT_SEND_ASYNC" : "E_CLIENT_SEND_SYNC";
                Exc(exs, $"url: {url}", e);
                throw;
            }
        }
        #endregion
    }
}
