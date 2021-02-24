using Cauldron.Shared.MessagePackObjects;
using MagicOnion;
using System.Threading.Tasks;

namespace Cauldron.Shared.Services
{
    public interface ICauldronHub : IStreamingHub<ICauldronHub, ICauldronHubReceiver>
    {
        Task<int> Test(int num);

        Task<OpenNewGameReply> OpenNewGame(OpenNewGameRequest request);

        Task<GetCardPoolReply> GetCardPool(GetCardPoolRequest request);

        Task<CloseGameReply> CloseGame(CloseGameRequest request);

        Task<SetDeckReply> SetDeck(SetDeckRequest request);

        Task<EnterGameReply> EnterGame(EnterGameRequest request);

        Task ReadyGame(ReadyGameRequest request);

        Task<StartTurnReply> StartTurn(StartTurnRequest request);

        Task<EndTurnReply> EndTurn(EndTurnRequest request);

        Task<PlayFromHandReply> PlayFromHand(PlayFromHandRequest request);

        Task<AttackToCreatureReply> AttackToCreature(AttackToCreatureRequest request);

        Task<AttackToPlayerReply> AttackToPlayer(AttackToPlayerRequest request);

        Task<(GameMasterStatusCode, CardId[])> ListPlayableCardId(GameId gameId);

        Task<(GameMasterStatusCode, (PlayerId[], CardId[]))> ListAttackTargets(GameId gameId, CardId cardId);
    }
}