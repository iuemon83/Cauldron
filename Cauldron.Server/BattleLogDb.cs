using Cauldron.Core.Entities;
using Cauldron.Shared.MessagePackObjects;
using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace Cauldron.Server
{
    public class BattleLogDb
    {
        public SqliteConnection Connection()
        {
            var filepath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BattleLogs.sqlite");
            return new SqliteConnection("Data Source=" + filepath);
        }

        public void CreateBattleLogsTableIfNotExists(SqliteConnection con)
        {
            using var command = con.CreateCommand();

            command.CommandText = @"
create table if not exists battle_logs(
    id integer primary key AUTOINCREMENT,
    game_id text not null,
    winner_player_id text not null,
    game_event text not null,
    created_at text not null default (datetime(CURRENT_TIMESTAMP, 'localtime'))
)";
            command.ExecuteNonQuery();
        }

        public void Add(SqliteConnection con, BattleLog log)
        {
            using var command = con.CreateCommand();

            command.CommandText = $@"
insert into battle_logs(game_id, winner_player_id, game_event)
values(@game_id, @winner_player_id, @game_event)
";

            command.Parameters.AddRange(new[]
            {
                new SqliteParameter("@game_id", log.GameId.ToString()),
                new SqliteParameter("@winner_player_id", log.WinnerPlayerId.ToString()),
                new SqliteParameter("@game_event", log.GameEvent.ToString()),
            });

            command.ExecuteNonQuery();
        }
        public void CreateBattlePlayersTableIfNotExists(SqliteConnection con)
        {
            using var command = con.CreateCommand();

            command.CommandText = @"
create table if not exists battle_players(
    player_id text primary key,
    game_id text not null,
    name text not null,
    card_names_in_deck_json text not null,
    play_order INTEGER not null,
    ip string not null,
    created_at text not null default (datetime(CURRENT_TIMESTAMP, 'localtime'))
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
    }

    public record BattleLog(
        GameId GameId,
        PlayerId WinnerPlayerId,
        GameEvent GameEvent
        );

    public record BattlePlayer(
        GameId GameId,
        PlayerId PlayerId,
        string Name,
        string[] CardNamesInDeck,
        int PlayOrder,
        string Ip
        );
}
