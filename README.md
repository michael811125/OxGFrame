<p align="center">
  <img width="545" height="215" src="Docs/OxGFrame_Logo.png">
</p>

---

## 基本介紹

OxGFrame 是基於 Unity 用於加快遊戲開發的輕量級框架, 並且使用 UniTask 進行異步處理，從資源加載 (AssetLoader)、遊戲介面 (UIFrame)、遊戲場景 (GSFrame)、Unity場景 (USFrame)、遊戲物件 (EPFrame)、影音 (MediaFrame)、遊戲整合 (GSIFrame)、網路 (NetFrame)、事件註冊 (EventCenter)、API註冊 (APICenter)、Http.Acax (仿 Ajax 概念)等都進行模組化設計，能夠簡單入手與有效的加快開發效率，並且支持多平台 Win、Android、iOS，WebGL。

---

## 第三方庫依賴 (需先安裝)

- [UnitTask Version 2.3.1 or higher](https://github.com/Cysharp/UniTask)
- [MyBox version 1.7.0 or higher](https://github.com/Deadcows/MyBox)
- [UnityWebSocket Version 2.6.6 or higher](https://github.com/psygames/UnityWebSocket)

【備註】Unity 2021.3.4f1 以下的額外需安裝 [com.unity.nuget.newtonsoft-json](https://github.com/jilleJr/Newtonsoft.Json-for-Unity/wiki/Install-official-via-UPM)，對於 [jillejr.newtonsoft.json-for-unity](https://github.com/jilleJr/Newtonsoft.Json-for-Unity/issues/145) 附加作者額外說明 (Unity 2021.3.4f1 以上的版本此庫可以不用安裝)。

---

## 模塊框架介紹

### AssetLoader

實現資源動態加載 (Dynamic Loading)，採用計數管理方式進行資源管控 (支援 Resource 與 AssetBundle)，一定要成對呼叫 Load & Unload (如果沒有成對呼叫，會導致計數不正確) 。 其中 AssetBundle 則採用自帶的配置檔進行主程式與資源版本比對，實現資源熱更新流程，並且下載器支援斷點續傳，也對於 AssetBundle 打包出來的資源，提供現有加密方式 Offset (偏移量方式)、XOR、HTXOR (Head-Tail XOR)、AES 實現檔案加密，還有針對加速 AssetBundle 開發方案提供在 Unity Editor 編輯器下能夠切換 AssetDatabase Mode 提高在 Unity Editor 編輯器中的開發效率。

**選擇使用 Bundle 開發時，需要先將 BundleSetup 拖曳置場景中，才能驅動 BundleDistributor。**

- Cacher【CacheResource, CacheBundle】(資源主要加載器)
  - 如果沒有群組化需求，可以直接使用 Cacher 進行資源 Load & Unload (成對式)
- KeyCacher【KeyResource, KeyBundle】(Link Cacher 進行 Key 索引，用於分類資源群組快取操作)
  - 如果有群組化需求，使用 KeyCacher 指定 GroupId 進行 Load 時，則相對需要使用 KeyCacher 進行 Unload (成對式)
- BundleDistributor (資源熱更核心)
- Downloader (下載器)
  - 支援 Slice Mode (針對大檔進行切割式下載)
- FileCryptogram (檔案加解密)
  - **Bundle 加密推薦 HTXOR**
  - 運算效率 HTXOR ~= OFFSET > XOR > AES
  - 內存占用 OFFSET > AES > HTXOR = XOR 
  - AB 包體積增加 OFFSET > AES > HTXOR = XOR
  - 破解難度 AES > HTXOR > XOR > OFFSET

【備註】AssetBundle 打包建議使用 [AssetBundle Browser Plus v1.9.1 or higher](https://github.com/michael811125/AssetBundles-Browser-Plus) 作為打包策略規劃。

---

### Build AssetBundle Step Flow

**Built-in (內置資源)**
1. 使用 [AssetBundle Browser Plus](https://github.com/michael811125/AssetBundles-Browser-Plus) 進行打包
    - 勾選 [Rename Manifest File] 命名 "imf" (取決於你是使用預設名稱，還是自定義名稱)
    - [Compression] 建議選擇 [Chunk Based Compression (LZ4)] (自己決定為主)
    - [Bundle Name] 選擇 [Md5 For Bundle Name] (取決於 BundleSetup 的 Load Options 是否有勾選 [Read Md5 Bundle Name]，預設為 true)
    - 勾選 [Without Manifest] (non-use)
2. 完成 AssetBundle 的打包後，選擇 Unity 上列 BundleDistributor 中的 [Step 1. Bundle Cryptogram] (取決於你的 AssetBundle 是否要加密)
3. 完成後，開啟 [Step 3. Bundle Config Generator] 選擇 Operation Type 為 [Generate Config To Source Folder] (製作 Built-in 的配置檔)，瀏覽選擇剛剛完成打包 AssetBundle 的來源路徑資料夾。
4. 最後，開啟 [Step 4. Copy to StreamingAssets] 選擇剛剛完成輸出 bcfg 跟 AssetBundles 的 SourceFolder，將其複製到 StreamingAssets 路徑 (記得要保留 StreamingAssets 中的 burlcfg.txt)。

**Patch (更新資源)**
1. 使用 [AssetBundle Browser Plus](https://github.com/michael811125/AssetBundles-Browser-Plus) 進行打包
    - 勾選 [Rename Manifest File] 命名 "emf" (取決於你是使用預設名稱，還是自定義名稱)
    - [Compression] 建議選擇 [Chunk Based Compression (LZ4)] (自己決定為主)
    - [Bundle Name] 選擇 [Md5 For Bundle Name] (取決於 BundleSetup 的 Load Options 是否有勾選 [Read Md5 Bundle Name]，預設為 true)
	- 勾選 [Without Manifest] (non-use)
2. 完成 AssetBundle 的打包後，選擇 Unity 上列 BundleDistributor 中的 [Step 3. Bundle Config Generator] 選擇 Operation Type 為 [Export And Config From Source Folder] (製作 Patch 的配置檔)，瀏覽選擇剛剛完成打包 AssetBundle 的來源路徑資料夾，再選擇要輸出的路徑。
3. 完成後，先至 Server 創建 ExportBundles 的資料夾，裡面依照平台創建 win, android, ios, h5，準備好 Server 的資料夾後，再將剛剛輸出帶有 ProductName 的資料夾直接依照平台歸納上傳就好。

**注意 Server 路徑名稱**
- ExportBundles/win/productName
- ExportBundles/android/productName
- ExportBundles/ios/productName
- ExportBundles/h5/productName

---

**※如果有要運行 BundleDemo 方法則一**
- 1. 離線版 [Offline Mode] 找到 OxGFrame/AssetLoader/Example/BundleDemo/Offline_Mode.zip，解壓後閱讀 README.txt 說明配置，勾選 BundleSetup 中的 offline 選項。 (實際上 Offline 只是請求 StreamingAssets 中的 bcfg 進行比對而已)
- 2. 更新版 [Patch Mode] 找到 OxGFrame/AssetLoader/Example/BundleDemo/Patch_Mode.zip，解壓後閱讀 README.txt 說明配置，取消勾選 BundleSetup 中的 offline 選項。

#### AssetBundle Config 名稱 (可以自行至 BundleConfig 更改命名，參數為常數配置 Const)
- bcfg (Bundle Config)，當前版本的 AssetBundle 訊息
- rcfg (Record Config)，記錄歷代版本下載更新過的 AssetBundle 訊息
- burlcfg (Bundle URL Config)，維護資源伺服器 IP & 商店 Link URL (Google or Apple)

#### AssetBundle Manifest 區分 (可以自行至 BundleSetup 更改命名，參數為執行配置 Runtime)
- imf (Internal Manifest File)，Builtin 資源的 Manifest
- emf (External Manifest File)，Patch 資源的 Manifest

#### Bundle [burlcfg] (Bundle URL Config) 格式

建立一個名為 burlcfg.txt 的 txt 檔案，複製以下格式更改你的需求。

```
#bundle_ip = Server IP
#google_store = GooglePlay Store Link
#apple_store = Apple Store Link

bundle_ip 127.0.0.1
google_store market://details?id=YOUR_ID
apple_store itms-apps://itunes.apple.com/app/idYOUR_ID
```

**\>\> 加載 burlcfg.txt 方式 \<\<**
- 將 burlcfg.txt 放至 StreamingAssets 根目錄中 (StreamingAssets/burlcfg.txt)。

---

### CoreFrame

此模塊包含用於製作 UI, Game Scene, Entity Prefab, Unity Scene，針對製作對應使用 UI Prefab => UIFrame、Game Scene Prefab => GSFrame、Other Prefab => EPFrame、Unity Scene => USFrame 皆實現 Singleton Manager 進行控管與動態調度。支援 Resources 與 AssetBundle 加載方式 (多載)，並且實現物件命名綁定功能 (UIBase and GSBase = _Node@XXX, EPBase = ~Node@XXX, 類型均為 GameObject)。

- UIFrame (User Interface) : 使用 UIManager 管理掛載 UIBase 的 Prefab，另外 UI 的 MaskEvent 可以 override 自定義事件 (使用 _Node@XXX 進行物件綁定)
- GSFrame (Game Scene) : 使用 GSManager 管理掛載 GSBase 的 Prefab (使用 _Node@XXX 進行物件綁定)
- USFrame (Unity Scene) : 使用 USManager 管理 Unity 場景 (支援 AssetBundle)
- EPFrame (Entity Prefab) : 使用 EPManager 管理掛載 EPBase 的 Prefab (使用 ~Node@XXX 進行綁定)
- UMT (Unity Main Thread)

※備註 : Right-Click Create/OxGFrame/CoreFrame... (Template cs and prefab)

---

### MediaFrame

此模塊包含用於製作 Audio, Video 遊戲影音，支援多平台加載方式 (Local, StreamingAssets, URL)，主要也對於 WebGL 有進行細節校正，因為 WebGL 對於 Audio 請求部分是無法取得正確長度 (官方放棄修正)，導致音訊控制會有部分缺陷，所以支援預置體製作時，可進行 Preload 請求 Clip 長度進行預設置。

- AudioFrame : 使用 AudioManager 管理掛載 AudioBase 的 Prefab，且採用 Unity Mixer 進行各音軌控制 **(需先將 AudioManager 預置體拖至場景)**
- VideoFrame : 使用 VideoManager 管理掛載 VideoBase 的 Prefab，且支援 RenderTexture, Camera

#### Media [murlcfg] (Media URL Config) 格式

如果音訊跟影片來源存放於 Server，可以使用 URL 的方式進行檔案請求，建立一個名為 murlcfg.txt 的 txt 檔案，進行 URL 的維護，複製以下格式更改你的需求。 **(如果不透過 murlcfg.txt 指定 URL 的話，也可以輸入完整資源 URL 至 Prefab 中，不過缺點就是對於未來更動 URL，要進行更改維護就會非常麻煩)**。

```
#audio_urlset = Audio Source Url
#video_urlset = Video Source Url

audio_urlset http://127.0.0.1/audio_dev/Audio/
video_urlset http://127.0.0.1/video_dev/Video/
```

**\>\> 加載 murlcfg.txt 方式 \<\<**
- 1. 選擇 Url Cfg Request Type = Assign 的方式指定 murlcfg.txt 至 prefab 中。
- 2. 選擇 Url Cfg Request Type = Streaming Assets 的方式請求 murlcfg.txt，將 murlcfg.txt 放至 StreamingAssets 根目錄中 (StreamingAssets/murlcfg.txt)。

**額外說明**：如果透過 URL 方式請求音訊或影片資源，建議於 WebGL 平台上使用，因為 WebGL 不支援 AssetBundle 事先指定 AudioClip 或 VideoClip (Assign 方式) 至 Prefab 中，所以提供 URL 的方式進行影音檔請求。

※備註 : Right-Click Create/OxGFrame/MediaFrame... (Template prefab)

---

### GSIFrame (Game System Integration)

遊戲整合模塊，對於遊戲製作的時候缺乏整合系統，導致遊戲系統運作之間過於零散，基本上遊戲階段區分為 StartupStage (啟動階段), LogoStage (商業Logo階段), PatchStage (資源熱更階段), LoginStage (登入階段), ReloginStage (重登階段), EnterStage (進入階段), GamingStage (遊玩階段), FightStage (戰鬥階段) 等, 以上只是舉例大致上遊戲階段之間的劃分，基本上還是依照自己規劃創建為主，這些遊戲階段規劃好後，都可以使用 GSIFrame 進行整合與切換 (階段劃分後就可以自行實現每階段的運作)。

- GameStageBase，遊戲階段基類，在透過 Update 切換當前階段自定義的狀態流程 (Enum) 時，可透過 StopUpdateStage & RunUpdateStage 方法進行開關設置，即可停止或繼續 Update 的每幀調用 (需建立實作 => 右鍵創建)
- GameStageManagerBase，用於繼承實現管理層與註冊階段，管理基類已實現單例 (建議名稱為 GameStageManager 的實作 => 右鍵創建)

※備註 : Right-Click Create/OxGFrame/GSIFrame... (Template cs)

---

### NetFrame (Websocket, TCP/IP)

網路模塊，實現統一接口，依照 Websocket 狀態概念進行接口設計 (ISocket)，狀態分為 OnOpen, OnMessage, OnError, OnClose，進行事件註冊後就可以針對網路狀態進行監控，也實現多網路節點 (NetNode)，可以自行建立 Websocket NetNode 或是 TCP/IP NetNode，再由 NetManager 進行網路節點註冊進行管理操作，另外可以設置心跳檢測回調、超時處理回調、重新連接回調的各處理，並且也能實現 INetTips 接口網路訊息介面的實作。

- NetManager (網路節點管理器)
- NetNode (網路節點)
- TcpSocket (TCP/IP)
- Websock (Websocket)
- INetTips (網路狀態提示接口)

---

### EventCenter

事件整合模塊，透過 FuncId (0x0000 + 1, 0x0000 + 2...) 進行 Event 註冊，可以自定義每個 Event 的格式進行派送。

- EventCenter : 事件註冊調度管理，管理基類已實現單例
  - EventBase，單個 Event 基類，需建立實作 => 右鍵創建
  - EventCenterBase，EventCenter 管理基類 (建議名稱為 EventCenter 的實作 => 右鍵創建)
  
※備註 : Right-Click Create/OxGFrame/EventCenter... (Template cs)

---

### APICenter

API 整合模塊，透過 FuncId (0x0000 + 1, 0x0000 + 2...) 進行 API 註冊，可以自定義每個 API 的格式進行短連接請求。

- Acax (類似 Ajax 方式，請求 API)
- APICenter : Http API 註冊管理，管理基類已實現單例
  - APIBase，單個 API 基類，需建立實作 => 右鍵創建
  - APICenterBase，APICenter 管理基類 (建議名稱為 APICenter 的實作 => 右鍵創建)

※備註 : Right-Click Create/OxGFrame/APICenter... (Template cs)

---

### Utility

各通用組件 => Adapter, Pool, Timer, ButtonPlus

- Utility 
  - Timer => DeltaTimer, RealTimer, DTUpdate, RTUpdate
  - Adapter => UISafeAreaAdapter
  - Pool => NodePool (物件池)
  - ButtonPlus => 繼承 Unity Button，實現 Long Press 功能 + Transition Scale 功能

---

### Unity 版本

建議使用 Unity 2021.3.13f1(LTS) or higher 版本 - [Unity Download](https://unity3d.com/get-unity/download/archive)

---

### 基於 OxGFrame 實現的小遊戲

[FlappyBird_OxGFrame 簡易版](https://github.com/michael811125/FlappyBird_OxGFrame)

---

## License

This library is under the MIT License.