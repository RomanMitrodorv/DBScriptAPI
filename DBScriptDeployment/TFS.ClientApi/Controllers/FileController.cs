using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Proxy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Http;
using TFS.ClientApi.Models;

namespace TFS.ClientApi.Controllers
{
    public class FileController : ApiController
    {
        private readonly VersionControlServer _versionServer;
        private readonly WorkItemStore _workItemStore;


        public FileController()
        {
            var uri = new Uri(ConfigurationManager.AppSettings["TfsURI"]);

            var login = ConfigurationManager.AppSettings["Login"];

            var password = ConfigurationManager.AppSettings["Password"];

            var credential = new NetworkCredential(login, password);

            var collection = new TfsTeamProjectCollection(uri, credential);

            collection.EnsureAuthenticated();

            _versionServer = collection.GetService<VersionControlServer>();
            _workItemStore = collection.GetService<WorkItemStore>();

        }


        [HttpGet]
        public IHttpActionResult FileValue([FromBody] TaskPath body)
        {
            try {
                var item = _versionServer.GetItem(body.Path);

                if (item != null)
                {
                    using (var stream = item.DownloadFile())
                    {
                        StreamReader reader = new StreamReader(stream, Encoding.GetEncoding(1251));

                        string fileValue = reader.ReadToEnd();

                        return Ok(new { Value = fileValue });
                    }
                }
                return NotFound();
            }
            catch (VersionControlException)
            {
                return NotFound();
            }
        }

    }
}
