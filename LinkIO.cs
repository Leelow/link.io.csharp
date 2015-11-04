using LinkIOcsharp.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkIOcsharp
{
    public interface LinkIO
    {

        LinkIO connectTo(String serverIP);
        LinkIO withUser(String user);
        LinkIO connect(Action listener);
        void createRoom();
        void joinRoom(String roomID, Action<String, List<User>> callback);
        
        void onUserInRoomChanged(Action<List<User>> listener);
        void on(String eventName, Action<Object> listener);
        void off(String eventName);

        void send(String eventName, Object data, Boolean receiveAlso);
        void send(String eventName, Object data);
        void send(String eventName, Newtonsoft.Json.JsonToken data, Boolean receiveAlso);
        void send(String eventName, Newtonsoft.Json.JsonToken data);
        void getLatency(Action<Object> listener);


    }
}
