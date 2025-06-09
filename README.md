# Mozart.Encore

A cross-platform re-implementation of O2Jam game server in C#.  
This project is inspired by the _Mozart Project 0.028_.

> [!WARNING]
> This project is currently work in progress.

## Project Structure

| Project                               | Description                                 |
|---------------------------------------|---------------------------------------------|
| [Encore](Source/Mozart.Encore/Encore) | Custom TCP server framework                 |
| [Mozart](Source/Mozart.Encore/Mozart) | Game server implementation for v3.10 client |

# Configuration

The server can be configured either with `config.ini` or command-line arguments. 
See [Command-line configuration provider](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#command-line-configuration-provider) to set up command-line config.

## TCP
TCP Server connection setting.  

Use `--Server:<Option>` to configure these settings via command-line arguments (e.g, `--Server:Port=15010`)

> [!WARNING]
> Gateway &amp; Channel deployment mode is not supported yet.
> Might not be implemented even after the full release.

| Option             | Description                                                                                                  |
|--------------------|--------------------------------------------------------------------------------------------------------------|
| `Address`          | TCP address to listen incoming connection. Using `0.0.0.0` may require admin privilege. Default: `127.0.0.1` |
| `Port`             | TCP port to listen incoming connection. Default: `15010`                                                     |
| `MaxConnections`   | Maximum number of clients connecting to the server. Default: `10000`                                         |
| `PacketBufferSize` | Maximum number of bytes that can be processed by the server. Default: `4096` bytes                           |                                                          

## Database
Database connection setting.

Use `--Db:<Option>` to configure these settings via command-line arguments

| Option           | Description                                                                                                                                                                                                            |
|------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Driver`         | Specify database provider. <br/>Supported drivers:<ul><li>`Memory` (NOT Recommended)</li><li>`Sqlite` (Recommended for local server)</li><li>`SqlServer`</li><li>`MySql`</li><li>`Postgres`</li></ul>Default: `Sqlite` |
| `Name`           | Database name. Default: `O2JAM`                                                                                                                                                                                        |
| `Url`            | Database Url, also known as Connection String. Default: `Data Source=O2JAM.db`                                                                                                                                         |
| `MinBatchSize`   | Minimum number of statements that are needed for a multi-statement command sent to the database. Default: (not configured)                                                                                             |                                                          
| `MaxBatchSize`   | Maximum number of statements that are needed for a multi-statement command sent to the database. Default: (not configured)                                                                                             |                                                          
| `CommandTimeout` | The wait time (in seconds) before terminating the attempt to execute a command and generating an error. Default: (not configured)                                                                                      |                                                          

## Auth
Determine auth behavior

Use `--Auth:<Option>` to configure these settings via command-line arguments.

| Option            | Description                                                                                                                                                                                                                                                                                                                         |
|-------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Mode`            | Determine the SQL table used for authentication<br/><ul><li>`Default`: `t_o2jam_credentials`</li><li>`Foreign`: `member`</li>                                                                                                                                                                                                       |
| `SessionExpiry`   | Determine the number of minutes before the session gets deleted from `t_o2jam_login` after the connection terminated.<br/><br/>Set `0` to never expired which is recommended for single player or online server with custom session implementation.<br/>Otherwise, It is recommended to set between 2-5 minutes. Default: 5 minutes |
| `RevokeOnStartup` | Clear login tables on start-up. Recommended to disable for local server. Default: `true`                                                                                                                                                                                                                                            |

## Gateway &amp; Channels
Gateway &amp; Channels network and rating configuration. There must be at least one channel for the server to work properly.  

Use `--Gateway:<Option>` &amp; `--Gateway:Channels:<N>:<Option>` to configure these settings via command-line arguments, 
where `N` is the index of channel table (not to be confused with channel id!).  

The index (`N`) represents an array index and must always start from 0, ordered and with no gap in-between. The `Id` however, can be un-ordered and with gaps in-between.

| Option                          | Description                              |
|---------------------------------|------------------------------------------|
| `Gateway:Channels:<N>:Id`       | The channel id (required)                |
| `Gateway:Channels:<N>:Capacity` | Channel maximum capacity. Default: `100` |
| `Gateway:Channels:<N>:Gem`      | GEM reward rate. Default: `1.0`          |
| `Gateway:Channels:<N>:Capacity` | EXP reward rate. Default: `1.0`          |

## Metadata
Metadata files that act as source of truth of particular game data outside the database. The metadata files are not optional.

Use `--Metadata:<Option>` to configure these settings via command-line arguments.

> [!WARNING]
> Metadata override per channel is not supported yet.

| Option             | Description                                                                              |
|--------------------|------------------------------------------------------------------------------------------|
| `ItemData`         | Relative or absolute path of `Itemdata.dat`. Format must compatible with v`3.10` client. |

## Game settings
Gameplay-specific settings.

Use `--Game:<Option>` to configure these settings via command-line arguments.

| Option                       | Description                                                                       |
|------------------------------|-----------------------------------------------------------------------------------|
| `AllowSoloInVersus`          | Specify whether playing solo is eligible in VS Mode. Default: `true`              |
| `SingleModeRewardLevelLimit` | Specify maximum level limit of gaining reward in Single mode. Default: Level `10` |

# Scaling

> [!WARNING]
> This feature is not implemented yet
> It is very likely that it will be scrapped after the full release.

# CLI Command

The server application has built-in utilities shaped toward local player usage. 

- `db:init`: Initialize a new database if the configured database is not exists.
- `user:register`: Register a new user.
- `user:authorize`: Authorize user credential. Display both decoded and encoded auth token that can be used to launch the game.

Run the CLI with `--help` flag for more details.