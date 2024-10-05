# Object Spawn System
オブジェクトをSpawnしたりReturn(Destroy)したりするギミックです。  
VCC/U#1.0以降向け。飲食店ワールドで嵩張るPickup飲食物を、自在に取り出したり片付けたりする等に活用できます。

適用すると、以下の動作となります。
* SpawnスイッチにInteractすることで、非表示状態であった任意のオブジェクトが出現する。
* ReturnトリガーにSpawnされたオブジェクトを重ねることで、任意のオブジェクトが非表示状態&初期位置に戻る。
* ResetスイッチにInteractすることで、任意のSpawnオブジェクト群が一括でReturnされる。

なお、 `VRCObjectPool` に追加されたSpawn対象オブジェクトは、デフォルトで非表示状態となります。

サンプルシーンを同梱しておりますので、まずはそちらをテストビルドし、動作をご確認ください。


## 使い方

VCC/U#1.0 導入済みのワールドプロジェクトに、本unitypackageをインポートします。

### Spawnスイッチの用意
1. シーンにGameObjectを追加し、 `VRCObjectPool` コンポーネントを追加します。
2. Spawnさせたいオブジェクト（いくつでも）を、 `VRCObjectPool` コンポーネントの `Pool` 配列に追加します。  
Inspector右上の鍵のマークをクリックして `VRCObjectPool` を開いた状態のままロックし、Hierarchyで複数選択して、 `Pool` の上にDrag&Dropすると便利です。
3. シーンにGameObjectを追加し、 何らかのCollider、および `SpawnObject` コンポーネントを追加します。
4. `SpawnObject` コンポーネントの `VRC Object Pool` 変数に、2までに作成した `VRCObjectPool` オブジェクトを追加します。

### Returnトリガーの用意
1. シーンにGameObjectを追加し、  `ReturnObject` コンポーネントを追加します。
2. 何らかのColliderコンポーネントを追加し、 `Is Trigger` にチェックをつけます。  
もしも全Reset専用として用いたい場合は、Collider自体不要です。
3. `VRC Object Poolオブジェクトまたは親` の配列項目に、Returnさせたいオブジェクトの `VRCObjectPool` オブジェクト、または、それらを何らかの親オブジェクトの配下としている場合は親オブジェクトを追加します。  
他のReturnObjectと同様の `VRCObjectPool` を参照させたい場合は、 `VRC Object Poolオブジェクトまたは親の参照先` 項目に対象ReturnObjectを設定します。
4. `Layer` に、Returnさせたいオブジェクトと同様のオブジェクトレイヤー番号を指定します。デフォルトはPickupレイヤーを示す13です。

### Resetスイッチの用意
すべてのSpawnオブジェクトを一括消去するスイッチが必要な場合、以下の手順に沿ってResetスイッチを用意します。

1. シーンにGameObjectを追加し、  `ResetSwitch` コンポーネントを追加します。
2. [Returnトリガーの用意](#Returnトリガーの用意) 手順に沿って、すべてのSpawnオブジェクトの `VRCObjectPool` を参照するReturnObjectを作成します。
3. ResetSwitchオブジェクトの `All Reseter` 変数に、2で作成したReturnObjectを設定します。

## 構成内容

* Script\SpawnObject.cs (&.asset)  
任意の `VRCObjectPool` からオブジェクトをSpawn（出現）させるスクリプト。  
`VRCObjectPool` に追加されたオブジェクトはデフォルトで非Activeとなります。

* Script\ReturnObject.cs (&.asset)  
SpawnObjectでSpawnされたオブジェクトをReturn（非表示化）させるスクリプト。  
あらかじめオブジェクトが所属する `VRCObjectPool` を指定する必要があります。  
別のReturnObjectを参照させることで、そのReturnObjectでReturn可能な `VRCObjectPool` を同様に使うことができます。

* Script\ResetSwitch.cs (&.asset)  
SpawnObjectでSpawnされたオブジェクトを、一括ですべてReturnさせるスクリプト。  
すべてのReturn対象 `VRCObjectPool` への参照があるReturnObjectを指定します。

* 上記以外のデータ  
実装サンプルデータです。

## 設定項目

### Spawn Object

| 変数名 | 型 | 説明 |
|--------|---|------|
| VRC Object Pool | `VRCObjectPool` | Spawn対象とするオブジェクトが所属する `VRCObjectPool` |
| Ramdom Spawn | bool | ランダムSpawn有無。チェックをつけた場合(True)、インスタンス作成時の一度だけ、Spawnの順序がランダムに変更される |
| Move Item To Hand | bool | チェックをつけた場合(True)、Spawn実行時、Spawn対象オブジェクトの初期位置に関わらず、Spawnスイッチに近い方の手の位置にSpawnオブジェクトが出現する |
| Spawn Point | Transform | 指定した場合、オブジェクトが指定の位置に出現します。ただし Move Item To Hand が優先です。 |
| Audio Source | `AudioSource` | Spawn時に再生する音源。無指定の場合は再生しない |
| Audio Clip | `AudioClip` | Spawn時に再生するオーディオクリップ。無指定の場合は `Audio Source` に指定された `AudioClip` を再生する |

### Return Object

| 変数名 | 型 | 説明 |
|--------|---|------|
| Pools | GameObject[] | Returnオブジェクトが所属する `VRCObjectPool` オブジェクト本体、またはその親 |
| Reference | `ReturnObject` | 別のReturnObjectで、何らかの `VRCObjectPool` を指定されているもの。ここに指定されたものを同様に参照する |
| Layer | int | Returnオブジェクトが所属するオブジェクトレイヤー番号。初期値はPickupレイヤーを示す13 |
| Audio Source | `AudioSource` | Return時に再生する音源。無指定の場合は再生しない |
| Audio Clip | `AudioClip` | Return時に再生するオーディオクリップ。無指定の場合は `Audio Source` に指定された `AudioClip` を再生する |

### Reset Switch

| 変数名 | 型 | 説明 |
|--------|---|------|
| All Reseter | `ResetObject` | 一括消去するための、すべてのReturn対象オブジェクトの `VRCObjectPool` を参照するReturnObject。<br>これ専用とする場合、ResetObject側にCollider不要 |
