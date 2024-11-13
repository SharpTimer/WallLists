<a name="readme-top"></a>

> [!IMPORTANT]
> Credits for the base plugin go to [K4ryuu](https://github.com/K4ryuu)! I made changes to it to work with SharpTimer and added several extra features.

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h1 align="center">SharpTimer Wall Lists</h1>
  <a align="center">Provides three lists to display on the map, a player Points List, and a Map Time List, and a # of Map Completions List.<br>Dynamically updates on either map start or a given interval.</a><br>
  <br>
  <img src="https://files.catbox.moe/mjhgo0.png" alt="" style="margin: 0;">

  <p align="center">
    <br />
    <a href="https://github.com/M-archand/SharpTimer-WallLists/releases/tag/1.0.2">Download</a>
  </p>
</div>

<!-- ABOUT THE PROJECT -->

### Dependencies

To use this server addon, you'll need the following dependencies installed:

- [**CounterStrikeSharp**](https://github.com/roflmuffin/CounterStrikeSharp/releases): CounterStrikeSharp allows you to write server plugins in C# for Counter-Strike 2.
- [**K4-WorldText-API**](https://github.com/K4ryuu/K4-WorldText-API): This is a shared developer API to handle world text.
- [**SharpTimer**](https://github.com/Letaryat/poor-sharptimer): SharpTimer is a timer plugin for game modes such as Surf or BHOP.
  
<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- COMMANDS -->

## Commands

Default Access: @css/root, can be configured.
All commands can be configured, these are the default commands:
- !pointslist - Create a points list in front of the player and save it to a config file.
- !timeslist  - Create a map records list for the current map in front of the player and save it to a config file.
- !completionslist - Create a # of map completions list in fron of the player and save it to a config file.
- !removelist - Remove the closest list from your position. (100 units max)

<!-- CONFIG -->

## Configuration

- A config file will be generated on first use located in _/addons/counterstrikesharp/configs/SharpTimer-WallLists_
- The coordinates are saved in json files, located in _/addons/counterstrikesharp/plugins/SharpTimer-WallLists/maps_
- You can see an example with detailed comments here: [SharpTimer-WallList.example.json](https://github.com/M-archand/SharpTimer-WallLists/blob/WallLists/SharpTimer-WallLists.example.json)
<!-- ROADMAP -->

## Roadmap

- [X] Update for SharpTimer usage.
- [X] Add color configs. See [here](https://i.sstatic.net/lsuz4.png) for color names.
- [X] Add font size & scale to config.
- [X] Add SQLite & PostgreSQL support. // 1=MySQL, 2=SQLite, 3=PostgreSQL
- [X] Merge MapList/PointsList plugins into a single plugin.
- [X] Add database table prefix to config to support latest version of SharpTimer.
- [X] Fix inconsistent results for !removelist
- [X] Add configurabled commands.
- [X] Add configurable permissions.
- [X] Add left alignment.
- [X] Add a Map Completions List.
- [X] Add alignment options for each type of list.
- [ ] Add minor position adjustment commands.
- [ ] Add database support for more convenient multi-server maintencance.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- LICENSE -->

## License

Distributed under the GPL-3.0 License. See `LICENSE.md` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>
