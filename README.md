# unity-camera-and-input

A collection of camera controller and input wrapper scripts

## Camera controller
- `FollowCamera` is a component which will move camera which set to `Camera` setting to follow target which set to `Target` setting.
- `FollowCameraControls` is a component which extending from `FollowCamera` to manage rotating and zooming.

* * *

## Input wrapper scripts

### How it work?

It has `InputManager` which is wrapper for mobile's on-screen joysticks to make it able to write the same input codes for mobile games and PC (or Mac or Linux).

You will have to change input codes:
- `UnityEngine.Input.GetButton(name)` to `InputManager.GetButton(name)`
- `UnityEngine.Input.GetButtonDown(name)` to `InputManager.GetButtonDown(name)`
- `UnityEngine.Input.GetButtonUp(name)` to `InputManager.GetButtonUp(name)`
- `UnityEngine.Input.GetAxisRaw(name)` to `InputManager.GetAxis(name, true)`
- `UnityEngine.Input.GetAxis(name)` to `InputManager.GetAxis(name, false)`

### Change input settings in `InputSettingManager` component instead of Unity's input manager
You can set input settings in `InputSettingManager` by add the component to empty game object, you should add it in init scene, then set input settings as you wish.
I think it's a lot more convenient to set input settings in `InputSettingManager` component, if you don't want to use it, you still can use Unity's input manager.

### Wrapper prority
Read on-screen input -> Read input setting from `InputSettingManager` -> Read input setting from Unity's input manager.

### How to test mobile on-screen joysticks in editor?
To test mobile's on-screen joysticks while running in editor, you have to set `InputManager` -> `useMobileInputOnNonMobile` to `TRUE`

### Rewired wrapper
This project is also has [Rewired](https://guavaman.com/projects/rewired/) integration, if you have it and also want to use it, you have to add `USE_REWIRED` to scripting define symbols setting.

* * *
