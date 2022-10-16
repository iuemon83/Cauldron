using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects;
using MagicOnion;
using System.Threading.Tasks;

namespace Cauldron.Shared.Services
{
    public interface ICauldronHub : IStreamingHub<ICauldronHub, ICauldronHubReceiver>
    {
        Task<CardDef[]> GetCardPool();

        Task<RuleBook> GetRuleBook();

        Task<ListAllowedClientVersionsReply> ListAllowedClientVersions();

        Task<RoomOutline[]> ListOpenGames();

        Task<GameReplay[]> ListGameHistories(ListGameHistoriesRequest request);

        Task<CardDef[]> GetCardPoolByGame(GameId gameId);

        Task<int> FirstActionLog(GameId gameId);
        Task<int> NextActionLog(GameId gameId, PlayerId playerId, int currentActionLogId);

        Task<OpenNewRoomReply> OpenNewRoom(OpenNewRoomRequest request);

        Task<JoinRoomReply> JoinRoom(JoinRoomRequest request);

        Task<bool> LeaveRoom();

        Task ReadyGame(ReadyGameRequest request);

        Task<StartTurnReply> StartTurn(StartTurnRequest request);

        Task<EndTurnReply> EndTurn(EndTurnRequest request);

        Task<GameMasterStatusCode> Surrender(GameId gameId);

        Task<PlayFromHandReply> PlayFromHand(PlayFromHandRequest request);

        Task<AttackToCreatureReply> AttackToCreature(AttackToCreatureRequest request);

        Task<AttackToPlayerReply> AttackToPlayer(AttackToPlayerRequest request);
    }
}