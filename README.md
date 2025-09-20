<a name="readme-top"></a>
[![UpdateManager Compatible](https://img.shields.io/badge/CS2-UpdateManager-darkgreen)](https://github.com/Kandru/cs2-update-manager/)
<!-- PROJECT LOGO -->
<br />
<div align="center">
  <h1 align="center">SharpTimer Wall Lists</h1>
  <a align="center">Provides three lists to display on the map: Times List, Points List, Completions List.<br>Dynamically updates on either map start or a given interval. Supports <a href="https://github.com/Kandru/cs2-update-manager">automatic updates</a>.</a><br>
  <br>
  <img src="https://github.com/user-attachments/assets/7f91b9c2-297a-48ce-8380-c4583aaf12af" alt="" style="margin: 0;">

  <p align="center">
    <br />
    <a href="https://github.com/SharpTimer/WallLists/releases/">Download</a>
  </p>
</div>

<!-- ABOUT THE PROJECT -->

### Dependencies

To use this server addon, you'll need the following dependencies installed:

- [**CounterStrikeSharp**](https://github.com/roflmuffin/CounterStrikeSharp/releases): CounterStrikeSharp allows you to write server plugins in C# for Counter-Strike 2.
- [**SharpTimer**](https://github.com/Letaryat/poor-sharptimer): SharpTimer is a timer plugin for game modes such as Surf or BHOP.
- [**K4-WorldText-API**](https://github.com/M-archand/K4-WorldText-API/releases): This is a shared developer API to handle world text.
- [**CS2MenuManager (optional)**](https://github.com/schwarper/cs2menumanager): This is a shared developer API to handle menus. It's only required if you want to use the list move menu command.

  
<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- COMMANDS -->

## Commands

Default Access: @css/root, can be configured.
All commands can be configured, these are the default commands:
- !tlist  - Create a map records list for the current map in front of the player and save it to a config file or database.
- !plist - Create a points list in front of the player and save it to a config file or database.
- !clist - Create a # of map completions list in fron of the player and save it to a config file or database.
- !rlist - Remove the closest list from your position.
- !mlist - Opens a menu that allows you to adjust menu locations/angles by small increments. Currently only supports lists saved to a database.
- !reloadlistcfg - Reload your config live in-game.
- !updatelistcfg - Update your config with any new variables that have been added in a new version.

<!-- CONFIG -->

## Configuration

- A config file will be generated on first use located in _/addons/counterstrikesharp/configs/SharpTimer-WallLists_
- The coordinates are saved in json files, located in _/addons/counterstrikesharp/plugins/SharpTimer-WallLists/maps_
- You can see an example with detailed comments here: [SharpTimer-WallList.example.json](https://github.com/SharpTimer/WallLists/blob/main/SharpTimer-WallLists.example.json)
<!-- ROADMAP -->

## Roadmap

- [ ] Add command arguments to allow for multiple types of Times lists, e.g. multiple styles, modes, etc.

<p align="right">(<a href="#readme-top">back to top</a>)</p>

> [!IMPORTANT]
> Credits for the base plugin go to [K4ryuu](https://github.com/K4ryuu)! I made changes for it to work with SharpTimer and added more functionality.

<!-- LICENSE -->

## License

Distributed under the GPL-3.0 License. See `LICENSE.md` for more information.

<p align="right">(<a href="#readme-top">back to top</a>)</p>
