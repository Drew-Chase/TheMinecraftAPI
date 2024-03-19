# The Minecraft API
The Minecraft API is a RESTful API for querying minecraft servers, various modding apis, etc. It is designed to be simple to use and easy to integrate into your application.

# Table of Contents
- [Features](#features)
- [Usage](#usage)
  - [Querying a Minecraft Server](#querying-a-minecraft-server)
  - [Querying Minecraft Versions](#querying-minecraft-versions)

# Features
- Query Minecraft Servers
  - status of a server
  - players on a server
  - server icon
  - server name
  - server description
  - server minecraft version
  - server modded status _(Forge, Fabric, Spigot, etc.)_
  - server mod/plugin list
  - server ping _(latency)_
- Query Minecraft Resources
  - Minecraft Versions
  - Mojang JRE Versions
  - Minecraft Server JARs
  - Get Resource Assets _(textures, sounds, etc.)_
- Query Modding APIs
  - Curse Forge
  - Modrinth
  - ATLauncher
  - Technic
  
# Usage
The Minecraft API is a RESTful API, meaning it uses standard HTTP methods and status codes. The API is designed to be simple to use and easy to integrate into your application.
## Querying a Minecraft Server
This is an example of how to query the minecraft server `hypixel` using the API.
```http request
GET /api/minecraft/server?url=mc.hypixel.net HTTP/1.1
Host: theminecraftapi.com
```
### Response:
```json
{
    "status": "online",
    "name": "HyPixel",
    "description": "",
    "icon": "",
    "minecraft_version": "1.20.4",
    "modded": {
        "loader": "Spigot",
        "mods": [],
        "ping": 4000
    },
    "players": [
        {
            "username": "Steve",
            "UUID": "0000-0000-0000-0000"
        }
    ]
}
```

## Querying Minecraft Versions
```http request
GET https://theminecraftapi.com/api/minecraft/versions?snapshots=true HTTP/1.1
Host: theminecraftapi.com
```
### Response:
```json
{
    "snapshots":[
        {
            "id": "24w11a",
            "latest": true,
            "url": "https://piston-meta.mojang.com/v1/packages/9bc42da3c197d0532c34636d370451b3d09cbba1/24w11a.json",
            "time": "2024-03-14T14:29:10+00:00",
            "releaseTime": "2024-03-14T14:21:33+00:00"
        }
    ],
    "releases":[
        {
            "id": "1.20.4",
            "latest": true,
            "url": "https://piston-meta.mojang.com/v1/packages/3730cb8ba42bd31ca9c7805fd43dbb746c095bc1/1.20.4.json",
            "time": "2024-03-14T06:20:27+00:00",
            "releaseTime": "2023-12-07T12:56:20+00:00"
        }
    ]
}
```