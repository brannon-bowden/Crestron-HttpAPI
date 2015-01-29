using System;
using System.Text;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharp.Net.Http;

namespace HttpAPIProcessor
{
    public delegate void DigitalChangedEventHandler(DigitalChangeEventArgs e);
    public delegate void AnalogChangedEventHandler(AnalogChangeEventArgs e);
    public delegate void SerialChangedEventHandler(SerialChangeEventArgs e);


    public class DigitalChangeEventArgs : EventArgs
    {
        public ushort joinID { get; set; }
        public ushort joinValue { get; set; }

        public DigitalChangeEventArgs()
        {
        }

        public DigitalChangeEventArgs(ushort JoinID, ushort JoinValue)
        {
            this.joinID = JoinID;
            this.joinValue = JoinValue;
        }
    }

    public class AnalogChangeEventArgs : EventArgs
    {
        public ushort joinID { get; set; }
        public ushort joinValue { get; set; }

        public AnalogChangeEventArgs()
        {
        }

        public AnalogChangeEventArgs(ushort JoinID, ushort JoinValue)
        {
            this.joinID = JoinID;
            this.joinValue = JoinValue;
        }
    }

    public class SerialChangeEventArgs : EventArgs
    {
        public ushort joinID { get; set; }
        public string joinValue { get; set; }

        public SerialChangeEventArgs()
        {
        }

        public SerialChangeEventArgs(ushort JoinID, string JoinValue)
        {
            this.joinID = JoinID;
            this.joinValue = JoinValue;
        }
    }

    public class Digital
    {
        private ushort State;
        public ushort state
        {
            get { return State; }
            set
            {
                State = value;
            }
        }
    }

    public class Analog
    {
        private ushort State;
        public ushort state
        {
            get { return State; }
            set
            {
                State = value;
            }
        }
    }

    public class Serial
    {
        private string State;
        public string state
        {
            get { return State; }
            set
            {
                State = value;
            }
        }
    }

    public static class SignalChangeEvents
    {
        public static event DigitalChangedEventHandler onDigitalValueChange;
        public static event AnalogChangedEventHandler onAnalogValueChange;
        public static event SerialChangedEventHandler onSerialValueChange;

        public static void DigitalValueChange(ushort JoinID, ushort JoinValue)
        {
            HttpAPI.Digitals[JoinID].state = JoinValue;
            SignalChangeEvents.onDigitalValueChange(new DigitalChangeEventArgs(JoinID, JoinValue));
        }

        public static void AnalogValueChange(ushort JoinID, ushort JoinValue)
        {
            HttpAPI.Analogs[JoinID].state = JoinValue;
            SignalChangeEvents.onAnalogValueChange(new AnalogChangeEventArgs(JoinID, JoinValue));
        }
        public static void SerialValueChange(ushort JoinID, string JoinValue)
        {
            HttpAPI.Serials[JoinID].state = JoinValue;
            SignalChangeEvents.onSerialValueChange(new SerialChangeEventArgs(JoinID, JoinValue));
        }        
    }

    public class HttpAPI
    {
        public static Digital[] Digitals = new Digital[101];          //Array of Media objects for Global
        public static Analog[] Analogs = new Analog[101];          //Array of Media objects for Global
        public static Serial[] Serials = new Serial[101];          //Array of Media objects for Global
        HttpServer Server;

        //Main page's HTML
        const string MainPageHTML =
           "Main page of website" +
              "<br /><br />";
        //Second page's HTML
        const string SecondPageHTML =
          "Second page of website" +
          "<br /><br />";
        //Page not found HTML
        const string PageNotFoundHTML =
          "Page not found" +
          "<br /><br />";


        /// <summary>
        /// SIMPL+ can only execute the default constructor. If you have variables that require initialization, please
        /// use an Initialize method
        /// </summary>
        public HttpAPI()
        {
            for (int i = 0; i <= 100; i++)
            {
                Digitals[i] = new Digital();
                Digitals[i].state = 0;
                Analogs[i] = new Analog();
                Analogs[i].state = 0;
                Serials[i] = new Serial();
                Serials[i].state = "";
            }

        }
        /*
        public void setDigitalSignal(int index, ushort value)
        {
            Digitals[index].state = value;
            SignalChangeEvents.DigitalValueChange((ushort) index, value);
        }
        public void setAnalogSignal(int index, ushort value)
        {
            Analogs[index].state = value;
            SignalChangeEvents.AnalogValueChange((ushort)index, value);
        }
        public void setSerialSignal(int index, string value)
        {
            Serials[index].state = value;
            SignalChangeEvents.SerialValueChange((ushort)index, value);
        }*/

        public ushort getDigitalSignal(int index)
        {
            return Digitals[index].state;
        }
        public ushort getAnalogSignal(int index)
        {
            return Analogs[index].state;
        }
        public string getSerialSignal(int index)
        {
            return Serials[index].state;
        }



        public void StartServer()
        {
            //Start listening for incoming connections
            Server.Active = true;
        }

        public void InitializeHTTPServer(String addressToAcceptConnectionFrom, int portSent)
        {

            ///Initialize all Variables


            //Create a new instance of a server
            Server = new HttpServer();
            //Set the server's IP address
            Server.ServerName = addressToAcceptConnectionFrom;
            //Set the server's port
            Server.Port = portSent;
            //Assign an event handling method to the server
            Server.OnHttpRequest += new OnHttpRequestHandler(HTTPRequestEventHandler);

        }

        public void HTTPRequestEventHandler(Object sender, OnHttpRequestArgs requestArgs) //requestArgs
        {
            //int bytesSent = 0;
            string QueryString = requestArgs.Request.Header.RequestPath;
            string JoinType = "";
            ushort JoinID = 0;
            string JoinValue = "0";

            ErrorLog.Notice(requestArgs.Request.Header.RequestType.ToString());

            if (requestArgs.Request.Header.RequestType.ToString() == "GET")
            {
                string[] words = QueryString.Split('/');
                JoinType = words[1];
                JoinID = (ushort)Convert.ToInt32(words[2]);

                switch (JoinType)
                {
                    case ("Digital"):
                        {
                            requestArgs.Response.ContentString = getDigitalSignal(JoinID).ToString();
                            break;
                        }
                    case ("Analog"):
                        {
                            requestArgs.Response.ContentString = getAnalogSignal(JoinID).ToString();
                            break;
                        }
                    case ("Serial"):
                        {
                            requestArgs.Response.ContentString = getSerialSignal(JoinID).ToString();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }

            if (requestArgs.Request.Header.RequestType.ToString() == "POST")
            {
                string[] words = QueryString.Split('/');
                JoinType = words[1];
                JoinID = (ushort)Convert.ToInt32(words[2]);
                JoinValue = words[3];

                switch (JoinType)
                {
                    case ("Digital"):
                        {
                            SignalChangeEvents.DigitalValueChange(JoinID,(ushort)Convert.ToInt32(JoinValue));
                            break;
                        }
                    case ("Analog"):
                        {
                            SignalChangeEvents.AnalogValueChange(JoinID,(ushort)Convert.ToInt32(JoinValue));
                            break;
                        }
                    case ("Serial"):
                        {
                            SignalChangeEvents.SerialValueChange(JoinID, JoinValue);
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }


            /*
            switch (requestArgs.Request.Header.RequestPath)
            {
                //"http://IP address of controller:port assigned to server" or
                //"http://IP address of controller:port assigned to server/home"
                case "/":
                case "/home":
                    {
                        var test = requestArgs.Request.QueryString.ToString();
                        ErrorLog.Notice(test + "\n");
                        //If request included data
                        if (requestArgs.Request.HasContentLength)
                        {
                            //Assign amount of bytes sent to bytesSent
                            bytesSent = requestArgs.Request.ContentLength;
                        }
                        if (bytesSent > 0)
                        {
                            String result = requestArgs.Request.ContentString;
                            CrestronConsole.Print("Data that was received from POST request: " + result + "\r\n");
                        }
                        //If the request contained no bytes
                        else
                        {
                            //Set the ContentString variable as the source of data to return
                            requestArgs.Response.ContentSource = ContentSource.ContentString;
                            //Return the homepage
                            requestArgs.Response.ContentString = MainPageHTML;
                            requestArgs.Response.Header.SetHeaderValue("Content-Type", "text/html");
                        }
                        break;
                    }
                //http://IP address of controller:port assigned to server/secondpage
                case "/secondpage":
                    {
                        requestArgs.Response.ContentSource = ContentSource.ContentString;
                        //Return the second page
                        requestArgs.Response.ContentString = SecondPageHTML;
                        requestArgs.Response.Header.SetHeaderValue("Content-Type", "text/html");
                        break;
                    }
                //http://IP address of controller:port assigned to server/anything else
                default:
                    {
                        requestArgs.Response.ContentSource = ContentSource.ContentString;
                        requestArgs.Response.ContentString = PageNotFoundHTML;
                        requestArgs.Response.Header.SetHeaderValue("Content-Type", "text/html");
                        break;
                    }
             
            }*/
        }
    }
}
