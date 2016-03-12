using System;
using System.Collections.Generic;

// Socket.io C# implementation
using Quobject.SocketIoClientDotNet.Client;

// Json C# implementation
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using LinkIOcsharp.model;
using LinkIOcsharp.exception;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace link.io.csharp
{
    public class LinkIOImp : LinkIO
    {
        private Socket socket;
        private String serverIP;
        private String user;
        private string id = string.Empty;
        private Action<List<User>> userInRoomChangedListener;
        private Dictionary<String, Action<Event>> eventListeners;
        private bool connected = false;
        private bool cSharpBinarySerializer = false;

        internal LinkIOImp()
        {
            eventListeners = new Dictionary<String, Action<Event>>();
            id = "";

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            };
        }

        public LinkIO connect(Action<LinkIO> listener)
        {
            IO.Options opts = new IO.Options();
            Dictionary<String, String> query = new Dictionary<String, String>();
            query.Add("user", user);

            if (id != "")
                query.Add("id", id);

            opts.Query = query;
            opts.AutoConnect = false;

            socket = IO.Socket("http://" + serverIP, opts);

            socket.On("users", (e) =>
            {
                if (userInRoomChangedListener != null)
                {
                    Task.Run(() =>
                    {
                        userInRoomChangedListener.Invoke(((JArray)e).ToObject<List<User>>());
                    });
                }
            });
            
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                Task.Run(() =>
                {
                    connected = true;
                    listener.Invoke(this);
                });
            });

            socket.On(Socket.EVENT_DISCONNECT, () =>
            {
                connected = false;
            });

            socket.On("event", (Object o) =>
            {
                JObject evt = (JObject)o;
                String eventName = (String)evt.SelectToken("type");
                if (eventListeners.ContainsKey(eventName))
                {
                    Task.Run(() =>
                    {
                        eventListeners[eventName].Invoke(new Event(evt, cSharpBinarySerializer));
                    });
                }


            });
            
            socket.Connect();

            return this;
        }

        internal void useCSharpBinarySerializer(bool use)
        {
            cSharpBinarySerializer = use;
        }

        public void createRoom(Action<String> callback)
        {
            checkConnect();
            socket.Emit("createRoom", (id) =>
            {
                Task.Run(() =>
                {
                    callback.Invoke(id as String);
                });
            }, null);
        }

        public void joinRoom(String roomID, Action<String, List<User>> callback)
        {
            checkConnect();
            socket.Emit("joinRoom", (id, users) =>
            {
                Task.Run(() =>
                {
                    callback.Invoke(id as String, ((JArray)users).ToObject<List<User>>());
                });
            }, roomID);
        }

        public void onUserInRoomChanged(Action<List<User>> listener)
        {
            userInRoomChangedListener = listener;
        }

        public void on(String eventName, Action<Event> listener)
        {
            eventListeners.Add(eventName, listener);
        }

        public void off(String eventName)
        {
            eventListeners.Remove(eventName);
        }

        public void send(String eventName, Object data, Boolean receiveAlso)
        {
            JObject o = JObject.FromObject(new
            {
                me = receiveAlso,
                type = eventName,
                data = cSharpBinarySerializer ? serializeObject(data) : data
            });

            Task.Run(() => { socket.Emit("event", o); });
            
        }

        public void send(String eventName, Object data)
        {
            send(eventName, data, false);
        }

        public void send(string eventName, object data, List<User> receivers, bool receiveAlso)
        {
            List<String> ids = new List<string>();
            foreach (var user in receivers)
            {
                ids.Add(user.ID);
            }


            JObject o = JObject.FromObject(new
            {
                me = receiveAlso,
                type = eventName,
                data = cSharpBinarySerializer ? serializeObject(data) : data,
                idList = ids
            });

            Task.Run(() => { socket.Emit("eventToList", o); });
        }

        public void send(string eventName, object data, List<User> receivers)
        {
            send(eventName, data, receivers, false);
        }

        public void send(String eventName, Object data, string id)
        {
            List<String> ids = new List<string>();
            ids.Add(id);


            JObject o = JObject.FromObject(new
            {
                me = false,
                type = eventName,
                data = cSharpBinarySerializer ? serializeObject(data) : data,
                idList = ids
            });

            Task.Run(() => { socket.Emit("eventToList", o); });
        }

        public void getLatency(Action<Double> listener)
        {
            checkConnect();

            var from = DateTime.UtcNow;
            socket.Emit("ping", () =>
            {
                Double ping = Math.Round((DateTime.UtcNow - from).TotalSeconds, 3) * 1000;
                listener.Invoke(ping);
            }, null);
        }

        private void checkConnect()
        {
            if (socket == null)
                throw new NotConnectedException("ConnectIO: please call connect() before.");
            else if (false) //!socket.connected()
                throw new NotConnectedException("ConnectIO: socket disconnected.");
        }

        public void getAllUsersInCurrentRoom(Action<List<User>> callback)
        {
            checkConnect();
            socket.Emit("getAllUsers", (users) => {
                callback.Invoke(((JArray)users).ToObject<List<User>>());
            });
        }

        public bool isConnected()
        {
            return connected;
        }

        public void disconnect()
        {
            if (socket != null)
                socket.Disconnect();
        }

        public static string serializeObject(object o)
        {
            if (!o.GetType().IsSerializable)
            {
                return null;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, o);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        internal void setServerIP(string ip)
        {
            this.serverIP = ip;
        }

        internal void setUser(string user)
        {
            this.user = user;
        }

        internal void setUserID(string id)
        {
            this.id = id;
        }
    }
}
