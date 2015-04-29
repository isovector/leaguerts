# League

I wrote this in 2007. It's an engine clone of Warcraft 3.

## Project Description
An RTS game engine written entirely in XNA. Designed to provide an interface
similar to that of Blizzard games, and the internal workings are much the same.
Currently supports units, buildings, abilities, a scripting engine, maps, custom
resources and more.

<img src="https://raw.githubusercontent.com/isovector/leaguerts/master/screenshot.png" />

## Map Making

* **Create the map archive:** This step is really easy. In the League/Content/maps/
folder create a new .zip file. Inside, create a new folder called "map/". Create
a new folder called "map/textures/". All of the files you will create in the
next two steps will go into this archive.
* **Design the terrain:** Open up MS Paint or whatever suits your fancy. Paint
probably isn't a good idea, because you need to be able to manipulate different
channels. I use Photoshop. Create a new image the size of your choosing. Every
pixel in the image maps to a 8x16x8 tile in game. The demo map was 33x33 for
some reason. The green channel determines cliff height. The others don't really
do anything right now, but they will later. For every 13 green you put in, the
tile will be one cliff level higher. Save this file and build it with MS Build
to turn it into a Texture .xnb file. Add this .xnb file to the map archive as
"map/terrain.xnb".
* **Create regions:** Regions describe a rectangle on the map in which you can detect
events. The demo map had one at each corner of the units' path. Create a new
text file in the map archive as "map/regions.lrg". Every line in this file will
be parsed as a region - regions should be in the following format:
`{"name",(x,y),(w,h)}`. The name is how you will reference the region in the
script (see the demo map for examples). X,Y defines the top left of the region
with respect to the middle of the map. W,H describes the size of the region.
* **Write the scripts:** In the map archive create a text file called "map/script.l".
You may put any map-specific scripting in this file, and it is invoked before
common.l, so you may unhook any common scripts you want. See Scripting for more
information.
* **Make the textures:** Find (or make) a texture for the terrain and one for the
cliffs. Alpha 1 doesn't show cliffs, but it loads them so it's best to include
the cliff file even if it's nothing. Both files should be Texture .xnb and
should be injected as "map/textures/terrain.xnb" and "map/textures/cliff.xnb"
respectively.
* **Load the map:** Congratulations, your map is now done. It's time to load it and
see if it worked. Load up "League.exe.config" with notepad and find
`<setting name="map" serializeAs="String">`. Change the next "value" node to point to your
map relative to "Content/maps/*.zip". Load up League. Yay! It works!

