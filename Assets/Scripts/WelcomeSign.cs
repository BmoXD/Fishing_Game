using UnityEngine;

public class WelcomeSign : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        UIManager.Instance.ShowDialog("Welcome!","Welcome to Salty Phish Island! We hope you enjoy your stay. We hope you enjoy the scenery. If you want something more exctiting, we have supplied you with your very own fishing rod so you can fish. It should be inside your backpack. If you want to, you can sell the fish at the Sell-O-Matic.");
    }
}