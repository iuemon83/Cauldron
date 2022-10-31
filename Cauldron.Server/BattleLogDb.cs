using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Core;
using Cauldron.Shared.MessagePackObjects;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cauldron.Server
{
    public class BattleLogDb
    {
        public static string UnknownPlayerName(int order) => $"Player{order}";

        public SqliteConnection Connection()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BattleLogs.sqlite");
            if (!Directory.Exists(Path.GetDirectoryName(filepath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filepath));
            }

            return new SqliteConnection("Data Source=" + filepath);
        }

        public void CreateGamesTableIfNotExists(SqliteConnection con)
        {
            using var command = con.CreateCommand();

            command.CommandText = @"
create table if not exists games(
    game_id text not null primary key,
    card_defs_json text not null,
    winner_player_id text,
    created_at text not null default (datetime(CURRENT_TIMESTAMP, 'localtime'))
)
";
            command.ExecuteNonQuery();
        }

        public void Add(SqliteConnection con, GameLog log)
        {
            using var command = con.CreateCommand();

            command.CommandText = $@"
insert into games(game_id, card_defs_json)
values(@game_id, @card_defs_json)
";

            var cardDefsJson = JsonConverter.Serialize(log.CardDefsJson);

            command.Parameters.AddRange(new[]
            {
                new SqliteParameter("@game_id", log.GameId.ToString()),
                new SqliteParameter("@card_defs_json", cardDefsJson),
            });

            command.ExecuteNonQuery();
        }

        public void SetGameWinner(SqliteConnection con, GameId gameId, PlayerId winnerPlayerId)
        {
            using var command = con.CreateCommand();

            command.CommandText = $@"
update games
set winner_player_id = @winner_player_id
where game_id = @game_id
";

            command.Parameters.AddRange(new[]
            {
                new SqliteParameter("@game_id", gameId.ToString()),
                new SqliteParameter("@winner_player_id", winnerPlayerId.ToString()),
            });

            command.ExecuteNonQuery();
        }

        public void CreateBattleLogsTableIfNotExists(SqliteConnection con)
        {
            using var command = con.CreateCommand();

            command.CommandText = @"
create table if not exists battle_logs(
    id integer primary key AUTOINCREMENT,
    game_id text not null,
    player_id text not null,
    game_event text not null,
    game_context_json text not null,
    notify_message_json text not null,
    created_at text not null default (datetime(CURRENT_TIMESTAMP, 'localtime'))
)
";
            command.ExecuteNonQuery();

            command.CommandText = @"
create index battle_logs_game_id on battle_logs(game_id);
create index battle_logs_player_id on battle_logs(player_id);
";
            command.ExecuteNonQuery();
        }

        public void Add(SqliteConnection con, BattleLog log)
        {
            using var command = con.CreateCommand();

            command.CommandText = $@"
insert into battle_logs(game_id, player_id, game_event, game_context_json, notify_message_json)
values(@game_id, @player_id, @game_event, @game_context_json, @notify_message_json)
";

            var gameContextJson = JsonConverter.Serialize(log.GameContext);

            command.Parameters.AddRange(new[]
            {
                new SqliteParameter("@game_id", log.GameId.ToString()),
                new SqliteParameter("@player_id", log.PlayerId.ToString()),
                new SqliteParameter("@game_event", log.NotifyEvent.ToString()),
                new SqliteParameter("@game_context_json", gameContextJson),
                new SqliteParameter("@notify_message_json", log.MessageJson.ToString()),
            });

            command.ExecuteNonQuery();
        }

        public void CreateBattlePlayersTableIfNotExists(SqliteConnection con)
        {
            using var command = con.CreateCommand();

            command.CommandText = @"
create table if not exists battle_players(
    player_id text not null,
    game_id text not null,
    name text not null,
    card_names_in_deck_json text not null,
    play_order INTEGER not null,
    ip text not null,
    client_id text not null,
    created_at text not null default (datetime(CURRENT_TIMESTAMP, 'localtime')),
    primary key(player_id, game_id)
)
";
            command.ExecuteNonQuery();

            command.CommandText = @"
create index battle_players_player_id on battle_players(player_id);
create index battle_players_game_id on battle_players(game_id);
create index battle_players_client_id on battle_players(client_id);
";
            command.ExecuteNonQuery();
        }

        public void Add(SqliteConnection con, BattlePlayer log)
        {
            using var command = con.CreateCommand();

            command.CommandText = $@"
insert into battle_players(player_id, game_id, name, card_names_in_deck_json, play_order, ip, client_id)
values(@player_id, @game_id, @name, @card_names_in_deck_json, @play_order, @ip, @client_id)
";

            command.Parameters.AddRange(new[]
            {
                new SqliteParameter("@player_id", log.PlayerId.ToString()),
                new SqliteParameter("@game_id", log.GameId.ToString()),
                new SqliteParameter("@name", log.Name),
                new SqliteParameter("@card_names_in_deck_json", JsonConverter.Serialize(log.CardNamesInDeck)),
                new SqliteParameter("@play_order", log.PlayOrder),
                new SqliteParameter("@ip", log.Ip),
                new SqliteParameter("@client_id", log.ClientId),
            });

            command.ExecuteNonQuery();
        }

        public IEnumerable<GameReplay> ListGameHistories(
            SqliteConnection con,
            IReadOnlyList<GameId> gameIdList,
            string clientId,
            bool onlyMyLogs
            )
        {
            var sql = $@"
select
	g.game_id,
	(
        select json_group_array(
            json_object(
                'id', json_object('value', bp.player_id),
                'name', bp.name
                )
            )
        from battle_players as bp
        where bp.game_id = g.game_id
        order by play_order asc
    ) as player_id_list,
	(
		select bl.id
		from battle_logs bl
		where bl.game_id = g.game_id
		order by bl.id
		limit 1
	) as action_log_id,
    g.created_at,
	exists(
		select *
		from battle_players bp
		where bp.game_id = g.game_id
		and client_id = @client_id
	) as is_mine
from games g
where g.winner_player_id is not NULL
";
            var sqlParameters = new List<SqliteParameter>
            {
                new SqliteParameter("client_id", clientId)
            };

            if (gameIdList.Any())
            {
                var gameIdListString = string.Join(",", gameIdList.Select((_, i) => $"@game_id_{i}"));
                sql += $" and g.game_id in ({gameIdListString})";

                sqlParameters.AddRange(
                    gameIdList.Select((g, i) => new SqliteParameter($"game_id_{i}", g.ToString()))
                    );
            }

            if (onlyMyLogs)
            {
                sql += $@"
and exists(
    select *
    from battle_players bp
    where bp.game_id = g.game_id
    and client_id = @client_id
)
";
            }

            using var command = con.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParameters);

            using var reader = command.ExecuteReader();

            var result = new List<GameReplay>();

            while (reader.Read())
            {
                var players = JsonConverter.Deserialize<GameReplayPlayer[]>(reader.GetFieldValue<string>(1));
                var isMine = reader.GetFieldValue<bool>(4);
                if (!isMine)
                {
                    players = players
                        .Select((p, i) => new GameReplayPlayer(p.Id, UnknownPlayerName(i + 1)))
                        .ToArray();
                }

                result.Add(new GameReplay(
                    GameId.Parse(reader.GetFieldValue<string>(0)),
                    players,
                    reader.GetFieldValue<int>(2),
                    DateTime.Parse(reader.GetFieldValue<string>(3))
                    ));
            }

            return result;
        }

        public CardDef[] FindCardPool(SqliteConnection con, GameId gameId)
        {
            var sql = $@"
select card_defs_json
from games g
where game_id = @game_id
";
            var sqlParameters = new List<SqliteParameter>
            {
                new SqliteParameter("@game_id", gameId.ToString()),
            };

            using var command = con.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParameters);

            using var reader = command.ExecuteReader();

            var result = new List<GameReplay>();

            while (reader.Read())
            {
                return JsonConverter.Deserialize<CardDef[]>(reader.GetFieldValue<string>(0));
            }

            return Array.Empty<CardDef>();
        }

        public ReplayActionLog FindNextActionLog(
            SqliteConnection con, GameId gameId, PlayerId playerId, int currentActionLogId,
            string clientId
            )
        {
            var sql = @"
select
    id as action_id,
    player_id, game_event,
    game_context_json,
    notify_message_json,
    exists(
        select *
        from battle_players bp
        where bp.game_id = b1.game_id
        and client_id = @client_id
    ) is_mine
from battle_logs b1
where game_id = @game_id
and player_id = @player_id
and id = (
	select min(id)
	from battle_logs b2
	where b2.game_id = b1.game_id
    and b2.player_id = @player_id
	and b2.id > @current_action_log_id
)
limit 1
";

            using var command = con.CreateCommand();
            command.CommandText = sql;

            command.Parameters.AddRange(new[]
            {
                new SqliteParameter("@game_id", gameId.ToString()),
                new SqliteParameter("@player_id", playerId.ToString()),
                new SqliteParameter("@current_action_log_id", currentActionLogId),
                new SqliteParameter("@client_id", clientId),
            });

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var gameContext = JsonConverter.Deserialize<GameContext>(reader.GetFieldValue<string>(3));
                var isMine = reader.GetFieldValue<bool>(5);
                if (!isMine)
                {
                    gameContext.You.PublicPlayerInfo.SetReplayAliasName(
                        gameContext.You.PublicPlayerInfo.IsFirst ? UnknownPlayerName(1) : UnknownPlayerName(2));

                    gameContext.Opponent.SetReplayAliasName(
                        gameContext.Opponent.IsFirst ? UnknownPlayerName(1) : UnknownPlayerName(2));
                }

                return new ReplayActionLog(
                    reader.GetFieldValue<int>(0),
                    PlayerId.Parse(reader.GetFieldValue<string>(1)),
                    Enum.Parse<NotifyEvent>(reader.GetFieldValue<string>(2)),
                    gameContext,
                    reader.GetFieldValue<string>(4)
                    );
            }

            return default;
        }
    }
}
