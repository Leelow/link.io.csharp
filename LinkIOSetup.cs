using Quobject.SocketIoClientDotNet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace link.io.csharp
{
    public class LinkIOSetup
    {
        private static LinkIOSetup instance;
        public static LinkIOSetup Instance {
            get
            {
                if (instance == null)
                    instance = new LinkIOSetup();
                return instance;
            }
        }

        private LinkIOImp linkIO;
        private string serverIP;
        private string user;
        private string id = string.Empty;
        private bool cSharpBinarySerializer = false;

        private LinkIOSetup()
        {

        }

        public LinkIOSetup create()
        {
            linkIO = new LinkIOImp();
            serverIP = string.Empty;
            user = string.Empty;
            id = string.Empty;
            cSharpBinarySerializer = false;
            return this;
        }

        public LinkIOSetup connectTo(String serverIP)
        {
            this.serverIP = serverIP;
            return this;
        }

        public LinkIOSetup withUser(String user)
        {
            this.user = user;
            return this;
        }

        public LinkIOSetup withID(String id)
        {
            this.id = id;
            return this;
        }

        [Obsolete("Using C# binary serializer instead of default JSON serializer disable communication with the Java client.")]
        public LinkIOSetup useCSharpBinarySerializer(bool use)
        {
            cSharpBinarySerializer = use;
            return this;
        }

        public void connect(Action<LinkIO> listener)
        {
            linkIO.setServerIP(serverIP);
            linkIO.setUser(user);
            linkIO.setUserID(id);
            linkIO.useCSharpBinarySerializer(cSharpBinarySerializer);
            linkIO.connect(listener);
        }
    }
}
