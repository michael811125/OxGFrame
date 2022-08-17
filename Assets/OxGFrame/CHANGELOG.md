# CHANGELOG

## [1.3.0] - 2022-08-18
- Added ButtonPlus Component (inherit UGUI Button).
- Added NodePool Component (GameObject Pool).
- Added Canvas, CanvasScaler, GraphicRaycaster for UICanvas(Init on Awake).
- Added Canvas, GraphicRaycaster for UIBase (Init on Awake).
- Fixed When UI Canvas render mode is WorldSpace occurred transform issue.
- Fixed Bundle Offset Type of Cryptogram encrypt random issue, extend can set random seed to fixed random values.

## [1.2.0] - 2022-08-15
- Added UISafeAreaAdapter Component.
- Modified When install new app version will compare StreamingAssets cfg app version to Local cfg app version, if different will write StreamingAssets cfg app version to Local cfg.
- Modified when generate cfg file must set product name (Not load from Application.productName).
- Fixed when request server config file and request download bundle will try catch to throw error.

## [1.1.0] - 2022-07-25
- Added USFrame (Unity Scene Manager) to controll unity scene, also supported AssetBundle.
- Modified BundleMode (Bundle Stream Mode) only for build, can switch true or false from BundleConfig by define.

## [1.0.2] - 2022-07-12
- Modified right-click feature to group CoreFrame and GSIFrame in OxGFrame path.

## [1.0.1] - 2022-07-11
- Fixed CacheBundle has parameter namin error issue lead to cannot found reference.

## [1.0.0] - 2022-07-10
- Initial submission for package distribution.
