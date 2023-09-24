# Object Spawn System
This is a VRChat U# gimmick for spawning and returning (destroying) objects. Compatible with VCC/U# 1.0 and later versions. It can be used to freely retrieve and clean up bulky pickup food and drinks in restaurant-themed worlds.

This is a culmination of the author's knowledge gained while using it in restaurant-themed events as an event organizer or world creator.

## How to Use

Import this unitypackage into your VCC/U# 1.0 or later world project.

### Setting Up the Spawn Switch
1. Add a GameObject to the scene and attach the `VRCObjectPool` component to it.
2. Add the objects you want to spawn (as many as you like) to the `Pool` array of the `VRCObjectPool` component. It's convenient to keep the `VRCObjectPool` window open by clicking the lock icon in the upper-right corner of the Inspector, then selecting multiple objects in the Hierarchy and dragging them onto the `Pool`.
3. Add another GameObject to the scene, attach some form of Collider, and add the `SpawnObject` component.
4. In the `VRC Object Pool` variable of the `SpawnObject` component, add the `VRCObjectPool` object you created in step 2.

### Setting Up the Return Trigger
1. Add a GameObject to the scene and attach the `ReturnObject` component to it.
2. Add some form of Collider component and check "Is Trigger." If you want it to be used exclusively for resetting, the Collider itself is not necessary.
3. In the `VRC Object Pool Object or Parent` array items, add the `VRCObjectPool` objects of the objects you want to return, or if they are under some parent object, add the parent object. If you want to reference the same `VRCObjectPool` as other `ReturnObject`, set the target `ReturnObject` in the "VRC Object Pool Object or Parent Reference" field.
4. Specify the object layer in the "Layer" field, which should match the layer number of the objects you want to return. The default is 13, indicating the Pickup layer.

### Setting Up the Reset Switch
If you need a switch to clear all spawned objects at once, follow these steps to set up a Reset Switch.

1. Add a GameObject to the scene and attach the `ResetSwitch` component to it.
2. Create ReturnObjects that reference all `VRCObjectPool` objects of all spawned objects following the [Setting Up the Return Trigger](#Setting-Up-the-Return-Trigger) steps.
3. Set the ReturnObjects created in step 2 to the "All Reseter" variable of the ResetSwitch object.

## Contents

* Script\SpawnObject.cs (&.asset)  
A script for spawning objects from any `VRCObjectPool`. Objects added to `VRCObjectPool` are initially set to inactive.

* Script\ReturnObject.cs (&.asset)  
A script for returning (hiding) objects spawned by SpawnObject. You need to specify the `VRCObjectPool` to which the object belongs in advance. You can use another ReturnObject to reference the same `VRCObjectPool` for return.

* Script\ResetSwitch.cs (&.asset)  
A script for returning all spawned objects in bulk. Specify the ReturnObjects that reference all target `VRCObjectPool` objects.

* Other Data  
Sample implementation data.

## Configuration

### Spawn Object

| Variable Name | Type | Description |
|--------|---|------|
| VRC Object Pool | `VRCObjectPool` | The `VRCObjectPool` to which the object to be spawned belongs. |
| Random Spawn | bool | Whether to spawn randomly. If checked (True), the order of spawning is randomized once when the instance is created. |
| Move Item To Hand | bool | If checked (True), the spawned object will appear near the hand closest to the Spawn switch, regardless of the initial position of the spawn target object. |
| Audio Source | `AudioSource` | The audio source to be played when spawning. If not specified, no audio will be played. |
| Audio Clip | `AudioClip` | The audio clip to be played when spawning. If not specified, the audio clip specified in `Audio Source` will be played. |
| Execute Custom Event | bool | Whether to globally execute custom methods on `UdonBehaviour` for each spawned object and objects specified in `External Objects`. |
| External Objects | `GameObject[]` | Objects to be targeted other than Spawn objects when executing custom methods during SpawnObject execution. These objects or their parent objects contain the desired custom methods to be executed.

If you want to use custom methods, please copy and use the methods in the `Custom Event Names` field.

### Return Object

| Variable Name | Type | Description |
|--------|---|------|
| Pools | GameObject[] | The `VRCObjectPool` objects or their parent objects to which the Return object belongs. |
| Reference | `ReturnObject` | Another ReturnObject that specifies some `VRCObjectPool`. The objects specified here will be referenced in the same way. |
| Layer | int | The object layer number to which the Return object belongs. The default is 13, indicating the Pickup layer. |
| Audio Source | `AudioSource` | The audio source to be played when returning. If not specified, no audio will be played. |
| Audio Clip | `AudioClip` | The audio clip to be played when returning. If not specified, the audio clip specified in `Audio Source` will be played. |
| Execute Custom Event | bool | Whether to globally execute custom methods on `UdonBehaviour` for each Return object and objects specified in `External Objects`. |
| External Objects | `GameObject[]` | Objects to be targeted other than Return objects when executing custom methods during SpawnObject execution. These objects or their parent objects contain the desired custom methods to be executed.

If you want to use custom methods, please copy and use the methods in the `Custom Event Names` field.

### Reset Switch

| Variable Name | Type | Description |
|--------|---|------|
| All Reseter | `ResetObject` | A ReturnObject that references all target `VRCObjectPool` objects for clearing all objects at once. If this is exclusively used for this purpose, no Collider is needed for the ResetObject. |