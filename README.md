<p align="center">
  <img width="545" height="215" src="Docs/OxGFrame_Logo.png">
</p>

---

## 新版 OxGFrame 安裝
將舊版的 OxGFrame 全部移除，並且重新串接新版的接口。

---

## 基本介紹

OxGFrame 是基於 Unity 用於加快遊戲開發的輕量級框架，並且使用 UniTask 進行異步處理，從資源加載 (AssetLoader)、遊戲介面 (UIFrame)、遊戲場景 (GSFrame)、Unity場景 (USFrame)、遊戲物件 (EPFrame)、影音 (MediaFrame)、遊戲整合 (GSIFrame)、網路 (NetFrame)、集中式事件註冊 (EventCenter)、集中式 API 註冊 (APICenter)、Http.Acax (仿 Ajax 概念)等都進行模組化設計，能夠簡單入手與有效的加快開發效率，並且支持多平台 Win、OSX、Android、iOS，WebGL。

[Roadmap wiki](https://github.com/michael811125/OxGFrame/wiki/Roadmap)

---

## 需先安裝 (Install via git)
- Install from Package Manager [MyBox version 1.7.0 or higher](https://github.com/Deadcows/MyBox)

---

## 第三方庫 (內建)
- 使用 [UnitTask](https://github.com/Cysharp/UniTask) (最佳異步處理方案)

---

## 特別推薦 (內建)

- 使用 [UnityWebSocket](https://github.com/psygames/UnityWebSocket) (最佳 Websocket 解決方案)
- 使用 [YooAsset](https://github.com/tuyoogame/YooAsset) (強大的資源熱更新方案)
- TODO [HybirdCLR](https://github.com/focus-creative-games/hybridclr) (高效的程式熱更新方案)

---

### Unity 低版本如果有遇到 Newtonsoft 問題
- 請自行安裝 [com.unity.nuget.newtonsoft-json](https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Install-official-via-UPM)

---

## 框架 API
- AssetLoaders (using OxGFrame.AssetLoader)
- AssetPatcher (using OxGFrame.AssetLoader)
- CoreFrames (using OxGFrame.CoreFrame)
- MediaFrames (using OxGFrame.MediaFrame)

※備註 : 建議詳看各模塊的 Example。

---

## 模塊框架介紹

### AssetLoader

實現資源動態 Async 或 Sync 加載 (Dynamic Loading)，採用計數管理方式進行資源管控 (支援 Resource 與 AssetBundle)，一定要成對呼叫 Load & Unload (如果沒有成對呼叫，會導致計數不正確)。
其中集成【YooAsset】實現資源熱更新方案，並且實現【YooAsset】提供的加密接口，實現加解密方式有 Offset (偏移量方式)、XOR、HTXOR (Head-Tail XOR)、AES 實現檔案加密。

※備註 : Use "res#" will load from Resources else load from Bundle

**選擇使用 Bundle 開發時，需要先將 PatchLauncher 拖曳至場景中，才能驅動相關配置。**

- FileCryptogram (檔案加解密)
  - 運算效率 HTXOR ~= OFFSET > XOR > AES
  - 內存占用 OFFSET > AES > HTXOR = XOR 
  - AB 包體積增加 OFFSET > AES > HTXOR = XOR
  - 破解難度 AES > HTXOR > XOR > OFFSET

### 資源熱更新方案 (YooAsset)

使用【YooAsset】的 Collector 進行資源收集 (可以使用 ActiveRule 決定哪些群組需要打包，進行 Built-in 跟 Patch 資源的區分)，再使用【YooAsset】的 Builder 進行打包 (不需手動更改資源日期版號)，如有 Bundle 加密需求需先配置加密設定 YooAsset/OxGFrame Cryptogram Setting With YooAsset。

再使用 OxGFrame/AssetLoader/Bundle Config Generator 進行配置檔建立。

1. 先進行 Export App Config To StreamingAssets 建立 appconfig.json 至 StreamingAssets 中 (主要用於 App Version 比對)。
2. 再選擇 Export App Config And Bundles for CDN 輸出上傳資源，Source Folder 選擇剛剛使用【YooAsset】輸出的 Bundles 資料夾，依照自己需求是否有想要使用 Tags 進行更新包的區分，輸出後將 CDN 直接上傳至 Server。
   
- 群組分包舉例
  - 最小運行包
  - 標準運行包
  - 全部運行包 (預設 #all)

---

#### Bundle [burlconfig] (Bundle URL Config) 格式

格式如下 **(store_link 針對非 Android, iOS 平台的，可以設置主程式下載的 link)**

```
# bundle_ip = First CDN Server IP (Plan A)
# bundle_fallback_ip = Second CDN Server IP (Plan B)
# store_link = GooglePlay Store Link (https://play.google.com/store/apps/details?id=YOUR_ID) or Apple Store Link (itms-apps://itunes.apple.com/app/idYOUR_ID)

bundle_ip 127.0.0.1
bundle_fallback_ip 127.0.0.1
store_link http://

```

**\>\> 建立 burlconfig 方式 \<\<**
- 使用 OxGFrame/AssetLoader/Bundle Url Config Generator 創建 burlconfig (StreamingAssets/burlconfig)。

---

### CoreFrame

此模塊包含用於製作 UI, Game Scene, Entity Prefab, Unity Scene，針對製作對應使用 UI Prefab => UIFrame、Game Scene Prefab => GSFrame、Other Prefab => EPFrame、Unity Scene => USFrame。支援 Resources 與 AssetBundle 加載方式，並且實現物件命名綁定功能 (UIBase and GSBase = _Node@XXX, EPBase = ~Node@XXX, 類型均為 GameObject)。

- UIFrame (User Interface) : 使用 UIManager 管理掛載 UIBase 的 Prefab，另外 UI 的 MaskEvent 可以 override 自定義事件 (使用 _Node@XXX 進行物件綁定)
- GSFrame (Game Scene) : 使用 GSManager 管理掛載 GSBase 的 Prefab (使用 _Node@XXX 進行物件綁定)
- USFrame (Unity Scene) : 使用 USManager 管理 Unity 場景 (支援 AssetBundle)
  - ※備註 : Use "build#" will load from Build else load from Bundle
- EPFrame (Entity Prefab) : 使用 EPManager 管理掛載 EPBase 的 Prefab (使用 ~Node@XXX 進行綁定)

#### 常用方法說明

初始順序 Init Order: Awake (Once) > BeginInit (Once) > InitOnceComponents (Once) > InitOnceEvents (Once) > PreInit (EveryOpen) > OpenSub (EveryOpen) > OnShow (EveryOpen)

- InitOnceComponents，在此方法內初始組件。
- InitOnceEvents，在此方法內初始事件。
- OpenSub，當有異步處理或者附屬物件控制時，可以在此處理。例如 : TopUI 附屬連動開啟 LeftUI & RightUI，那麼就可以在 TopUI 中的 OpenSub 方法實現 Show LeftUI & RightUI。

#### 物件綁定說明

- 透過 collector.GetNode("BindName") 返回取得 GameObject (需注意綁定名稱)
  - UIBase & GSBase 使用 _Node@XXX
  - EPBase 使用 ~Node@XXX

※備註 : Right-Click Create/OxGFrame/CoreFrame... (Template cs and prefab)

---

### MediaFrame

此模塊包含用於製作 Audio, Video 遊戲影音，支援多平台加載方式 (Local, StreamingAssets, URL)，主要也對於 WebGL 有進行細節校正，因為 WebGL 對於 Audio 請求部分是無法取得正確長度 (官方放棄修正)，導致音訊控制會有部分缺陷，所以支援預置體製作時，可進行 Preload 請求 Clip 長度進行預設置。

- AudioFrame : 使用 AudioManager 管理掛載 AudioBase 的 Prefab，且採用 Unity Mixer 進行各音軌控制 **(需先將 AudioManager 預置體拖至場景)**
- VideoFrame : 使用 VideoManager 管理掛載 VideoBase 的 Prefab，且支援 RenderTexture, Camera

#### Audio Sound Type 說明
- Sole，唯一性 (不能重複播放)，建議 BGM (背景音樂), Voice (配音)
- SoundEffect，多實例 (可以重複播放)，建議 Fight Sound (戰鬥音效), General Sound (一般音效)

#### Video Render Mode 說明
- RenderTexture，將 Video 映射至 RenderTexture 再透過 UGUI 的 RawImage 進行渲染 (VideoBase 使用 RenderTexture.GetTemporary 跟 RenderTexture.ReleaseTemporary 創建與釋放，確保內存正確釋放 RenderTexture)
- Camera，透過 Camera 進行渲染。

#### Media [murlconfig] (Media URL Config) 格式

如果音訊跟影片來源存放於 Server，可以使用 URL 的方式進行檔案請求，格式如下 **(如果不透過 murlconfig.txt 指定 URL 的話，也可以輸入完整資源 URL 至 Prefab 中，不過缺點就是對於未來更動 URL，要進行更改維護就會非常麻煩)**

```
# audio_urlset = Audio Source Url Path
# video_urlset = Video Source Url Path

audio_urlset 127.0.0.1/audio/
video_urlset 127.0.0.1/video/
```

**\>\> 建立 murlconfig.txt 方式 \<\<**
- 使用 OxGFrame/MediaFrame/Media Url Config Generator 創建 murlconfig.txt (StreamingAssets/murlconfig.txt)。

**\>\> 加載 murlconfig.txt 方式 \<\<**
- 1. 如果選擇 Url Cfg Request Type = Assign 的方式指定 murlconfig.txt 至 prefab 中。
- 2. 如果選擇 Url Cfg Request Type = Streaming Assets 的方式請求 murlconfig.txt，將 murlconfig.txt 放至 StreamingAssets 根目錄中 (StreamingAssets/murlconfig.txt)。

**額外說明**：如果透過 URL 方式請求音訊或影片資源，建議於 WebGL 平台上使用，因為 WebGL 不支援 AssetBundle 事先指定 AudioClip 或 VideoClip (Assign 方式) 至 Prefab 中，所以提供 URL 的方式進行影音檔請求。

※備註 : Right-Click Create/OxGFrame/MediaFrame... (Template prefab)

---

### GSIFrame (Game Stage Integration)

遊戲整合模塊 (FSM 概念)，對於遊戲製作的時候缺乏整合系統，導致遊戲系統運作之間過於零散，基本上遊戲階段區分為 StartupStage (啟動階段), LogoStage (商業Logo階段), PatchStage (資源熱更階段), LoginStage (登入階段), ReloginStage (重登階段), EnterStage (進入階段), GamingStage (遊玩階段), FightStage (戰鬥階段) 等, 以上只是舉例大致上遊戲階段之間的劃分，基本上還是依照自己規劃創建為主，這些遊戲階段規劃好後，都可以使用 GSIFrame 進行整合與切換 (階段劃分後就可以自行實現每階段的運作)。

- GSIBase，遊戲階段基類，在透過 Update 切換當前階段自定義的狀態流程 (Enum) 時，可透過 StopUpdate & RunUpdate 方法進行開關設置，即可停止或繼續 Update 的每幀調用，需建立實作 => 右鍵創建
- GSIManagerBase，用於繼承實現管理層與註冊階段，需建立實作 => 右鍵創建

※備註 : Right-Click Create/OxGFrame/GSIFrame... (Template cs)

---

### NetFrame (Websocket, TCP/IP)

網路模塊，實現統一接口，依照 Websocket 狀態概念進行接口設計 (ISocket)，狀態分為 OnOpen, OnMessage, OnError, OnClose，進行事件註冊後就可以針對網路狀態進行監控，也實現多網路節點 (NetNode)，可以自行建立 Websocket NetNode 或是 TCP/IP NetNode，再由 NetManager 進行網路節點註冊進行管理操作，另外可以設置心跳檢測回調、超時處理回調、重新連接回調的各處理，並且也能實現 INetTips 接口網路訊息介面的實作。

- NetManager (網路節點管理器)
- NetNode (網路節點)
- TcpSocket (TCP/IP)
- Websock (Websocket)
- INetTips (網路狀態提示接口)

**如果沒有要使用 NetFrame 網路模塊的，可以直接刪除整個 NetFrame，並且無需安裝匯入 UnityWebSocket 插件。**

---

### EventCenter

集中式 Event 整合模塊，可以自定義每個 Event 的格式進行派送。

- EventCenter : 事件註冊調度管理，管理基類已實現單例
  - EventBase，單個 Event 基類，需建立實作 => 右鍵創建
  - EventCenterBase，用於繼承實現管理層與註冊階段，需建立實作 => 右鍵創建
  
※備註 : Right-Click Create/OxGFrame/EventCenter... (Template cs)

---

### APICenter

集中式 API 整合模塊，可以自定義每個 API 的格式進行短連接請求。

- Acax (類似 Ajax 方式，請求 API)，支援 Async & Sync
- APICenter : Http API 註冊管理，管理基類已實現單例
  - APIBase，單個 API 基類，需建立實作 => 右鍵創建
  - APICenterBase，用於繼承實現管理層與註冊階段，需建立實作 => 右鍵創建

**如果沒有要使用 APICenter 短連接請求模塊的，可以直接刪除整個 APICenter。**

※備註 : Right-Click Create/OxGFrame/APICenter... (Template cs)

---

### Utility

各通用組件 => Adapter, Pool, Timer, ButtonPlus

- Utility 
  - Timer => DeltaTimer, RealTimer, DTUpdate, RTUpdate
  - Adapter => UISafeAreaAdapter
  - Pool => NodePool (GameObject Pool)
  - ButtonPlus => Inherited by Unity Button. extend Long Press and Transition Scale
  - UMT => Unity Main Thread.

---

### Unity 版本

建議使用 Unity 2021.3.23f1(LTS) or higher 版本 - [Unity Download](https://unity3d.com/get-unity/download/archive)

---

### 基於 OxGFrame 實現的小遊戲

[FlappyBird_OxGFrame 簡易版](https://github.com/michael811125/FlappyBird_OxGFrame)

---

## License

This library is under the MIT License.