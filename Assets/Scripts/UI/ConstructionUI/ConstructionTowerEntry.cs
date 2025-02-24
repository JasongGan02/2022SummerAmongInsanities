using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConstructionTowerEntry : MonoBehaviour
{
    [SerializeField] private Image _thumbnail;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Button _button;

    // Define colors for each state
    [SerializeField] private Color buildableColor = Color.white;
    [SerializeField] private Color lackMaterialsColor = Color.yellow;
    [SerializeField] private Color lockedColor = Color.gray;

    /// <summary>
    /// Initialize the entry with a tower's name, thumbnail and state.
    /// </summary>
    public void Initialize(string towerName, Sprite thumbnail, TowerEntryState state, UnityEngine.Events.UnityAction onClick)
    {
        _nameText.text = towerName;
        _thumbnail.sprite = thumbnail;
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(onClick);
        SetState(state);
    }

    /// <summary>
    /// Update the entry’s state: Buildable (button active), LackMaterials or Locked (button disabled).
    /// </summary>
    public void SetState(TowerEntryState state)
    {
        switch (state)
        {
            case TowerEntryState.Buildable:
                _thumbnail.color = buildableColor;
                _button.interactable = true;
                break;
            case TowerEntryState.LackMaterials:
                _thumbnail.color = lackMaterialsColor;
                _button.interactable = false;
                break;
            case TowerEntryState.Locked:
                _thumbnail.color = lockedColor;
                _button.interactable = false;
                break;
        }
    }
}

public enum TowerEntryState
{
    Buildable,
    LackMaterials,
    Locked
}