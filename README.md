# YuGiOhBot

Discord Bot using Discord.NET library. Deals with YuGiOh material.

## Usage

Note: Capitalization does not matter

Commands for this bot follow this structure: `y! <command> [argument]`.

| Command | Description
|---------|-------------|
|`y!card [card name]` | Searches card based on card name|
|`rcard` | Returns a random card. Great for making random decks|
|`search [search]` | Searches for cards based on search terms given|
|`archetype [search]` | Attemps an archetype search|
|`banlist [1/2/3]` | Returns the current banlist WARNING >> big message|
|`invite` | Sends invite link to dm|
|`info` | Returns information on bot|
|`stats` | Returns stats on the bot|
|`uptime` | Returns the uptime of the bot|
|`ping` | Returns the latency between bot and guild|
|`help` | The defacto help command|
|`feedback [feedback]` | Sends feedback to The One and the Only|
|`minimal [true/false]` | Sets minimal card settings for guild|
|`help` | Brings up help menu|
|`help [command]` | Brings up help for a command based on input|
|`hangman` | play a game of hangman|
|`guess` | guess an image game |

I also have inline declaration of cards. For example, "I like [[blue-eyes]]" will give you a Blue-Eyes card. You can use multiple inline declarations such as "[[red-eyes]] will beat [[blue-eyes]]"!

--------------------------------------------------------

Support guild/server: <https://discord.gg/cVhvrEa>

Only guild ids are stored to save bot specific configuration regarding the bot. As well as aggregating unique user count for temporary statistics calculations.
