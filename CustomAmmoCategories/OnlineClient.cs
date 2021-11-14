using CustomAmmoCategoriesLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CustAmmoCategories.Online {
  public enum LinkStatus {
    Connected,
    Disconnected
  }
  public class MessageEventArgs : EventArgs {
    public byte[] Message { get; private set; }
    public bool Result { get; private set; }
    public int Index { get; private set; }
    public MessageEventArgs(int pIndex, byte[] pData, bool pResult) {
      Message = pData;
      Result = pResult;
      Index = pIndex;
    }
  }
  public class ServerEventArgs : EventArgs {
    public IPAddress IP { get; private set; }
    public int Port { get; private set; }
    public int Index { get; private set; }
    public ServerEventArgs(int pIndex, IPEndPoint pServerEndPoint) {
      this.IP = pServerEndPoint.Address;
      this.Port = pServerEndPoint.Port;
      this.Index = pIndex;
    }
  }
  public class CACTcpClient {
    private int mIndex;
    private LinkStatus mConnectionStatus = LinkStatus.Disconnected;
    private Socket mClientSocket = null;
    private NetworkStream mNetworkStream = null;
    private BackgroundWorker mBwReceiver;
    private IPEndPoint mServerEP = null;
    private IPEndPoint mClientEP = null;
    private Semaphore mSendSemaphore;
    private int mConnectionSleepTime = 5000;
    public CACTcpClient(int pIndex, string pServerAddress, int pPortNumber) {
      this.mIndex = pIndex;
      this.mSendSemaphore = new Semaphore(1, 1);
      IPAddress address = IPAddress.Loopback;
      try {
        address = IPAddress.Parse(pServerAddress);
      } catch (Exception) {
        Log.M.TWL(0, pServerAddress+ " is not valid IP address. Trying DNS");
        try {
          IPHostEntry ipHostInfo = Dns.GetHostEntry(pServerAddress);
          address = ipHostInfo.AddressList[0];
        } catch (Exception) {
          Log.M.TWL(0, pServerAddress + " can't get DNS record");
        }
      }
      IPEndPoint endPoint = new IPEndPoint(address, pPortNumber);
      this.mServerEP = endPoint;
      this.mClientEP = new IPEndPoint(IPAddress.Any, 0);
      this.mSendSemaphore = new Semaphore(1, 1);
    }
    public event EventHandler<MessageEventArgs> MessageReceived;
    public event EventHandler<MessageEventArgs> MessageSent;
    public event EventHandler<MessageEventArgs> MessageSendingFailed;
    public event EventHandler<ServerEventArgs> Disconnected;
    public event EventHandler<ServerEventArgs> ConnectingSucceeded;
    public LinkStatus ConnectionStatus {
      get {
        if (this.mConnectionStatus == LinkStatus.Connected) {
          bool result = false;
          try {
            if (this.mClientSocket != null) {
              result = !(this.mClientSocket.Poll(1, SelectMode.SelectRead) && this.mClientSocket.Available == 0);
            }
          } catch {
          }
          if (result) {
            this.mConnectionStatus = LinkStatus.Connected;
          } else {
            this.mConnectionStatus = LinkStatus.Disconnected;
          }
        } else {
          this.mConnectionStatus = LinkStatus.Disconnected;
        }

        return this.mConnectionStatus;
      }
    }
    public void ConnectToServer(int pSleepingInterval) {
      this.mConnectionSleepTime = pSleepingInterval;
      BackgroundWorker bwConnector = new BackgroundWorker();
      bwConnector.DoWork += new DoWorkEventHandler(this.BwConnector_DoWork);
      bwConnector.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BwConnector_RunWorkerCompleted);
      bwConnector.RunWorkerAsync();
    }
    public void SendCommand(byte[] pMessage) {
      {
        if (this.ConnectionStatus == LinkStatus.Connected) {
          BackgroundWorker bwSendWorker = new BackgroundWorker();
          bwSendWorker.DoWork += new DoWorkEventHandler(this.BwSendWorker_DoWork);
          bwSendWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(this.BwSendWorker_RunWorkerCompleted);
          bwSendWorker.WorkerSupportsCancellation = true;
          bwSendWorker.RunWorkerAsync(pMessage);
        } else {
          this.OnMessageSendingFailed(new MessageEventArgs(this.mIndex, pMessage, false));
        }
      }
    }
    public void DisconnectFromServer(bool pCanRaise) {
      try {
        if (this.ConnectionStatus == LinkStatus.Connected) {
          try {
            this.mBwReceiver.CancelAsync();
            this.mBwReceiver.Dispose();
          } catch {
          }
        }
        try {
          this.mClientSocket.Shutdown(SocketShutdown.Both);
          this.mClientSocket.Close();
        } catch {
        }
      } finally {
        this.mClientSocket = null;
      }

      this.SocketDisconnected(pCanRaise);
    }
    protected virtual void OnMessageReceived(MessageEventArgs e) {
      if (this.MessageReceived != null) {
        this.MessageReceived(this, e);
      }
    }
    protected virtual void OnMessageSent(MessageEventArgs e) {
      if (this.MessageSent != null) {
        this.MessageSent(this, e);
      }
    }
    protected virtual void OnMessageSendingFailed(MessageEventArgs e) {
      if (this.MessageSendingFailed != null) {
        this.MessageSendingFailed(this, e);
      }
    }
    protected virtual void OnServerDisconnected(ServerEventArgs e) {
      this.DisconnectFromServer(true);
    }
    protected virtual void OnConnectionSucceeded() {
      this.mConnectionStatus = LinkStatus.Connected;
      if (this.ConnectingSucceeded != null) {
        this.ConnectingSucceeded(this, new ServerEventArgs(this.mIndex, this.mServerEP));
      }
    }
    protected virtual void OnConnectingFailed() {
      this.DisconnectFromServer(true);
    }
    private void BwConnector_DoWork(object sender, DoWorkEventArgs e) {
      bool result = false;
      try {
        Thread.Sleep(this.mConnectionSleepTime);
        this.mClientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        this.mClientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        this.mClientSocket.Bind(this.mClientEP);
        // thread gets block until it gets response for server
        this.mClientSocket.Connect(this.mServerEP);
        result = true;
      }
      //// catch generic exception
      catch {
        result = false;
      }
      e.Result = result;
    }
    private void BwConnector_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      try {
        //// raise connection fail event if client not connected to server
        if (!(bool)e.Result) {
          this.OnConnectingFailed();
        } else {
          this.mNetworkStream = new NetworkStream(this.mClientSocket);
          this.mBwReceiver = new BackgroundWorker();
          this.mBwReceiver.WorkerSupportsCancellation = true;
          this.mBwReceiver.DoWork += new DoWorkEventHandler(this.BwReceiver_DoWork);
          this.mBwReceiver.RunWorkerAsync();
          this.OnConnectionSucceeded();
        }
        ((BackgroundWorker)sender).Dispose();
      }
      // catch generic exception if any thing happens, this is for safe
      catch {
      }
    }
    private void BwReceiver_DoWork(object sender, DoWorkEventArgs e) {
      while (this.ConnectionStatus == LinkStatus.Connected && (this.mNetworkStream != null)) {
        try {
          if (this.mNetworkStream.CanRead) {
            byte[] data = new byte[1024];
            int noOfBytesRead = 0;
            noOfBytesRead = this.mNetworkStream.Read(data, 0, data.Length);
            if (noOfBytesRead > 0) {
              byte[] receivedData = new byte[noOfBytesRead];
              Array.Copy(data, receivedData, receivedData.Length);
              MessageEventArgs mArgs = new MessageEventArgs(this.mIndex, receivedData, true);
              this.OnMessageReceived(mArgs);
            }
          } else {
          }
        } catch {
          break;
        }
      }
      this.OnServerDisconnected(new ServerEventArgs(this.mIndex, this.mServerEP));
    }
    private void BwSendWorker_DoWork(object sender, DoWorkEventArgs e) {
      byte[] sendData = (byte[])e.Argument;
      MessageEventArgs args = null;
      //// check for connectivity
      if (this.ConnectionStatus == LinkStatus.Connected && sendData.Length > 0) {
        try {
          this.mSendSemaphore.WaitOne();
          this.mNetworkStream.Write(sendData, 0, sendData.Length);
          this.mNetworkStream.Flush();
          this.mSendSemaphore.Release();
          args = new MessageEventArgs(this.mIndex, sendData, true);
          e.Result = args;
        }
        //// catch generic exception, for safe
        catch {
          args = new MessageEventArgs(this.mIndex, sendData, false);
          this.mSendSemaphore.Release();
          e.Result = args;
        }
      } else {
        args = new MessageEventArgs(this.mIndex, sendData, false);
        e.Result = args;
      }
    }
    private void BwSendWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      try {
        MessageEventArgs args = e.Result as MessageEventArgs;
        if (args != null) {
          // raised when message is sent to server successfully
          if ((!e.Cancelled && e.Error == null) && (bool)args.Result) {
            this.OnMessageSent(new MessageEventArgs(this.mIndex, args.Message, args.Result));
          }
          // raised when message sending is failed to server
          else {
            this.OnMessageSendingFailed(new MessageEventArgs(this.mIndex, args.Message, args.Result));
          }
        } else {
          this.OnMessageSendingFailed(new MessageEventArgs(this.mIndex, null, false));
        }
        ((BackgroundWorker)sender).Dispose();
      } catch { }
    }
    public void SocketDisconnected(bool pCanRaise) {
      this.mConnectionStatus = LinkStatus.Disconnected;
      if (this.Disconnected != null && pCanRaise) {
        this.Disconnected(this, new ServerEventArgs(this.mIndex, this.mServerEP));
      }
    }
  }
  public class CACOnlineClient {
    private CACTcpClient mTcpClinet = null;
    //private bool mTransportConnectionStatus = false;
    private int mReconnect = 10;
    public int Reconnect {
      get {
        return this.mReconnect;
      }
      set {
        this.mReconnect = value;
      }
    }
    public bool TransportConnectionStatus {
      get {
        bool result = false;
        if (this.mTcpClinet != null) {
          if (this.mTcpClinet.ConnectionStatus == LinkStatus.Connected) {
            result = true;
          }
        }
        return result;
      }
    }
    public void InitializeTransport() {
      if (this.DeInitializeTransport()) {
        this.mTcpClinet = new CACTcpClient(0, CustomAmmoCategories.Settings.OnlineServerHost, CustomAmmoCategories.Settings.OnlineServerServicePort);
        this.mTcpClinet.ConnectingSucceeded += new EventHandler<ServerEventArgs>(Transport_ConnectingSucceeded);
        this.mTcpClinet.Disconnected += new EventHandler<ServerEventArgs>(Transport_Disconnected);
        this.mTcpClinet.MessageReceived += new EventHandler<MessageEventArgs>(Transport_MessageReceived);
        this.mTcpClinet.ConnectToServer(this.Reconnect);
      }
    }
    public bool DeInitializeTransport() {
      bool result = true;
      this.mReconnect = 0;
      try {
        if (this.mTcpClinet != null) {
          this.mTcpClinet.MessageReceived -= this.Transport_MessageReceived;
          this.mTcpClinet.Disconnected -= this.Transport_Disconnected;
          this.mTcpClinet.ConnectingSucceeded -= this.Transport_ConnectingSucceeded;
          this.mTcpClinet.DisconnectFromServer(false);
        }
      } catch {
      } finally {
        this.mTcpClinet = null;
      }
      return result;
    }
    public bool SendData(byte[] pData) {
      if (this.mTcpClinet.ConnectionStatus == LinkStatus.Connected) {
        this.mTcpClinet.SendCommand(pData);
        return true;
      } else {
        return false;
      }
    }
    private void Transport_MessageReceived(object sender, MessageEventArgs e) {
      Log.M.TWL(0,"Incoming online server message queue:"+ Encoding.ASCII.GetString(e.Message));
    }
    private void Transport_Disconnected(object sender, ServerEventArgs e) {
      this.mTcpClinet.ConnectToServer(this.mReconnect);
    }
    private void Transport_ConnectingSucceeded(object sender, ServerEventArgs e) {
      Log.M.TWL(0, "Incoming online server connected succesfull");
    }
  }
  //public class 
  public static class OnlineClientHelper {
    private static CACOnlineClient client = null;
    private static int online_connection_id = 0;
    //private static int online_instance_id = 0;
    private static Stopwatch keep_alive = null;
    //private static readonly HttpClient httpClient = new HttpClient();
    public static void UpdateInstanceData() {

    }
    public static void KeepAlive() {
      if (keep_alive == null) { return; };
      if (keep_alive.ElapsedMilliseconds > 1000) {
        keep_alive.Stop();
        keep_alive.Reset();
        client?.SendData(Encoding.ASCII.GetBytes(online_connection_id.ToString()));
        keep_alive.Start();
      }
    }
    public static void Init() {
      try {
        return;
        //keep_alive = new Stopwatch();
        //keep_alive.Start();
        //online_connection_id = UnityEngine.Random.Range(0, int.MaxValue);
        //online_instance_id = UnityEngine.Random.Range(0, int.MaxValue);
        //client = new CACOnlineClient();
        //client.InitializeTransport();
        //httpClient = 
      } catch (Exception e) {
        Log.M.TWL(0, e.ToString(), true);
      }
    }
  }
}