using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One colour card in the shop (Assets/prefabs/ShopCard). MenuManager fills it in;
// the card's button calls Press from its OnClick() list in the Inspector.
public class ShopCard : MonoBehaviour
{
    public Image swatch;
    public TextMeshProUGUI nameText;
    public Button button;
    public TextMeshProUGUI buttonLabel;

    [HideInInspector] public MenuManager menu;
    [HideInInspector] public int colourIndex;

    public void Press()
    {
        menu.BuyOrEquip(colourIndex);
    }
}
