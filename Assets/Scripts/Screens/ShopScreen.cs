using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Minimal shop for the prototype: spend adventure gold on undo tokens for the night's swipes.
// Real items (weapons, hats, gifts) come later from BalanceConfig.ShopItems
public class ShopScreen : ScreenBase
{
    [SerializeField] private int _undoTokenPrice = 50;
    [SerializeField] private TMP_Text _goldText;
    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private Button _buyUndoButton;

    private void Awake()
    {
        _buyUndoButton.onClick.AddListener(BuyUndoToken);
        _buyUndoButton.GetComponentInChildren<TMP_Text>().text = "Undo token (" + _undoTokenPrice + " gold)";
    }

    private void OnEnable()
    {
        _messageText.text = "";
        Refresh();
    }

    private void BuyUndoToken()
    {
        if (GameManager.Instance.TrySpendGold(_undoTokenPrice))
        {
            GameManager.Instance.AddUndoToken();
            _messageText.text = "Bought an undo token. At the Inn, use it to bring back a hero you passed on.";
        }

        Refresh();
    }

    private void Refresh()
    {
        GameManager game = GameManager.Instance;
        _goldText.text = "Gold: " + game.Gold + "    Undo tokens: " + game.UndoTokens;
        _buyUndoButton.interactable = game.Gold >= _undoTokenPrice;
    }
}