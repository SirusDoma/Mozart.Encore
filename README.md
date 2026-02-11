# Amadeus.Encore

A cross-platform re-implementation of O2Jam game server in C#.  
This project is inspired by the _Mozart Project 0.028_.

Supported client version: **v3.82\* (NX)**  
<sub>* v3.73 and older might work but not fully tested.</sub>

## Features

- Zero-Configuration for quick start
- Full online and local network multiplayer support
- Complete packet op-code coverage
- Compatible with multiple SQL RDBMS
- Support multi planet and channels deployment
- Highly customizable with high-level network protocol implementation

<sub>* In-game web server features are not included.</sub>

## Project Structure

| Project                                        | Description                                       |
|------------------------------------------------|---------------------------------------------------|
| [Encore](Source/Encore/)                       | Custom TCP server framework                       |
| [Mozart.Server](Source/Mozart.Server/)         | Core O2Jam server implementation                  |
| [Mozart.Data](Source/Mozart.Data/)             | Data persistent implementation                    |
| [Mozart.Migrations](Source/Mozart.Migrations/) | Database migrations with various drivers          |
| [Amadeus.Web](Source/Amadeus.Web/)             | Lightweight HTTP Web server                       |
| [Amadeus](Source/Amadeus/)                     | Game server implementation for O2Jam client v3.82 |

# Configuration

The server can be configured either with `config.ini` or command-line arguments. 
See [Command-line configuration provider](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#command-line-configuration-provider) to set up command-line config.

## Server
Server deployment mode and TCP connection setting.  

Use `--Server:<Option>` to configure these settings via command-line arguments (e.g, `--Server:Port=15010`)

| Option             | Description                                                                                                                                                                                                                                                                                                                                                                                              |
|--------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Mode`             | Server deployment mode. Supported values: <ul><li>`Full`<br/>Act as a full-package server: One gateway with one or more channels.</li><br/><li>`Gateway`<br/>Handle authentication and end-user TCP connections, relaying them to the `Channel` servers.</li><br/><li>`Channel`<br/>Manage users persistent and non-persistent in-game states/logic of one particular channel.</li></ul> Default: `Full` |
| `Address`          | <ul><li>In `Full`/`Gateway` mode:<br/>TCP address to listen incoming connection. Using `0.0.0.0` may require admin privilege.</li><br/><li>In `Channel` mode:<br/>The local endpoint address for the TCP client.</li></ul> Default: `127.0.0.1`                                                                                                                                                          |
| `Port`             | <ul><li>In `Full`/`Gateway` mode:<br/>TCP port to listen incoming connection.</li><br/><li>In `Channel` mode:<br/>The local endpoint address for the TCP client.</li></ul> Default: `15010`                                                                                                                                                                                                              |
| `MaxConnections`   | The maximum number of clients connecting to the server. Default: `10000`                                                                                                                                                                                                                                                                                                                                 |
| `PacketBufferSize` | The maximum number of bytes per [message frame](https://blog.stephencleary.com/2009/04/message-framing.html) that can be processed by the server. Default: `4096` bytes                                                                                                                                                                                                                                  |                                                          

## HTTP
Lightweight HTTP web server settings.

| Option    | Description                                                                                                       |
|-----------|-------------------------------------------------------------------------------------------------------------------|
| `Enabled` | Determine whether the web server is enabled.  Default: `false`                                                    |
| `Address` | Web server address to listen incoming requests. Using `0.0.0.0` may require admin privilege. Default: `127.0.0.1` |
| `Port`    | HTTP port to listen incoming requests. Default: `15010`                                                           |

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

| Option            | Description                                                                                                                                                                                                                                                                                                                                |
|-------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Mode`            | Determine the SQL table used for authentication<br/><ul><li>`Default`: `t_o2jam_credentials`</li><li>`Foreign`: `member`</li></ul>`9you` and `GAMANIA` are aliases for `Foreign`.                                                                                                                                                          |
| `SessionExpiry`   | Determine the number of minutes before the session gets deleted from `t_o2jam_login` after the connection terminated.<br/><br/>Set `0` to never expired which is recommended for single player or online server with custom session implementation. Otherwise, It is recommended to set between 2-5 minutes.<br/><br/>Default: `5` minutes |
| `RevokeOnStartup` | Clear login tables on start-up. Recommended to disable for local server. Default: `true`                                                                                                                                                                                                                                                   |

## Gateway &amp; Channels
Gateway &amp; Channels network and economy rating configuration. There must be at least one channel for the server to work properly.  

Use `--Gateway:<Option>` &amp; `--Gateway:Channels:<N>:<Option>` to configure these settings via command-line arguments (e.g, `--Gateway:Channels:0:Id=0`). 
`<N>` is the index of channel table (not to be confused with channel id!).  

The index (`<N>`) represents an array index and must always start from 0, ordered and with no gap in-between. The `Id` however, can be un-ordered and with gaps in-between.

### Gateway
These options can be configured under `Gateway` section.

> [!TIP]
> This configuration is ignored in the `Full` deployment mode.

| Option    | Description                                                                                                                                                                                                                                                             |
|-----------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Address` | <ul><li>In `Gateway` mode:<br/>Inbound TCP address to listen incoming channel server connection. Using `0.0.0.0` may require admin privilege.</li><br/><li>In `Channel` mode:<br/>Outbound TCP address to connect to the Gateway server.</li></ul> Default: `127.0.0.1` |
| `Port`    | <ul><li>In `Gateway` mode:<br/>Inbound TCP port to listen incoming channel server connection.</li><br/><li>In `Channel` mode:<br/>Outbound TCP port to connect to the Gateway server.</li></ul> Default: `15047`                                                        |
| `Timeout` | The maximum wait time (in seconds) for establishing connection between the gateway and the channel. Default: `30` seconds                                                                                                                                               |

### Channels
These options can be configured under `Gateway:Channels:<N>` section as explained above. 

> [!TIP]
> This configuration is ignored in the `Gateway` deployment mode.

> [!IMPORTANT]
> You can only have exactly one channel in the `Channel` deployment mode.

| Option      | Description                                                                                                                                                   |
|-------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `Id`        | The channel id (required)                                                                                                                                     |
| `Capacity`  | Channel maximum capacity. Default: `100`                                                                                                                      |
| `Gem`       | GEM reward rate. Default: `1.0`                                                                                                                               |
| `Exp`       | EXP reward rate. Default: `1.0`                                                                                                                               |
| `FreeMusic` | Unlock all premium music based on the provided `MusicList` exclusive for this channel. Default: Default: (Empty) using global [Game settings](#Game-settings) |
| `MusicList` | Path of `OJNList.dat` exclusive for this channel. Format must compatible with client v`3.82`. Default: (Empty) using global [Metadata](#Metadata)             |
| `AlbumList` | Path of `AlbumList.ojs` exclusive for this channel. Format must compatible with client v`3.82`. Default: (Empty) using global [Metadata](#Metadata)           |
| `ItemData`  | Path of `Itemdata.dat` exclusive for this channel. Format must compatible with client v`3.82`. Default: (Empty) using global [Metadata](#Metadata)            |

## Metadata
Metadata files that act as source of truth of particular game data outside the database. 
The metadata files are not optional and can be usually overriden per channel.

Use `--Metadata:<Option>` to configure these settings via command-line arguments.

| Option      | Description                                                                               |
|-------------|-------------------------------------------------------------------------------------------|
| `MusicList` | Relative or absolute path of `OJNList.dat`. Format must compatible with client v`3.82`.   |
| `AlbumList` | Relative or absolute path of `AlbumList.ojs`. Format must compatible with client v`3.82`. |
| `ItemData`  | Relative or absolute path of `Itemdata.dat`. Format must compatible with client v`3.82`.  |

## Game settings
Gameplay-specific settings.

Use `--Game:<Option>` to configure these settings via command-line arguments.

| Option                       | Description                                                                                                                                                                                                                                                                                                  |
|------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `AllowSoloInVersus`          | Specify whether playing solo is eligible in VS Mode. Default: `true`                                                                                                                                                                                                                                         |
| `SingleModeRewardLevelLimit` | The maximum level limit of gaining reward in Single mode. Default: Level `10`                                                                                                                                                                                                                                |
| `FreeMusic`                  | Unlock all premium music based on the provided `MusicList`. Default: `true`                                                                                                                                                                                                                                  |
| `MusicLoadTimeout`           | The maximum wait time (in seconds) before terminating unresponsive client sessions when loading the game music.<br/><br/>Note: when one or more clients are timed out, the remaining clients will still likely stuck for a certain amount of time regardless of this setting.<br/><br/>Default: `60` seconds |

# Database Migration

Use Entity Framework tools to run the database migration.
See [Entity Framework Core CLI tools](https://learn.microsoft.com/en-us/ef/core/cli/) to learn more about the CLI installation.

>[!IMPORTANT]
> You may notice that the database schema look funky with premature normalizations here and there.  
> This is intentional because the app need to support the existing official database schema.
> 
> The table structure represents a best-effort attempt to follow the e-Games database distribution.
> Structures that are known exclusive to the foreign database distribution are omitted.
>
> However, unlike official server app, Mozart will **not** interact with database via Stored Procedure and will execute DML directly.

>[!CAUTION]
> A breaking change was introduced to the database schema and its migrations starting with Mozart v1.10.0.
> Manual adjustments to existing database schemas may be required when upgrading from Mozart v1.8.0.
> 
> Foreign database schema remain supported with proper `Auth:Mode` configuration.

## Add Migration

The migration files are divided by [projects based on provider](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/providers?tabs=dotnet-core-cli).
Use the following command to create a new migration:

```shell
 # Replace "MySql" with your preferred database driver
 dotnet ef migrations add --project Source\Mozart.Migrations\MySql\Mozart.Migrations.MySql.csproj \
                             --startup-project Source\Amadeus\Amadeus.csproj \
                             --context Mozart.Data.Contexts.MainDbContext \
                             <migration name>
                             -- --Auth:Mode=<auth mode> \
                             --Db:Driver=<driver> \
                             --Db:Url="<connection string>"
```

>[!IMPORTANT]
> Database migration is automatically executed every start-up as long as the `Auth:Mode` equals to `Default`.  
> This is because `Auth:Mode=Foreign` is a compatibility mode that enables Mozart to continue to work with an existing foreign database that has different auth schema than the original e-Games clients (such as 9you or GAMANIA).
>  
> Database migration will never be officially supported in `Foreign` mode<sup>*</sup>.
> 
> <sub>* The server will likely raise an exception with [`PendingModelChangesWarning`](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes#exception-is-thrown-when-applying-migrations-if-there-are-pending-model-changes) when running database migration with `Foreign` mode.
> The errors can be suppressed, but there's no guarantee that migration will continue to work using foreign auth schema for the future releases.</sub>

>[!TIP]
> The `--` token directs `dotnet ef` to treat everything that follows as an argument and not try to parse them as options.
> Any extra arguments not used by dotnet ef are forwarded to the Mozart.

>[!TIP]
> You can place the configured `config.ini` in your working directory to configure the database configuration
> instead of passing them via CLI. 

## Execute Migration
Run the following command to execute the migration:

```shell
 dotnet ef database update --project Source\Mozart.Migrations\MySql\Mozart.Migrations.MySql.csproj \
                           --startup-project Source\Amadeus\Amadeus.csproj \
                           --context Mozart.Data.Contexts.MainDbContext \
                           -- --Auth:Mode=<auth mode> \
                           --Db:Driver=<driver> \
                           --Db:Url="<connection string>"
```

# Web Server

By default, the server exposes user registration and login APIs used to generate the authentication tokens required to run the game.

The original web server files (ASP Classic) are not included and cannot be hosted within this project.
This functionality considered out-of-scope, and unlikely to be added in the future.

> [!WARNING]
> Official e-Games clients does not allow custom web server url by default.
> The game simply does not respect the web server address in the launch argument.
>
> Therefore, even if the original web server is ported into this Web Server module,
> enabling in-game shop functionality still requires either modifying the game client or configuring client-side host settings.

# Scaling

> [!WARNING]
> Scaling is a feature that **99% users won’t ever need**.  
> 
> It’s intended for niche scenarios—such as replicating the original server’s scaling infrastructure—or for deployments across constrained hardware 
> (e.g., deploying servers into multiple microcontrollers).

Like many traditional MMOs, O2Jam shards its network traffic across multiple servers known as `Planet`s, each of which hosts several `Channel`s.
To support this design, you must run Mozart.Encore in separate instances:

- **Gateway**
  - One instance per `Planet`
  - Listens for all incoming end-user client connections
  - Keeps track of its Planet’s Channel instances

- **Channel**
  - One instance per `Channel`
  - Handles persistent and non-persistent in-game states for its assigned Channel

There can only be one "node" of `Gateway` or `Channel` instance at a time, and it cannot be horizontally scaled. 
You cannot run multiple instances to represent a single `Gateway` or `Channel`, because each instance is the scaling unit of the horizontal scaling itself.

See [Server](#Server) and [Gateway &amp; Channels](#gateway--channels) configuration section above to configure the `Gateway` and `Channel` instances.

## Service Discovery

### Gateway

Clients specify all available Gateways when launching O2Jam via `OTwo.exe`. The syntax is:

```shell
OTwo.exe <token> <ftp_server> O2Jam <gateway_count> \
  <gateway_address_1> <gateway_port_1> \
  <gateway_address_2> <gateway_port_2> \
  … \
  <gateway_address_n> <gateway_port_n>
```

For example, if you have three Planets (three Gateways), you might use:

```shell 
OTwo.exe myEncodedBase64Token my-ftp-server:1234 O2Jam 3 \
192.168.10.1 15010 \
192.168.10.2 15010 \
192.168.10.3 15010
```

> [!TIP]
> You may mirror one gateway instance for multiple planets by reusing the same IP and port multiple times.
> For example:
> 
> ```shell 
> OTwo.exe myEncodedBase64Token my-ftp-server:1234 O2Jam 3 \
> 192.168.10.1 15010 \
> 192.168.10.1 15010 \
> 192.168.10.1 15010
> ```

### Channel

Upon start-up, the `Channel` instances will register themselves to the configured `Gateway` instance via TCP network.
Therefore, the `Gateway` need to be available first. This will allow the `Gateway` instances to discover `Channel`s that available for user to select.

When the `Channel` lost its connection to its `Gateway`, it will automatically shut down itself.

## Advanced scaling

It might be possible to host and scale `Mozart.Encore` in kubernetes via [agones](https://agones.dev/). However, it may require code changes.
Please refer to their [documentation](https://agones.dev/site/docs/) and [third-party examples](https://agones.dev/site/docs/third-party-content/examples/) to learn more. 

# CLI Command

The server application has built-in utilities to help local player usage or server maintenance. 

- `db:migrate`: Execute database migration within the configured database.
- `user:register`: Register a new user.
- `user:authorize`: Authorize user credential. Display both decoded and encoded auth token that can be used to launch the game.
- `ranking:upsert`: Generates or updates user rankings. This command is intended to be executed periodically using a scheduled cron job.

Run the CLI with `--help` flag for more details.
