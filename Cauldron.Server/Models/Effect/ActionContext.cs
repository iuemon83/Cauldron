namespace Cauldron.Server.Models.Effect
{
    public record ActionContext(
        ActionAddEffectContext ActionAddEffectContext = null,
        ActionDestroyCardContext ActionDestroyCardContext = null,
        ActionMoveCardContext ActionMoveCardContext = null
        );
}
