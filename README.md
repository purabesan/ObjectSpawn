# Object Spawn System

ドキュメントは [こちら](https://purabesan.github.io/ObjectSpawn/)

オブジェクトを Spawn したり Return(Destroy) したりするギミックです。
VCC/U#1.0 以降向け。飲食店ワールドで嵩張る Pickup 飲食物を、自在に取り出したり片付けたりする等に活用できます。

適用すると、以下の動作となります。
* Spawn スイッチに Interact することで、非表示状態であった任意のオブジェクトが出現する。
* Return トリガーに Spawn されたオブジェクトを重ねることで、任意のオブジェクトが非表示状態&初期位置に戻る。
* Reset スイッチに Interact することで、任意の Spawn オブジェクト群が一括で Return される。
* Spawn Delay 中に連続して Spawn した場合も、すべての対象が順番に指定位置へ移動する。

なお、 `VRCObjectPool` に追加された Spawn 対象オブジェクトは、デフォルトで非表示状態となります。

サンプルシーンを同梱しておりますので、まずはそちらをテストビルドし、動作をご確認ください。

## 動作確認環境

- Unity 2022.3.22f1
- VRChat SDK - Worlds 3.10.4

## 使い方

VCC/U#1.0 導入済みのワールドプロジェクトに、本 unitypackage をインポートします。

### Spawn スイッチの用意
1. シーンに GameObject を追加し、 `VRCObjectPool` コンポーネントを追加します。
2. Spawn させたいオブジェクト（いくつでも）を、 `VRCObjectPool` コンポーネントの `Pool` 配列に追加します。
Inspector 右上の鍵のマークをクリックして `VRCObjectPool` を開いた状態のままロックし、Hierarchy で複数選択して、 `Pool` の上に Drag&Drop すると便利です。
3. シーンに GameObject を追加し、何らかの Collider、および `SpawnObject` コンポーネントを追加します。
4. `SpawnObject` コンポーネントの `VRC Object Pool` 変数に、2 までに作成した `VRCObjectPool` オブジェクトを追加します。

### Return トリガーの用意
1. シーンに GameObject を追加し、`ReturnObject` コンポーネントを追加します。
2. 何らかの Collider コンポーネントを追加し、 `Is Trigger` にチェックをつけます。
もしも全 Reset 専用として用いたい場合は、Collider 自体不要です。
3. `VRC Object Pool オブジェクトまたは親` の配列項目に、Return させたいオブジェクトの `VRCObjectPool` オブジェクト、または、それらを何らかの親オブジェクトの配下としている場合は親オブジェクトを追加します。
他の ReturnObject と同様の `VRCObjectPool` を参照させたい場合は、 `VRC Object Pool オブジェクトまたは親の参照先` 項目に対象 ReturnObject を設定します。
4. `Layer` に、Return させたいオブジェクトと同様のオブジェクトレイヤー番号を指定します。デフォルトは Pickup レイヤーを示す 13 です。

### Reset スイッチの用意
すべての Spawn オブジェクトを一括消去するスイッチが必要な場合、以下の手順に沿って Reset スイッチを用意します。

1. シーンに GameObject を追加し、`ResetSwitch` コンポーネントを追加します。
2. [Return トリガーの用意](#return-トリガーの用意) 手順に沿って、すべての Spawn オブジェクトの `VRCObjectPool` を参照する ReturnObject を作成します。
3. ResetSwitch オブジェクトの `All Reseter` 変数に、2 で作成した ReturnObject を設定します。

## 構成内容

* Script/SpawnObject.cs (&.asset)
任意の `VRCObjectPool` からオブジェクトを Spawn（出現）させるスクリプト。
`VRCObjectPool` に追加されたオブジェクトはデフォルトで非 Active となります。

* Script/ReturnObject.cs (&.asset)
SpawnObject で Spawn されたオブジェクトを Return（非表示化）させるスクリプト。
あらかじめオブジェクトが所属する `VRCObjectPool` を指定する必要があります。
別の ReturnObject を参照させることで、その ReturnObject で Return 可能な `VRCObjectPool` を同様に使うことができます。

* Script/ResetSwitch.cs (&.asset)
SpawnObject で Spawn されたオブジェクトを、一括ですべて Return させるスクリプト。
すべての Return 対象 `VRCObjectPool` への参照がある ReturnObject を指定します。

* 上記以外のデータ
実装サンプルデータです。

## 設定項目

### Spawn Object

| 変数名 | 型 | 説明 |
|--------|---|------|
| VRC Object Pool | `VRCObjectPool` | Spawn 対象とするオブジェクトが所属する `VRCObjectPool` |
| Random Spawn | bool | ランダム Spawn 有無。チェックをつけた場合 (True)、インスタンス作成時の一度だけ、Spawn の順序がランダムに変更される |
| Move Item To Hand | bool | チェックをつけた場合 (True)、Spawn 実行時、Spawn 対象オブジェクトの初期位置に関わらず、Spawn スイッチに近い方の手の位置に Spawn オブジェクトが出現する |
| Spawn Point | Transform | 指定した場合、オブジェクトが指定の位置に出現します。ただし Move Item To Hand が優先です。 |
| Spawn Delay Frames | int | Spawn 後に移動を実行するまでの遅延フレーム数。<br>Spawn したアイテムの移動処理を安定化させます。連続 Spawn 時は内部キューによりすべての対象を処理します。 |
| Audio Source | `AudioSource` | Spawn 時に再生する音源。無指定の場合は再生しない |
| Audio Clip | `AudioClip` | Spawn 時に再生するオーディオクリップ。無指定の場合は `Audio Source` に指定された `AudioClip` を再生する |

### Return Object

| 変数名 | 型 | 説明 |
|--------|---|------|
| Pools | GameObject[] | Return オブジェクトが所属する `VRCObjectPool` オブジェクト本体、またはその親 |
| Reference | `ReturnObject` | 別の ReturnObject で、何らかの `VRCObjectPool` を指定されているもの。ここに指定されたものを同様に参照する |
| Layer | int | Return オブジェクトが所属するオブジェクトレイヤー番号。初期値は Pickup レイヤーを示す 13 |
| Audio Source | `AudioSource` | Return 時に再生する音源。無指定の場合は再生しない |
| Audio Clip | `AudioClip` | Return 時に再生するオーディオクリップ。無指定の場合は `Audio Source` に指定された `AudioClip` を再生する |

### Reset Switch

| 変数名 | 型 | 説明 |
|--------|---|------|
| All Reseter | `ReturnObject` | 一括消去するための、すべての Return 対象オブジェクトの `VRCObjectPool` を参照する ReturnObject。<br>これ専用とする場合、ReturnObject 側に Collider 不要 |

## 作者

* 開発・配布: Purabe Works
* BOOTH: https://purabeworks.booth.pm/
* GitHub: https://github.com/purabesan/ObjectSpawn
* お問い合わせ: BOOTH のメッセージからご連絡ください。

## ライセンス

このスクリプトは MIT ライセンスのもとで公開されています。
詳細は同梱の `LICENSE.txt` をご覧ください。
