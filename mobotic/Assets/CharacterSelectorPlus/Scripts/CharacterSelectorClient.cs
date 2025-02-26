using UnityEngine;

public class CharacterSelectorClient : MonoBehaviour {

    private void OnEnable()
    {
        CharacterSelector.OnCharacterSelected += CharacterSelected;
        CharacterSelector.OnPurchaseCharacter += PurchaseCharacter;
    }

    private void OnDisable()
    {
        CharacterSelector.OnCharacterSelected -= CharacterSelected;
        CharacterSelector.OnPurchaseCharacter -= PurchaseCharacter;
    }

    private void CharacterSelected(string characterName)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(child.gameObject.name == characterName);
        }
    }

    private void PurchaseCharacter(CharacterProperty characterProperty)
    {
        Debug.Log("You must pay " + characterProperty.name + ", for the character: " + characterProperty.nameObj);
        GameObject.Find("CharacterSelectorController").GetComponent<CharacterSelector>().paymentConfirmed(characterProperty);
    }
}
