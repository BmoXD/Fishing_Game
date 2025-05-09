using System;
using UnityEngine;

public static class PlayerEvents
{
    public static event Action<bool> OnFishingStateChanged;
    public static event Action<bool> OnPlayerEnterMenu;
    public static event Action<bool> OnPlayerEnterMinigame;

    public static void RaiseFishingStateChanged(bool isFishing)
    {
        OnFishingStateChanged.Invoke(isFishing);
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
}
