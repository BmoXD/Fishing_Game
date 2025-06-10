using System;
using UnityEngine;

public static class PlayerEvents
{
    public static event Action<bool> OnFreezePlayer;
    public static event Action<bool> OnPlayerEnterMenu;
    public static event Action<bool> OnPlayerEnterMinigame;
    public static event Action<bool> OnDialogBoxStateChanged;

    public static void RaisePlayerFreeze(bool isFishing)
    {
        OnFreezePlayer.Invoke(isFishing);
        Debug.Log("RaiseFishingStateChanged: "+isFishing);
    }

    public static void RaisePlayerEnterMenu(bool isInMenu)
    {
        OnPlayerEnterMenu.Invoke(isInMenu);
        Debug.Log("RaisePlayerEnterMenu: "+isInMenu);
    }

    public static void RaisePlayerEnterMinigame(bool isInMinigame)
    {
        OnPlayerEnterMinigame.Invoke(isInMinigame);
        Debug.Log("RaisePlayerEnterMinigame: "+isInMinigame);
    }

    public static void RaiseDialogBoxStateChanged(bool isOpen)
    {
        OnDialogBoxStateChanged?.Invoke(isOpen);
        Debug.Log("RaiseDialogBoxStateChanged: " + isOpen);
    }
}
