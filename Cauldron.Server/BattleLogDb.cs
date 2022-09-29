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
        public SqliteConnection Connection()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BattleLogs.sqlite");
            if (!Directory.Exists(Path.GetDirectoryName(filepath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filepath));
            }

            return new SqliteConnection("Data Source=" + filepath);
        }

        public void CreateCardPoolLogTableIfNotExists(SqliteConnection con)
        {
            using var command = con.CreateCommand();

            command.CommandText = @"
create table if not exists card_pool_log(
    game_id text not null,
    card_defs_json text not null,
    created_at text not null default (datetime(CURRENT_TIMESTAMP, 'localtime'))
)";
            command.ExecuteNonQuery();
        }

        public void Add(SqliteConnection con, CardPoolLog log)
        {
            using var command = con.CreateCommand();

            command.CommandText = $@"
insert into card_pool_log(game_id, card_defs_json)
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

        public void CreateBattleLogsTableIfNotExists(SqliteConnection con)
        {
            using var command = con.CreateCommand();

            command.CommandText = @"
create table if not exists battle_logs(
    id integer primary key AUTOINCREMENT,
    game_id text not null,
    player_id text not null,
    winner_player_id text not null,
    game_event text not null,
    game_context_json text not null,
    notify_message_json text not null,
    created_at text not null default (datetime(CURRENT_TIMESTAMP, 'localtime'))
)";
            command.ExecuteNonQuery();
        }

        public void Add(SqliteConnection con, BattleLog log)
        {
            using var command = con.CreateCommand();

            command.CommandText = $@"
insert into battle_logs(game_id, player_id, winner_player_id, game_event, game_context_json, notify_message_json)
values(@game_id, @player_id, @winner_player_id, @game_event, @game_context_json, @notify_message_json)
";

            var gameContextJson = JsonConverter.Serialize(log.GameContext);

            command.Parameters.AddRange(new[]
            {
                new SqliteParameter("@game_id", log.GameId.ToString()),
                new SqliteParameter("@player_id", log.PlayerId.ToString()),
                new SqliteParameter("@winner_player_id", log.WinnerPlayerId.ToString()),
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
    player_id text,
    game_id text not null,
    name text not null,
    card_names_in_deck_json text not null,
    play_order INTEGER not null,
    ip string not null,
    created_at text not null default (datetime(CURRENT_TIMESTAMP, 'localtime')),
    primary key(player_id, game_id)
)
";
            command.ExecuteNonQuery();
        }

        public void Add(SqliteConnection con, BattlePlayer log)
        {
            using var command = con.CreateCommand();

            command.CommandText = $@"
insert into battle_players(player_id, game_id, name, card_names_in_deck_json, play_order, ip)
values(@player_id, @game_id, @name, @card_names_in_deck_json, @play_order, @ip)
";

            command.Parameters.AddRange(new[]
            {
                new SqliteParameter("@player_id", log.PlayerId.ToString()),
                new SqliteParameter("@game_id", log.GameId.ToString()),
                new SqliteParameter("@name", log.Name),
                new SqliteParameter("@card_names_in_deck_json", JsonConverter.Serialize(log.CardNamesInDeck)),
                new SqliteParameter("@play_order", log.PlayOrder),
                new SqliteParameter("@ip", log.Ip),
            });

            command.ExecuteNonQuery();
        }

        public IEnumerable<GameReplay> ListGameHistories(SqliteConnection con, IReadOnlyList<GameId> gameIdList)
        {
            var sql = @"
select
	bl1.game_id,
	(select json_group_array(bp.player_id) from battle_players as bp where bp.game_id = bl1.game_id) as player_id_list,
    bl1.id as action_log_id,
    bl1.created_at,
    cp.card_defs_json
from battle_logs bl1
inner join card_pool_log cp
	on cp.game_id = bl1.game_id
where game_event = 'OnStartGame'
and exists(
	select *
	from card_pool_log c
	where c.game_id = bl1.game_id
)
and exists(
	select *
	from battle_logs bl2
	where bl2.game_id = bl1.game_id
	and game_event = 'OnEndGame'
)
";
            var sqlParameters = new SqliteParameter[0];

            if (gameIdList.Any())
            {
                var gameIdListString = string.Join(",", gameIdList.Select((_, i) => $"@game_id_{i}"));
                sql += $" and bl1.game_id in ({gameIdListString})";

                sqlParameters = gameIdList.Select((g, i) => new SqliteParameter($"game_id_{i}", g.ToString())).ToArray();
            }

            using var command = con.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddRange(sqlParameters);

            using var reader = command.ExecuteReader();

            var result = new List<GameReplay>();

            while (reader.Read())
            {
                var playerIdList = JsonConverter.Deserialize<string[]>(reader.GetFieldValue<string>(1))
                    .Select(PlayerId.Parse)
                    .ToArray();

                var cardDefs = JsonConverter.Deserialize<CardDef[]>(reader.GetFieldValue<string>(4));

                result.Add(new GameReplay(
                    GameId.Parse(reader.GetFieldValue<string>(0)),
                    playerIdList,
                    reader.GetFieldValue<int>(2),
                    DateTime.Parse(reader.GetFieldValue<string>(3)),
                    cardDefs
                    ));
            }

            return result;
        }

        public ReplayActionLog FindNextActionLog(
            SqliteConnection con, GameId gameId, PlayerId playerId, int currentActionLogId)
        {
            var sql = @"
select
    id as action_id,
    player_id, game_event,
    game_context_json,
    notify_message_json
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
                new SqliteParameter("@current_action_log_id",currentActionLogId),
            });

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var gameContext = JsonConverter.Deserialize<GameContext>(reader.GetFieldValue<string>(3));

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
