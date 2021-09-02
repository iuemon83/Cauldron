namespace Cauldron.Core.Entities.Effect
{
    public record ActionContext(
        ActionAddEffectContext ActionAddEffectContext = null,
        ActionDestroyCardContext ActionDestroyCardContext = null,
        ActionMoveCardContext ActionMoveCardContext = null,
        ActionExcludeCardContext ActionExcludeCardContext = null,
        ActionModifyCardContext ActionModifyCardContext = null,
        ActionModifyCounterContext ActionModifyCounterContext = null
        );
}
