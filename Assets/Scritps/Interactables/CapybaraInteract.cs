using UnityEngine;

public class CapybaraInteract: MonoBehaviour, IInformable
{
    [SerializeField] private AudioManager audioManager;

    public string GetDescription() {
        return $"Hello there! It seems you got lost here while delivering those packages.\nThere's some weird zombie outbreak happening right now, and they are coming for you!\nHowever, I can help you survive. You can use your dropped packages by picking them with 'E' as a permanent upgrade.\nBut be warned: once you pick one of your packages, zombies will emerge, and you will have to fight on your own!\nSince everyone died here, you can barge into houses and camps to look out for supplies that can help you during the zombie attack.\nYou can also try sliding by pressing the 'CTRL' button, which can help you dodge enemy attacks or even hurt zombies by crashing into them!\nIf zombies are faster than you, you can try bunny-hopping away from them, which will give you some speed.\nBefore I leave, here's the gun I found. It's not that much, but it's better than nothing.\nYou can still upgrade it though. Also, I've heard that there's a hidden stash somewhere that opens randomly!\nGood luck.";
    }

    public void Interact() {
    }
}
