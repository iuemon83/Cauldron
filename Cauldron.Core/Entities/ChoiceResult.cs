namespace Cauldron.Shared.MessagePackObjects
{
    public record ChoiceResult(PlayerId[] PlayerIdList, Card[] CardList, CardDef[] CardDefList);
}
