using UnityEngine;

// Orb wallet and shop purchases, saved in PlayerPrefs so they survive closing the app.
// Nothing is cached in statics, so it is safe with domain reload switched off.
public static class ShopData
{
    const string OrbsKey = "Shop_Orbs";
    const string EquippedKey = "Shop_Equipped";
    const string OwnedPrefix = "Shop_Owned_";

    public static int Orbs => PlayerPrefs.GetInt(OrbsKey, 0);

    public static void AddOrbs(int amount)
    {
        PlayerPrefs.SetInt(OrbsKey, Orbs + amount);
    }

    // Free items (price 0) are always owned.
    public static bool IsOwned(string id, int price)
    {
        return price <= 0 || PlayerPrefs.GetInt(OwnedPrefix + id, 0) == 1;
    }

    public static bool TryBuy(string id, int price)
    {
        if (IsOwned(id, price) || Orbs < price)
            return false;

        PlayerPrefs.SetInt(OrbsKey, Orbs - price);
        PlayerPrefs.SetInt(OwnedPrefix + id, 1);
        PlayerPrefs.Save();
        return true;
    }

    public static string GetEquipped(string defaultId)
    {
        return PlayerPrefs.GetString(EquippedKey, defaultId);
    }

    public static void Equip(string id)
    {
        PlayerPrefs.SetString(EquippedKey, id);
        PlayerPrefs.Save();
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }
}
