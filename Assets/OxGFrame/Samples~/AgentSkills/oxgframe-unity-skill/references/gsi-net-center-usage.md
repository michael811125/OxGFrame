# GSIFrame, NetFrame, CenterFrame Usage

## GSIFrame — game stage state machine

Namespace `OxGFrame.GSIFrame`. Define a manager (`GSIManagerBase<T>` singleton) that registers stages (`GSIBase`) in its constructor, and drive it from one MonoBehaviour.

```csharp
public class GSIManagerExample : GSIManagerBase<GSIManagerExample>
{
    public GSIManagerExample()
    {
        this.AddGameStage<NoneStageExample>();
        this.AddGameStage<StartupStageExample>();
        this.AddGameStage<LogoStageExample>();
        this.AddGameStage<PatchStageExample>();
        this.AddGameStage<LoginStageExample>();
        this.AddGameStage<EnterStageExample>();
    }

    public override void OnStart() { this.ChangeGameStage<StartupStageExample>(); }
    public override void OnUpdate(float dt = 0.0f) { base.OnUpdate(dt); }   // base call is REQUIRED (applies deferred stage changes)
}

// Driver MonoBehaviour
private void Start()  { GSIManagerExample.DriveStart(); }
private void Update() { GSIManagerExample.DriveUpdate(Time.deltaTime); }
```

Stage skeleton — lifecycle: `OnCreate` (once, lazy) → `OnEnter` → `OnUpdate` (while `runUpdate`) → `OnExit`:

```csharp
public class LogoStageExample : GSIBase
{
    public async override UniTask OnCreate() { /* once */ }
    public async override UniTask OnEnter()  { /* load scene/UI/BGM here */ }
    public override void OnUpdate(float dt = 0.0f) { /* poll conditions; this.StopUpdate() when waiting */ }
    public override void OnExit() { /* close UI/SR, StopAll audio */ }
}
```

Switching:

```csharp
GSIManagerExample.ChangeStage<PatchStageExample>();          // deferred to next DriveUpdate tick
GSIManagerExample.ChangeStage<PatchStageExample>(force: true); // immediate; only way to re-enter the current stage
// instance-side equivalents inside the manager: ChangeGameStage<U>() / ChangeGameStageForce<U>()
```

Notes: stage keys default to `typeof(U).GetHashCode()`; explicit int ids are supported via `AddGameStage<U>(int id)`. Static (`AddStage/ChangeStage/GetStage`) and instance (`AddGameStage/ChangeGameStage/GetGameStage`) APIs are mirrors of each other.

## NetFrame — network clients

Namespace `OxGFrame.NetFrame`. Nodes are keyed by an int id. Providers: `WebSocketNetProvider`, `TcpNetProvider` (binary only), `KcpNetProvider` (binary only).

```csharp
public enum NNID { WebSocket = 0, TCP = 1, KCP = 2 }

var netNode = new NetNode(new WebSocketNetProvider(), new NetTipsExample());
netNode.SetResponseBinaryHandler(recvData => { /* parse bytes */ });
// websocket also supports: netNode.SetResponseMessageHandler(text => { ... });
netNode.SetConnectingHandler(() => { /* first verification */ });
netNode.SetConnectedHandler(()  => { /* handshake done */ });
netNode.SetHeartBeatTickerTime(10f);  netNode.SetHeartBeatAction(() => { /* send ping */ });
netNode.SetOutReceiveTickerTime(60f); netNode.SetOutReceiveAction(() => { /* idle timeout */ });
netNode.SetReconnectTickerTime(5f);   netNode.SetReconnectAction(() => { /* on reconnect tick */ });

NetFrames.AddNetNode(netNode, (int)NNID.WebSocket);

NetFrames.Connect(new WebSocketNetOption("ws://127.0.0.1:10100/websocket", autoReconnectCount: 5), (int)NNID.WebSocket);
NetFrames.Connect(new TcpNetOption("127.0.0.1", 10100, 5), (int)NNID.TCP);
NetFrames.Connect(new KcpNetOption("127.0.0.1", 10100, kcp2k.KcpChannel.Reliable, 5), (int)NNID.KCP);

NetFrames.Send(Encoding.UTF8.GetBytes("Hello"), (int)NNID.WebSocket);
bool up = NetFrames.IsConnected((int)NNID.TCP);
NetFrames.Close((int)NNID.WebSocket, removeNetNode: true);

// Channel-specific KCP send:
NetFrames.GetNetNode((int)NNID.KCP).GetNetProvider<KcpNetProvider>()
         .SendBinary(kcp2k.KcpChannel.Unreliable, buffer);
```

`INetTips` (connection feedback for UI): `OnConnecting()`, `OnConnected(object)`, `OnConnectionError(object)`, `OnDisconnected(object)`, `OnReconnecting()`. `autoReconnectCount: -1` disables auto-reconnect.

## CenterFrame — API / Event registries

Namespace `OxGFrame.CenterFrame` (+ `.APICenter` / `.EventCenter`). One generic base serves both: subclass `CenterBase<TCenter, APIBase>` or `CenterBase<TCenter, EventBase>`; register in the constructor; call `Find<T>()` from anywhere. Ids default to `typeof(T).GetHashCode()`; explicit ids (hex convention `0x01`) are supported.

### APICenter

```csharp
public class APICenterExample : CenterBase<APICenterExample, APIBase>
{
    public APICenterExample() { this.Register<APIQueryTest>(); }
}

APICenterExample.Find<APIQueryTest>()?.ReqPost(new[] { "id1", "id2" }, data => { /* handle */ });
await APICenterExample.Find<APIQueryTest>().ReqPostAsync(guids, data => { });
```

API classes extend `APIBase` and typically use the built-in HTTP helper (`Http.Acax` — UnityWebRequest; also `HttpNativeClient` / `HttpNativeWebRequest` variants with the same signature):

```csharp
Http.Acax(
    "url", "POST",
    new string[,] { { "Content-Type", "application/json" }, { "Ticket", "none" } },
    new object[,] { { "guids", guids } },
    json => { var response = JObject.Parse(json); rh?.Invoke(response["data"]); }
);
// await Http.AcaxAsync(...same...);
```

### EventCenter

```csharp
public class EventCenterExample : CenterBase<EventCenterExample, EventBase>
{
    public EventCenterExample() { this.Register<EventMsgTest>(); }
}

// Event class: store payload in Emit, dispatch via HandleEvent, clear in Release
public class EventMsgTest : EventBase
{
    public event Action<int, string> eventHandler = null;
    private int _valueInt; private string _valueString;

    public void Emit(int a, string b) { this._valueInt = a; this._valueString = b; this.HandleEvent().Forget(); }
    public async override UniTaskVoid HandleEvent() { this.eventHandler?.Invoke(this._valueInt, this._valueString); this.Release(); }
    protected override void Release() { this._valueInt = 0; this._valueString = null; }
}

// Consumers subscribe/unsubscribe in OnEnable/OnDisable:
EventCenterExample.Find<EventMsgTest>().eventHandler += this.OnRecvEventHandle;
EventCenterExample.Find<EventMsgTest>()?.Emit(10, "Hello Event!");
```

Use the EventCenter to route cross-system UI requests (message dialogs, confirms) instead of opening common UI directly from unrelated gameplay code.
