# Mozart.Encore

A cross-platform O2Jam game server re-implementation in C#.  
This project is inspired by the _Mozart Project 0.028_.

## Server Builds

> [!IMPORTANT]
> See a build's README below for setup requirements, compatibility notes and client-specific configuration.

| Build                                   | Supported client version |
|-----------------------------------------|--------------------------|
| [Mozart.Encore](Source/Mozart/)         | v3.10 (O2Jam Original)   |
| [Amadeus.Encore](Source/Amadeus/)       | v3.82 (O2Jam NX)         |
| [CrossTime.Encore](Source/CrossTime/)   | v2.33 (O2Jam X2)         |
| [Identity.Encore](Source/Identity/)     | v5.89 (O2JamO2 Beta)     |
| [IdentityP2.Encore](Source/IdentityP2/) | v5.89 (O2JamO2 Final)    |
| [Memoryer.Encore](Source/Memoryer/)     | v8.02 (O2Jam Classic)    |

## Features

> [!IMPORTANT]
> This project is free from copyrighted materials. All code is original work written from scratch.  
> No copyrighted game assets, binaries, or master data are distributed in this repository.
>
> You must obtain and provide the required metadata files in order to enable all functionality.  
> See [Metadata files](#metadata) to learn more.

- Zero-Configuration for quick start.
- Full online and local network multiplayer support.
- Complete packet op-code coverage.
- Compatible with multiple SQL database systems.
- Support multi planet and channels deployment.
- Highly customizable with high-level network protocol implementation.

<sub>* FTP and in-game website features are not included.</sub>

## Quick Start

Download and extract the appropriate server binary from [here](https://github.com/SirusDoma/Mozart.Encore/releases/latest).

| Build      | Binary                    | Default client directory                  |
|------------|---------------------------|-------------------------------------------|
| Mozart     | `Mozart.Encore.exe`       | `"C:\Program Files (x86)\e-Games\O2Jam\"` |
| Amadeus    | `Amadeus.Encore.exe`      | `"C:\Program Files (x86)\e-Games\O2Jam\"` |
| CrossTime  | `CrossTime.Encore.exe`    | `"C:\Program Files (x86)\O2Jam\O2JamX2\"` |
| Identity   | `Identity.Encore.exe`     | `"C:\Program Files (x86)\O2Jam\O2JamO2\"` |
| IdentityP2 | `IdentityP2.Encore.exe`   | `"C:\Program Files (x86)\O2Jam\O2JamO2\"` |
| Memoryer   | `Memoryer.Encore.exe`     | `"C:\Program Files (x86)\NOWCOM\O2Jam\"`  |

### First-Time Setup

Extract the archive, import the client metadata from O2Jam directory and register a user using credential of your choice:

```powershell
.\<server>.exe metadata:import "<client-directory>"
.\<server>.exe user:register <username> <password>
```

### Start the Game

Start the server in the first terminal (or double-click it) and leave it running:

```powershell
.\<server>.exe
```

Open a second terminal in the same server directory, then use the same username, password, and client directory to launch the game:

```powershell
.\<server>.exe game:start <username> <password> "<client-directory>"
```

## Project Structure

| Shared project                                                  | Description                                                    |
|-----------------------------------------------------------------|----------------------------------------------------------------|
| [Encore.Framework](Source/Encore/Encore.Framework/)             | TCP/UDP networking, messaging, and hosting framework           |
| [Encore.Server](Source/Encore/Encore.Server/)                   | Shared sessions, services, channels, and room lifecycle logic  |
| [Encore.Data](Source/Encore/Encore.Data/)                       | Shared entities, metadata, options, and repositories           |
| [Encore.CLI](Source/Encore/Encore.CLI/)                         | Shared CLI infrastructure and common commands                  |
| [Encore.Web](Source/Encore/Encore.Web/)                         | Shared HTTP server and authentication/registration endpoints   |

| Project                 | Description                                            |
|-------------------------|--------------------------------------------------------|
| `{Build}`               | Executable, controllers, messages, and CLI tasks       |
| `{Build}.Web`           | HTTP host that compiles the shared `Encore.Web` source |
| `{Build}.Server`        | Build-specific game and protocol logic                 |
| `{Build}.Data`          | Build-specific EF Core data model and metadata parsers |
| `{Build}.Migrations`    | Database migrations organized by provider              |

## Build and Run

Build the complete solution:

```shell
dotnet build Mozart.Encore.sln -c Release -p:Platform=x64
```

Run a server by replacing `<Build>` with one of the build names listed above:

```shell
dotnet run --project Source/<Build>/<Build>
```

# Configuration

Servers can be configured with `config.ini` or command-line arguments.  
Command-line values can be passed using `--Section:Option=Value` syntax.

For example:

```shell
dotnet run --project Source/<Build>/<Build> -- --Server:Port=15010
```

See [Command-line configuration provider](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration-providers#command-line-configuration-provider) to learn more about setting up command-line config.  
Refer to the build's README and `config.ini` for the complete set of options.

## Metadata

Metadata files act as source of truth of particular game data outside the database.  

They are optional for running the server; however, missing certain files may disable one or more features such as play rewards, ranking, and equipment.

Metadata can usually be overridden per channel. Refer to build's README for more information.

> [!IMPORTANT]
> Metadata files must be compatible with the client version supported by the build you are running.  
> Older format versions may work, but are not officially supported and may affect game features.
>
> The server will continue to run even if one or more Metadata files are invalid or use an incompatible format, though the affected features will behave as if the file were not present.

| Option      | Description                                                                                                     |
|-------------|-----------------------------------------------------------------------------------------------------------------|
| `MusicList` | Relative or absolute path to the build's supported music list file (usually `OJNList.dat`, or `X2OJNList.dat`). |
| `ItemData`  | Relative or absolute path to `Itemdata.dat`.                                                                    |
| `AlbumList` | Relative or absolute path to `AlbumList.ojs`, when supported by the build.                                      |

# Database Migration

Use Entity Framework tools to run the database migration.  
See [Entity Framework Core CLI tools](https://learn.microsoft.com/en-us/ef/core/cli/) to learn more about the CLI installation.  

Refer to the build's README for details about adding a new migration or executing the database migration.

>[!IMPORTANT]
> You may notice that the database schema look funky with premature normalizations here and there.  
> This is intentional because the app need to support the existing official database schema.
>
> The table structure represents a best-effort attempt to follow the e-Games database distribution.
> Structures that are known exclusive to the foreign database distribution are omitted.
>
> However, unlike official server app, Mozart will **not** interact with database via Stored Procedure and will execute DML directly.

>[!IMPORTANT]
> Database migration is automatically executed every start-up as long as the `Auth:Mode` equals to `Default`.  
> This is because `Auth:Mode=Foreign` is a compatibility mode that enables Mozart to continue to work with an existing foreign database that has different auth schema than the original e-Games clients (such as 9you or GAMANIA).
>
> Database migration will never be officially supported in `Foreign` mode<sup>*</sup>.
>
> <sub>* The server will likely raise an exception with [`PendingModelChangesWarning`](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/breaking-changes#exception-is-thrown-when-applying-migrations-if-there-are-pending-model-changes) when running database migration with `Foreign` mode.
> The errors can be suppressed, but there's no guarantee that migration will continue to work using foreign auth schema for the future releases.</sub>

# Web Server

Each build includes a lightweight HTTP server for the authentication and registration endpoints implemented by the project. 
Original server files are not included and in-game web functionality is outside the scope of this project.

Endpoint availability and integration requirements vary by build. Refer to the selected build's README for details.

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

## Service Discovery

Most of the time, the command-line arguments specify all available Gateways when launching O2Jam via `OTwo.exe`.  
The syntax of command-lines is vary to each client version. Refer to the build's README to learn more.

Note that some clients may store the gateway addresses directly within the client executable itself.

### Channel

Upon start-up, the `Channel` instances will register themselves to the configured `Gateway` instance via TCP network.
Therefore, the `Gateway` need to be available first. This will allow the `Gateway` instances to discover `Channel`s that available for user to select.

When the `Channel` lost its connection to its `Gateway`, it will automatically shut down itself.

## Advanced scaling

It might be possible to host and scale `Mozart.Encore` in kubernetes via [agones](https://agones.dev/). However, it may require code changes.
Please refer to their [documentation](https://agones.dev/site/docs/) and [third-party examples](https://agones.dev/site/docs/third-party-content/examples/) to learn more.

# CLI Commands

The server applications include utilities for local play and server maintenance. Available commands vary by build.

- `db:migrate`: Execute database migration using the configured database.
- `user:register`: Register a user.
- `user:authorize`: Authorize user credentials and generate the authentication parameters supported by the build.
- `user:equip <username> <item id>`: Equip an item that match with the specified item id. The previously equipped item is moved to the bag.
- `user:stash <username> <item id>`: Add an item that match with the specified item id to a user's bag.
- `user:deposit <username> <gem> [<point>]`: Add gems and points to a user. Point defaults to `0`.
- `metadata:import [<dir>]`: Import supported metadata from an O2Jam installation. Directory defaults to the current working directory.
- `game:start <username> <password> [<dir>]`: Authorize a user and launch the game. Directory defaults to the current working directory.
- `ranking:upsert`: Generate or update user rankings where supported.
- `encdec <param>`: Encrypt or decrypt authentication parameters in Memoryer.

Run the CLI with `--help` for more details.
