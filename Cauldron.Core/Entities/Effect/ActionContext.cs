namespace Cauldron.Core.Entities.Effect
{
    public record ActionContext(
        ActionAddCardContext? AddCard = null,
        ActionAddEffectContext? AddEffect = null,
        ActionDamageContext? Damage = null,
        ActionDestroyCardContext? DestroyCard = null,
        ActionDrawCardContext? DrawCard = null,
        ActionExcludeCardContext? ExcludeCard = null,
        ActionModifyCardContext? ModifyCard = null,
        ActionModifyCounterContext? ModifyCounter = null,
        ActionMoveCardContext? MoveCard = null
        );
}
