# ATTENTION #
<font color='red'>This document is outdated! Please find the full documentation here: <a href='http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/17_Extensions/3_Plugins/WifiRemote'>http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/17_Extensions/3_Plugins/WifiRemote</a></font>


---

# API Documentation #

If you want your client to use WifiRemote to remote control MediaPortal you have to understand the "language" the plugin communicates in.

**This documentation is for developers only. If you just want to use the plugin, you don't need to know all this!**




The WifiRemote language consists of small JSON messages.
A message always consists of a "Type" field (string), which identifies what kind of message is sent.
This is a list of all messages a client can send to WifiRemote and messages WifiRemote sends to the client.



---



## Messages from WifiRemote to the client ##

### welcome ###
The welcome message is sent to every client (socket) that connects to WifiRemote. It contains information about the plugin and about the current status of MediaPortal.

#### Fields ####
  * Type (string): Type of the message, always "welcome"
  * Server\_Version (int): Version of the server. A client can require a certain version of the server, to make sure all required features are supported by the plugin.
  * Status (object): This field contains a [Status message](APIDocumentation#status.md). See below.
  * Volume (object): This field contains a [Volume message](APIDocumentation#volume.md). See below.

Example:
```
{
  "Type":"welcome",
  "Server_Version":2,
  "Status":{
    "Type":"status",
    "IsPlaying":false,
    "IsPaused":false,
    "Title":"",
    "CurrentModule":"Homescreen",
    "SelectedItem":"Music"
  },
  "Volume":{
    "Type":"volume",
    "Volume":100,
    "IsMuted":false"
  }
}
```


### status ###
Describes the current status of MediaPortal, consisting of player and navigation related information.
This is sent when the status changes (a file starts/stops playing, the user navigates in MediaPortal) and integrated in the [welcome message](APIDocumentation#welcome.md).

#### Fields ####
  * Type (string): Type of the message, always "status"
  * IsPlaying (boolean): true if MediaPortal is playing a file, false otherwise
  * IsPaused (boolean): true if MediaPortal is playing a file but it is paused, false otherwise
  * Title (string): Title of the currently played media file (for example "Hot Fuzz")
  * CurrentModule (string): Currently active module (for example "TV Series")
  * SelectedItem (string): Selected item in the skin. This is working a bit flaky at the moment. (for example: "Switch view")

Example:
```
{
  "Type":"status",
  "IsPlaying":false,
  "IsPaused":false,
  "Title":"",
  "CurrentModule":"Homescreen",
  "SelectedItem":"Music"
}
```



### volume ###
A message containing information about MediaPortals volume.
This is sent when there was a change in volume and integrated in the [welcome message](APIDocumentation#welcome.md).

#### Fields ####
  * Type (string): Type of the message, always "volume"
  * Volume (int): Volume set in MediaPortal in percent (0 - 100)
  * IsMuted (boolean): true if the sound in MediaPortal is muted, false otherwise

Example:
```
{
  "Type":"volume",
  "Volume":100,
  "IsMuted":false"
}
```



### plugins ###
The plugins message is sent in response to a [requestplugins message](APIDocumentation#requestplugins.md).
It contains a list of all installed and active window plugins in MediaPortal including the plugin name, the icon as byte array (if requested) and the window id of the plugin

#### Fields ####
  * Type (string): Type of the message, always "plugins"
  * Plugins (array): An array, containing one object per plugin with these fields:
    * Name (string): Name of the plugin in the same language as configured in MediaPortal
    * WindowId (int): Window ID of the plugin. This can be used to activate the plugin via the [window message](APIDocumentation#window.md).
    * Icon (string): This string is a byte-array representation of the plugin icon png file. In the example it is cut to see what's going on. In your client you can get the image from this string and display it.

Example:
```
{
  "Type":"plugins",
  "Plugins":[
    {
      "Name":"Play Disc",
      "WindowId":3001,
      "Icon":"iVBORw0KGgoAAAANSUhEUgAAAIAAAACACAMAAAD04JH5AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAMAUExURQAAAAgHBwsLCxgYGCYmJjAvMDc3N0pJSlBPUFZVVmloaXh4eImJiZaWlpmsvpqytJuwvZ+5sZ+4vaSjpKqkqaurq6+rsKS7rqq8pqm8qqa0vaO5s6C4u6q3uai5vLGqr7yvq7OnsrKrsrqsu7K9pr60q7u/pL64rbOys7W8tLW+vLy2s7qzur65tL29vZurwpuzwaOrxauqw6C3yaW+xqa7z6y0xK+5xaa60au61LSswrqtw7O1xLS7wbq0wry0zL2+wLC917S82Lu51Li82KXAqqvBpqrAq6bBvarEu63KuLLDpLLDrbfIpbDJrbvDpLvEq7/LpL3Lq7…"
    },
    {
      "Name":"Moving Pictures",
      "WindowId":12345,
      "Icon":"…"
    }
  ]
}
```

### properties ###
This message is sent in response to a "properties" request message. It contains the properties (tag and value) of all requested tags. Furthermore, from now on the client will be notified if one of the requested properties changes with a propertychanged message. Only properties that start with #Play or #TV.View are currently supported. See http://wiki.team-mediaportal.com/1_MEDIAPORTAL_1/18_Contribute/7_Skins/Skin_Architecture/Current_File_Tags for a list of supported tags.

#### Fields ####
  * Type (string): Type of the message, always "plugins"
  * Tags (array): An array, containing one object per plugin with these fields:
    * Tag (string): Name of the property
    * Value (string): Value of the property.

### propertychanged ###
The propertychanged  message is sent when one of the tags changes, to which the client has registered for with the properties message.

#### Fields ####
  * Type (string): Type of the message, always "plugins"
  * Tag (string): Name of the property
  * Value (string): Value of the property.

### nowplaying ###
A nowplaying message is sent to all connected clients when playback of a supported file starts.
This message contains information about the file, taken from the corresponding plugins like MP-TVSeries or Moving Pictures. Due to the differences in the plugins the content of the nowplaying message can vary.


#### Fields (global) ####
  * Type (string): Type of the message, always "nowplaying"
  * Duration (int): Duration of the file in seconds
  * Position (int): Current position in the file in seconds
  * File (string): Path of the file
  * MediaInfo (object): An object containing media specific information. You can check which type of information it is with the MediaInfo field "MediaType".

#### Fields (series) ####
  * MediaType (string): Type of MediaInfo, for series always "series"
  * Series (string): Name of the series
  * Season (int): Season of this episode
  * Episode (int): Number of this episode in the season
  * Title (string): Name of the episode
  * AirDate (string): First air date of the episode
  * Director (string): Director(s) of the episode
  * Writer (string): Writer(s) of the episode
  * Genre (string): Genre(s) of the series
  * Image (string): byte-array representation of the season poster image. You can initialize an image from this string in your client. The string is cut in the example
  * MyRating (string): Rating the user gave this episode
  * Plot (string): Plot description of this episode
  * Rating (string): Online rating of this episode
  * RatingCount (string): Number of votes this episode got online
  * Status (string): Status of the series. Can be "Continuing" and "Ended".


Example:
```
{
    "Type":"nowplaying",
    "Duration":2458,
    "Position":0,
    "File":"",
    "MediaInfo":{
        "MediaType":"series",
        "Series":"24",
        "Season":1,
        "Episode":1,
        "Title":"Tag 1: 00:00Uhr-01:00Uhr",
        "AirDate":"2001-11-06",
        "Director":"Stephen Hopkins",
        "Writer":"Robert Cochran, Joel Surnow",
        "Genre":"|Action and Adventure|Drama|",
        "Image":"/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQN…",
        "MyRating":"",
        "Plot":"Stressiger Abend f\U00fcr CTU-Agent Jack Bauer: Seine halbw\U00fcchsige Tochter Kimberly stiehlt sich unerlaubt aus dem Haus. Aber bevor er sich auf die Suche machen kann, wird er dringend ins B\U00fcro zitiert, denn Terroristen planen einen Anschlag auf Senator David Palmer. Es scheint eine lange Nacht f\U00fcr den Agenten zu werden…",
        "Rating":"7.5",
        "RatingCount":"",
        "Status":"Ended"
    }
}
```


#### Fields (moving-pictures) ####
  * MediaType (string): Type of MediaInfo, for moving-pictures always "movies"
  * Title (string): Name of the movie
  * AlternateTitles (string, pipe | separated): Alternate names of the movie (foreign names)
  * Tagline (string): Tagline of the movie
  * Year (int): Release year of the movie
  * Directors (string, pipe | separated): Directors of the movie
  * Writers (string, pipe | separated): Writers of the movie
  * Actors (string, pipe | separated): List of actors in the movie
  * Genres (string, pipe | separated): Genre(s) of the movie
  * Certification (string): Certification of the movie
  * DetailsUrl (string): URL to a website where the movie information was scraped from
  * Image (string): Byte-array representation of the movie poster image. You can initialize an image from this string in your client. The string is cut in the example
  * Rating (string): Online rating of the movie
  * Summary (string): Movie summary


Example:
```
{
    "Type":"nowplaying",
    "Duration":7177,
    "Position":0,
    "File":"";
    "MediaInfo":{
        "MediaType":"movie",
        "Title":"Star Wars",
        "AlternateTitles":"|La guerra de las galaxias|La guerre des \U00e9toiles|",
        "Tagline":"It's Back! (re-release)",
        "Year":1977,
        "Directors":"|George Lucas|",
        "Writers":"|George Lucas|",
        "Actors":"|Mark Hamill|Harrison Ford|Carrie Fisher|Peter Cushing|Alec Guinness|Anthony Daniels|Kenny Baker|Peter Mayhew|David Prowse|James Earl Jones|",
        "Genres":"|Action|Adventure|Fantasy|Sci-Fi|",
        "Certification":"PG",
        "DetailsUrl":"http://www.imdb.com/title/tt0076759",
        "Image":"/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAgGBgcGBQgHBwcJCQgKD…",
        "Rating":"8,8",
        "Summary":"Part IV in a George Lucas epic, Star Wars: A New Hope opens with a rebel ship being boarded by the tyrannical Darth Vader. The plot then follows the life of a simple farmboy, Luke Skywalker, as he and his newly met allies (Han Solo, Chewbacca, Ben Kenobi, C-3PO, R2-D2) attempt to rescue a rebel leader, Princess Leia, from the clutches of the Empire. The conclusion is culminated as the Rebels, including Skywalker and flying ace Wedge Antilles make an attack on the Empires most powerful and ominous weapon, the Death Star."
    }
}
```

### nowplayingupdate ###
A nowplayingupdate message is sent to all connected clients in fixed intervals during playback of a media item.
This message contains information about the current playing state (duration, position, speed,...). It is meant to update a representation of the playback state on the remote end.


#### Fields (global) ####
  * Type (string): Type of the message, always "nowplayingupdate "
  * Duration (int): Duration of the file in seconds
  * Position (int): Current position in the file in seconds
  * Speed (int): Current speed of the player
  * IsTv (bool): True if the currently playing item is a tv stream.

Example:
```
{
    "Type":"nowplayingupdate",
    "Duration":7177,
    "Position":40,
    "Speed":1;
    "IsTv":true
}
```

### onscreenkeyboard ###
A short message informing clients about the MediaPortal onscreen keyboard appearing and disappearing.

#### Fields ####
  * Type (string): Type of the message, always "onscreenkeyboard"
  * IsActive (bool): True if the onscreen keyboard is active, false otherwise

Example:
```
{
    "Type":"onscreenkeyboard",
    "IsActive":true
}
```