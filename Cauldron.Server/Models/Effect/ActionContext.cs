namespace Cauldron.Server.Models.Effect
{
    public record ActionContext(
        ActionDestroyCardContext ActionDestroyCardContext = null,
        ActionMoveCardContext ActionMoveCardContext = null
        );
}
